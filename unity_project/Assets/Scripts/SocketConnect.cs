using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Threading;

public class SocketConnect : MonoBehaviour
{
    #region private members
    private TcpClient socketConnection;
    private Thread clientReceiveThread;
    private Actor human;
    private PredictedDeforms predictedDeforms;
    private StreamWriter fileWriter = new StreamWriter("");
    private Queue<int> numFramesQueue = new Queue<int>();
    private Queue<Vector3[]> ballInitQueue = new Queue<Vector3[]>();
    private Queue<Vector3[]> clothStateQueue = new Queue<Vector3[]>();
    private int numFramesToSend = 0;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        human = GetComponent<Actor>();
        ConnectToTcpServer();
    }

    // Update is called once per frame
    void Update()
    {
        if(numFramesToSend > 0)
        {
            // SendMessage();
            numFramesToSend--;
        }
        if(numFramesToSend == 0)
        {
            // SendMessage();
            numFramesToSend = 100;
            // SendNull();
        }
    }

    void LateUpdate() {
        if (numFramesToSend == 0 && numFramesQueue.Count != 0)
        {
            numFramesToSend = numFramesQueue.Dequeue();
            var tmpAction = ballInitQueue.Dequeue();
            // charController.SetPosition(tmpAction[0]);
            // charController.SetVelocity(tmpAction[1]);
            Debug.Log("Frames to send: " + numFramesToSend);
            Debug.Log("Set ball to state: " + tmpAction[0].ToString("F3") + tmpAction[1].ToString("F3"));
        }
        if (clothStateQueue.Count != 0)
        {
            var tmpVertices = clothStateQueue.Dequeue();
            predictedDeforms.SetState(tmpVertices);
        }
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
                        Vector3[] dataVec = new Vector3[0]; // empty
                        string prefix = "";
                        var numFrames = ByteDecode(incommingData, ref dataVec, ref prefix);
                        if(numFrames != 0 && prefix.Equals("ACTION"))
                        {
                            numFramesQueue.Enqueue(numFrames);
                            ballInitQueue.Enqueue(dataVec);
                        }
                        else if(numFrames == 0 && prefix.Equals("STATE"))
                        {
                            clothStateQueue.Enqueue(dataVec);
                        }
                    }
                }
            }
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void SendMessage() {
        if (socketConnection == null) {
            return;
        }
        try {
            // Get a stream object for writing.
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite) {
                byte[] stateByteArray = GetStateEncode();

                Debug.Log("Num of bytes: " + stateByteArray.Length);
                stream.Write(stateByteArray, 0, stateByteArray.Length);
                // Debug.Log("State sent!");
            }
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private void SendNull() {
        if (socketConnection == null) {
            return;
        }
        try {
            // Get a stream object for writing.
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite) {
                byte[] nullByteArray = Encoding.ASCII.GetBytes("NULL");
                stream.Write(nullByteArray, 0, nullByteArray.Length);
            }
        }
        catch (SocketException socketException) {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    private byte[] GetStateEncode()
    {
        var tmpCloth = human.GetComponent<Cloth>();
        Vector3[] verticies = tmpCloth.vertices;
        // Debug.Log("Num of verticies: " + verticies.Length);
        string stateMsg="#"; // start with symbol #
        for(int i = 0; i < verticies.Length; i++)
        {
            var tmpStr = verticies[i].ToString("F3");
            stateMsg = string.Concat(stateMsg, tmpStr);
            stateMsg = string.Concat(stateMsg, "#"); // end with symbol #
        }
        // Convert string message to byte array.
        byte[] data = Encoding.ASCII.GetBytes(stateMsg);
        return data;
    }

    private int ByteDecode(byte[] _ac, ref Vector3[] _data, ref string prefix)
    {
        string dataMsg = Encoding.ASCII.GetString(_ac);
        // Debug.Log("Received action: " + dataMsg);
        string[] dataStrs = dataMsg.Split('#'); // only the first action is used
        Debug.Log("Received action length: " + dataStrs.Length);
        prefix = dataStrs[0];
        _data = new Vector3[dataStrs.Length - 3];
        for(int i = 0; i < _data.Length; i++)
        {
            _data[i] = string2vector(dataStrs[i+1]);
        }
        return int.Parse(dataStrs[dataStrs.Length-2]);
    }

    private Vector3 string2vector(string _strInput)
    {
        if (_strInput.StartsWith ("(") && _strInput.EndsWith (")")) {
            _strInput = _strInput.Substring(1, _strInput.Length-2);
        }
 
        // split the items
        string[] sArray = _strInput.Split(',');
 
        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));
 
        return result;
    }
}
