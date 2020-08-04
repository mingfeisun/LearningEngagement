using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    private Vector3 target_pos = Vector3.zero;
    private Vector3 target_vel = Vector3.zero;
    private void Start() {
        Reset();
    }

    void Update()
    {
        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = target_vel;
    }

    public void Reset()
    {
        Vector3 init_pos = new Vector3(0.0f, 0.2f, -0.5f);
        SetPosition(init_pos);
    }

    public void SetPosition(Vector3 _pos)
    {
        target_pos = _pos;
        transform.position = target_pos;
    }

    public void SetVelocity(Vector3 _vel)
    {
        target_vel = _vel;
    }
}
