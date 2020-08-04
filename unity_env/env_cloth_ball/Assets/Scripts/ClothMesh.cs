using System;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

public class ClothMesh : MonoBehaviour
{
    private Mesh mesh;
    // private TcpListener server = null;
    // private Thread serverThread;
    // private Int32 port = 65431;
    // private IPAddress localAddr = IPAddress.Parse("localhost");
    // private Queue<Vector3[]> clothStateQueue = new Queue<Vector3[]>();
    private Vector3[] state = new Vector3[0];
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        // BuildTcpServer();
    }

    // Update is called once per frame
    void Update()
    {
        if(state.Length == mesh.vertices.Length){
            mesh.vertices = state;
            // Debug.Log("Vertex number: " + mesh.vertices.Length);
            mesh.RecalculateNormals();
        }
    }

    // private void BuildTcpServer () {
    //     try {
    //         server = new TcpListener(localAddr, port);
    //         server.Start();
    //         serverThread = new Thread (new ThreadStart(ListenForData));
    //         serverThread.IsBackground = true;
    //         serverThread.Start();
    //     }
    //     catch (Exception e) {
    //         Debug.Log("On client connect exception " + e);
    //     }
    // }

    // private void ListenForData() {
    //     try {
    //         TcpClient client = server.AcceptTcpClient();
    //         Byte[] bytes = new Byte[10240];
    //         Debug.Log("Connection established!"); 		
    //         while (true) {
    //             // Get a stream object for reading
    //             using (NetworkStream stream = client.GetStream()) {
    //                 int length;
    //                 // Read incomming stream into byte arrary.
    //                 while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) {
    //                     var incommingData = new byte[length];
    //                     Array.Copy(bytes, 0, incommingData, 0, length);

    //                     // Vector3[] action = new Vector3[0]; // empty
    //                     // var numFrames = ActionDecode(incommingData, ref action);
    //                     // numFramesQueue.Enqueue(numFrames);
    //                     // ballInitQueue.Enqueue(action);
    //                 }
    //             }
    //         }
    //     }
    //     catch (SocketException socketException) {
    //         Debug.Log("Socket exception: " + socketException);
    //     }
    // }

    public void SetState(Vector3[] _state)
    {
        state = _state;
    }
}
