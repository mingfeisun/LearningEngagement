using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
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

    void Reset() {
    }

    void Awake() {
        actor = GameObject.Find("human").GetComponent<Actor>();
    }

    void Start() { 
        offsetTf = GameObject.Find("human").transform;
        // motionData = (MotionData)AssetDatabase.LoadMainAssetAtPath("Assets/walk_3dmax.bvh.asset");
        // motionData = (MotionData)AssetDatabase.LoadMainAssetAtPath("Assets/Motions/Brooklyn Uprock.asset");
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
            name2index.Add(joint_name, index);
        }
        inp_stm.Close( );  
}

    void Update() {
        UpdateFromQueue();
    }

    void UpdateFromQueue() {
        if (recvBuffer.Count > 0){
            Vector3 trans = new Vector3();
            Quaternion quat = new Quaternion(); 
            String recv = (String)recvBuffer.Dequeue();
            var bone_name = DecodeData(recv, ref trans, ref quat);

            // int index = (int)name2index[bone_name];
            // actor.Bones[index].Transform.position = trans;
            // actor.Bones[index].Transform.rotation = quat;
            Debug.Log(bone_name);
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
