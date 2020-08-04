using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothReal : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3[] vertices;
    void Start()
    {
        Debug.Log("Hello world");
    }

    // Update is called once per frame
    void Update()
    {
        var cloth = GetComponent<Cloth>();
        vertices = cloth.vertices;
    }

    public Vector3[] GetState()
    {
        return vertices;
    }
}
