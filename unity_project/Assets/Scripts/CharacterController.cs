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
    private Queue recvBuffer = new Queue();
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
        motionData = (MotionData)AssetDatabase.LoadMainAssetAtPath("Assets/Motions/rumba_dancing.asset");
        //Debug.Log("Number of frames: " + motionData.Frames.Length);
        ConnectToTcpServer();
        ConstructMapping();
    }

    void ConstructMapping() {
        StreamReader inp_stm = new StreamReader("Assets/Scripts/name2index.txt");
        while(!inp_stm.EndOfStream)
        {
            String inp_ln = inp_stm.ReadLine( );
            var sArray = inp_ln.Split(',');
            int index = Int16.Parse(sArray[0]);
            String joint_name = sArray[1];
            Debug.Log("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Debug.Log(joint_name);
            name2index.Add(joint_name, index);
        }
        inp_stm.Close( );  
}

    void Update() {
        UpdateFromQueue();
        //UpdateFromMotionData();
    }

    void UpdateFromQueue() {
        if (recvBuffer.Count > 0){
            Vector3 trans = new Vector3();
            Quaternion quat = new Quaternion(); 
            String recv = (String)recvBuffer.Dequeue();
            Debug.Log(recv);
            var bone_name = DecodeData(recv, ref trans, ref quat);
            Debug.Log(bone_name);
            String output_joint_name = MappingJoint(bone_name);
            Debug.Log(output_joint_name);
            int index = JointCollection.IndexOf(output_joint_name)+2;
            Debug.Log(index);
            //int index = 0;
            //foreach(String key in name2index.Keys)
            //{
                //Debug.Log(String.Format("{0}: {1}", key, name2index[key]));
            //    if(key == output_joint_name){
            //        Debug.Log("??????????????????????????????????????????");
            //        index = (int)name2index[key];
            //    }
            //};
            //Debug.Log(index);
            // int index = (int)name2index[bone_name];
            //System.Random rnd = new System.Random();
            //int index  = rnd.Next(2, 66); 
            //Debug.Log(actor.Bones.Length);
            //Debug.Log(actor.Bones[0]);
            
            //var fuck = "mixamorig1_RightHandThumb1";
            //Debug.Log(name2index[fuck]);
            //int index = (int)name2index[output_joint_name];
            
            
            
            //actor.Bones[index].Transform.position = trans + offsetTf.position;
            //offsetTf.position = actor.Bones[index].Transform.position;
            Debug.Log(offsetTf.position);
            Debug.Log("=================================");
            actor.Bones[index].Transform.position = trans;
            actor.Bones[index].Transform.rotation = quat;




            Debug.Log(bone_name);
            Debug.Log(trans);
            Debug.Log(quat);
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("/home/mingfei/Documents/MaShuai's WorkSpace/LearningEngagement/unity_project/Assets/Scripts/WriteLines2.txt", true))
            {
                file.WriteLine(bone_name);
            }

        }
    }

    private String MappingJoint(String input_joint){
        String output_joint = "";
        if (input_joint == "Hips")  output_joint = "mixamorig1_Hips";
        if (input_joint == "LHipJoint")  output_joint = "mixamorig1_LeftUpLeg";
        if (input_joint == "LeftHip")  output_joint = "mixamorig1_LeftUpLeg";
        if (input_joint == "LeftKnee")  output_joint = "mixamorig1_LeftLeg";
        if (input_joint == "LeftAnkle")  output_joint = "mixamorig1_LeftFoot";
        if (input_joint == "LeftToe")  output_joint = "mixamorig1_LeftToeBase";
        if (input_joint == "RHipJoint")  output_joint = "mixamorig1_RightUpLeg";
        if (input_joint == "RightHip")  output_joint = "mixamorig1_RightUpLeg";
        if (input_joint == "RightKnee")  output_joint = "mixamorig1_RightLeg";
        if (input_joint == "RightAnkle")  output_joint = "mixamorig1_RightFoot";
        if (input_joint == "RightToe")  output_joint = "mixamorig1_RightToeBase";
        if (input_joint == "lowerback")  output_joint = "mixamorig1_Spine";
        if (input_joint == "Chest")  output_joint = "mixamorig1_Spine1";
        if (input_joint == "Chest2")  output_joint = "mixamorig1_Spine2";
        if (input_joint == "lowerneck")  output_joint = "mixamorig1_Neck";
        if (input_joint == "Neck")  output_joint = "mixamorig1_Neck";
        if (input_joint == "Head")  output_joint = "mixamorig1_Head";
        if (input_joint == "LeftCollar")  output_joint = "mixamorig1_LeftShoulder";
        if (input_joint == "LeftShoulder")  output_joint = "mixamorig1_LeftShoulder";
        if (input_joint == "LeftElbow")  output_joint = "mixamorig1_LeftForeArm";
        if (input_joint == "LeftWrist")  output_joint = "mixamorig1_LeftHand";
        if (input_joint == "lhand")  output_joint = "mixamorig1_LeftHandIndex1";
        if (input_joint == "LFingers")  output_joint = "mixamorig1_LeftHandMiddle1";
        if (input_joint == "LThumb")  output_joint = "mixamorig1_LeftHandThumb1";
        if (input_joint == "RightCollar")  output_joint = "mixamorig1_RightHandMiddle1";
        if (input_joint == "RightShoulder")  output_joint = "mixamorig1_RightShoulder";
        if (input_joint == "RightElbow")  output_joint = "mixamorig1_RightForeArm";
        if (input_joint == "RightWrist")  output_joint = "mixamorig1_RightHand";
        if (input_joint == "rhand")  output_joint = "mixamorig1_RightHandIndex1";
        if (input_joint == "RFingers")  output_joint = "mixamorig1_RightHandMiddle1"; 
        if (input_joint == "RThumb")  output_joint = "mixamorig1_RightHandThumb1"; 
        return output_joint;

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
            socketConnection = new TcpClient("localhost", 65432);
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
                        recvBuffer.Enqueue(dataMsg);
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
        String[] dataStrs = _dataMsg.Split('#'); // only the first action is used
        Debug.Log("Received action length: " + dataStrs.Length);

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
            actor.Bones[i].Transform.position = tf.GetPosition();
            actor.Bones[i].Transform.rotation = tf.GetRotation();
        }
    }
}
