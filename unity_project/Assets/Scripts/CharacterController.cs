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
    private int recvCount = 0;
    private int offsetCount = 0;
    private bool isUpdated = false;
    private volatile Queue recvBuffer = new Queue();
    private object _asyncLock = new object();
    private Dictionary<String, Vector3> skinTrans = new Dictionary<String, Vector3>();
    private Dictionary<String, Vector3> animTrans = new Dictionary<String, Vector3>();
    private Dictionary<String, Quaternion> skinRot = new Dictionary<String, Quaternion>();
    private Dictionary<String, Quaternion> alignment = new Dictionary<String, Quaternion>();
    

    void Reset() {
    }

    void Awake() {
        actor = GetComponent<Actor>();
    }

    void Start() { 
        offsetTf = GameObject.Find("human").transform;
        motionData = (MotionData)AssetDatabase.LoadMainAssetAtPath("Assets/Motions/dance-mb2.bvh.asset");
        RecordInitPose();
        ConnectToTcpServer();
        //ConstructMapping();
    }

    void RecordInitPose(){
        for(int i=0; i<actor.Bones.Length; i++) {
            var bone = actor.Bones[i];
            String bone_name = bone.GetName();
            Vector3 bone_local_trans = bone.Transform.localPosition;
            Quaternion bone_local_rot = bone.Transform.localRotation; 
            skinTrans.Add(bone_name, bone_local_trans);
            skinRot.Add(bone_name, bone_local_rot);
        }
    }

    void Update() {
        // UpdateAlignment();
        UpdateFromQueue();
        // UpdateFromMotionData();
    }

    void UpdateFromQueue() {
        Vector3 trans = new Vector3();
        Quaternion quat = new Quaternion(); 
        Vector3 offset = new Vector3();
        for (int i =0 ; i< 50; i++){
            lock(_asyncLock){
                if (recvBuffer.Count > 0){
                    String recv = (String)recvBuffer.Dequeue();
                    var bone_name = DecodeData(recv, ref trans, ref quat, ref offset);
                    if (bone_name == "LeftHandIndex1"){
                        bone_name = "LeftHandFinger1";
                    }
                    else if (bone_name == "RightHandIndex1"){
                        bone_name = "RightHandFinger1";
                    }
                    if (offsetCount < 100){
                        try {
                            animTrans.Add(bone_name, offset);
                        }
                        catch (ArgumentException) { }
                        offsetCount ++;
                    }
                    var bone = actor.FindBone(bone_name);
                    bone.Transform.localPosition = trans;
                    bone.Transform.localRotation = quat;
                    // Vector3 new_trans = new Vector3(trans.x, trans.y, -trans.z);
                    // Quaternion new_quat = new Quaternion(-quat.x, -quat.y, quat.z, quat.w);
                    // if (alignment.Count > 0){
                    //     bone.Transform.localPosition = new_trans;
                    //     bone.Transform.localRotation = new_quat;
                    // }
                    // else {
                    //     bone.Transform.localPosition = new_trans;
                    //     bone.Transform.localRotation = new_quat;
                    // }
                }
            }
        }
    }

    void UpdateAlignment(){
        if (recvCount < 100 || offsetCount < 100){
            return;
        }
        if (isUpdated){
            return;
        }
        for(int i=0; i<actor.Bones.Length; i++) {
            var bone = actor.Bones[i];
            var bone_name = bone.GetName();

            var skin_arrow = bone.Transform.localPosition;
            var anim_arrow = animTrans[bone_name];
            if (skin_arrow.magnitude == 0 || anim_arrow.magnitude == 0){
                alignment.Add(bone_name, new Quaternion(0, 0, 0, 1));
                continue;
            }

            Vector3 axis = Vector3.Cross(anim_arrow, skin_arrow);
            float angle = Mathf.Asin(axis.magnitude / (skin_arrow.magnitude * anim_arrow.magnitude) );
            Debug.Log("axis");
            Debug.Log(axis.x);
            Debug.Log(axis.y);
            Debug.Log(axis.z);
            Debug.Log("angle");
            Debug.Log(angle);
            Quaternion rot = Quaternion.AngleAxis(angle, axis.normalized);
            alignment.Add(bone_name, rot);
        }
        isUpdated = true;
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
                            if (recvCount < 100){
                                recvCount ++;
                            }
                        }
                    }
                }
            }
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private String DecodeData(String _dataMsg, ref Vector3 _trans, ref Quaternion _quat, ref Vector3 _offset)
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

        sArray = dataStrs[3].Split(',');
        _offset.x = float.Parse(sArray[1]);
        _offset.y = float.Parse(sArray[2]);
        _offset.z = float.Parse(sArray[3]);

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
