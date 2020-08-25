using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Actor))]
public class CharacterController : MonoBehaviour {
    private Actor actor = null;
    private MotionData motionData = null;
    private AnimationClip animationClip;
    private float currTime = 0.0f;
    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    private Transform offsetTf;
    private volatile Queue recvBuffer = new Queue();
    private object _asyncLock = new object();
    private Hashtable name2index = new Hashtable();
    List<String> JointCollection = new List<String>{"mixamorig1_Hips", "mixamorig1_LeftUpLeg", "mixamorig1_LeftLeg", "mixamorig1_LeftFoot", "mixamorig1_LeftToeBase", "mixamorig1_LeftToe_End", "mixamorig1_RightUpLeg", "mixamorig1_RightLeg", "mixamorig1_RightFoot", "mixamorig1_RightToeBase", "mixamorig1_RightToe_End", "mixamorig1_Spine", "mixamorig1_Spine1", "mixamorig1_Spine2", "mixamorig1_LeftShoulder", "mixamorig1_LeftArm", "mixamorig1_LeftForeArm", "mixamorig1_LeftHand", "mixamorig1_LeftHandIndex1", "mixamorig1_LeftHandIndex2", "mixamorig1_LeftHandIndex3", "mixamorig1_LeftHandIndex4", "mixamorig1_LeftHandMiddle1", "mixamorig1_LeftHandMiddle2", "mixamorig1_LeftHandMiddle3", "mixamorig1_LeftHandMiddle4", "mixamorig1_LeftHandPinky1", "mixamorig1_LeftHandPinky2", "mixamorig1_LeftHandPinky3", "mixamorig1_LeftHandPinky4", "mixamorig1_LeftHandRing1", "mixamorig1_LeftHandRing2", "mixamorig1_LeftHandRing3", "mixamorig1_LeftHandRing4", "mixamorig1_LeftHandThumb1", "mixamorig1_LeftHandThumb2", "mixamorig1_LeftHandThumb3", "mixamorig1_LeftHandThumb4", "mixamorig1_Neck", "mixamorig1_Head", "mixamorig1_HeadTop_End", "mixamorig1_RightShoulder", "mixamorig1_RightArm", "mixamorig1_RightForeArm", "mixamorig1_RightHand", "mixamorig1_RightHandIndex1", "mixamorig1_RightHandIndex2", "mixamorig1_RightHandIndex3", "mixamorig1_RightHandIndex4", "mixamorig1_RightHandMiddle1", "mixamorig1_RightHandMiddle2", "mixamorig1_RightHandMiddle3", "mixamorig1_RightHandMiddle4", "mixamorig1_RightHandPinky1", "mixamorig1_RightHandPinky2", "mixamorig1_RightHandPinky3", "mixamorig1_RightHandPinky4", "mixamorig1_RightHandRing1", "mixamorig1_RightHandRing2", "mixamorig1_RightHandRing3", "mixamorig1_RightHandRing4", "mixamorig1_RightHandThumb1", "mixamorig1_RightHandThumb2", "mixamorig1_RightHandThumb3", "mixamorig1_RightHandThumb4"};
    //List<string> JointCollection = new List<string>{ "AdditionalCardPersonAdressType", "mixamorig1_Hips"};


    void Reset() {
    }

    void Awake() {
        //actor = GameObject.Find("human").GetComponent<Actor>();
        actor = GetComponent<Actor>();
        //actor = GameObject.Find("mixamorig1_Hips").GetComponent<Actor>();
        Debug.Log("Call the Awake()");
    }

    void Start() { 
        offsetTf = GameObject.Find("human").transform;
        // motionData = (MotionData)AssetDatabase.LoadMainAssetAtPath("Assets/walk_3dmax.bvh.asset");
        // motionData = (MotionData)AssetDatabase.LoadMainAssetAtPath("Assets/Motions/Brooklyn Uprock.asset");
        // motionData = (MotionData)AssetDatabase.LoadMainAssetAtPath("Assets/Motions/rumba_dancing.asset");
        motionData = (MotionData)AssetDatabase.LoadMainAssetAtPath("Assets/Motions/dance-max.bvh.asset");
        //Debug.Log("Number of frames: " + motionData.Frames.Length);
        ConnectToTcpServer();
        //ConstructMapping();
    }

    void ConstructMapping() {
        StreamReader inp_stm = new StreamReader("Assets/Scripts/name2index.txt");
        while(!inp_stm.EndOfStream)
        {
            String inp_ln = inp_stm.ReadLine( );
            var sArray = inp_ln.Split(',');
            int index = Int16.Parse(sArray[0]);
            String joint_name = sArray[1];
            name2index.Add(joint_name, index);
        }
        inp_stm.Close( );  
}

    void Update() {
        // UpdateFromQueue();
        UpdateFromMotionData();
    }

    void UpdateFromQueue() {
        Vector3 trans = new Vector3();
        Quaternion quat = new Quaternion(); 
        for (int i =0 ; i< 50; i++){
            lock(_asyncLock){
                if (recvBuffer.Count > 0){
                    String recv = (String)recvBuffer.Dequeue();
                    var bone_name = DecodeData(recv, ref trans, ref quat);
                    if (bone_name == "LeftHandIndex1"){
                        bone_name = "LeftHandFinger1";
                    }
                    else if (bone_name == "RightHandIndex1"){
                        bone_name = "RightHandFinger1";
                    }
                    var bone = actor.FindBone(bone_name);
                    bone.Transform.position = trans * 2;
                    bone.Transform.rotation = quat;
                }
            }
        }
    }

    void UpdateFromMotionData() {
        currTime += Time.deltaTime;
        if (currTime > motionData.GetTotalTime()){
            currTime = 0.0f;
        }
        var frame = motionData.GetFrame(currTime);
        Animate(frame);
    }

    private void ConnectToTcpServer () {
        try {
            clientReceiveThread = new Thread (new ThreadStart(ListenForData));
            clientReceiveThread.IsBackground = true;
            clientReceiveThread.Start();
        }
        catch (Exception e) {
            Debug.Log("On client connect exception " + e);
        }
    }

    private void ListenForData() {
        try {
            socketConnection = new TcpClient("localhost", 65431);
            socketConnection.SendBufferSize = 204800;
            socketConnection.ReceiveBufferSize = 204800;
            Byte[] bytes = new Byte[socketConnection.ReceiveBufferSize];
            Debug.Log("Connection established!");
            while (true) {
                // Get a stream object for reading
                using (NetworkStream stream = socketConnection.GetStream()) {
                    int length;
                    // Read incomming stream into byte arrary.
                    while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) {
                        var incommingData = new byte[length];
                        Array.Copy(bytes, 0, incommingData, 0, length);
                        String dataMsg = Encoding.ASCII.GetString(incommingData);
                        lock(_asyncLock){
                            recvBuffer.Enqueue(dataMsg);
                        }
                    }
                }
            }
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private String DecodeData(String _dataMsg, ref Vector3 _trans, ref Quaternion _quat)
    {
        // e.g., LeftHandIndex1#(,0.0442,-0.0000,0.0000,)#(,0.0000,0.0000,-0.0621,0.9981,)
        String[] dataStrs = _dataMsg.Split('#'); // only the first action is used

        var bone_name = dataStrs[0];
        String[] sArray = dataStrs[1].Split(',');
        _trans.x = float.Parse(sArray[1]);
        _trans.y = float.Parse(sArray[2]);
        _trans.z = float.Parse(sArray[3]);

        sArray = dataStrs[2].Split(',');
        _quat.x = float.Parse(sArray[1]);
        _quat.y = float.Parse(sArray[2]);
        _quat.z = float.Parse(sArray[3]);
        _quat.w = float.Parse(sArray[4]);

        return bone_name;
    }

    private void Animate(Frame frame) {
        for(int i=0; i<actor.Bones.Length; i++) {
            var tf = frame.GetBoneTransformation(i);
            actor.Bones[i].Transform.position = tf.GetPosition()*7;
            actor.Bones[i].Transform.rotation = tf.GetRotation();
        }
    }
}
