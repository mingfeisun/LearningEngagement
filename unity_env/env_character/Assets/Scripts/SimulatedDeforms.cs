using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedDeforms : MonoBehaviour
{
    public Vector3[] vertices;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Hello world");
    }

    // Update is called once per frame
    void Update()
    {
        var cloth = GetComponent<Cloth>();
        vertices = cloth.vertices;
        // Debug.Log(vertices.Length);
    }

    public Vector3[] GetState()
    {
        return vertices;
    }
}
