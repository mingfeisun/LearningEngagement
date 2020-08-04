using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredictedDeforms : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] state = new Vector3[0];

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Update()
    {
        if(state.Length == mesh.vertices.Length){
            mesh.vertices = state;
            // Debug.Log("Vertex number: " + mesh.vertices.Length);
            mesh.RecalculateNormals();
        }
    }

    public void SetState(Vector3[] _state)
    {
        state = _state;
    }
}
