#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class Frame {
    public MotionData Data;
    public int Index;
    public float Timestamp;
    public Matrix4x4[] Local;
    public Matrix4x4[] World;

    public Frame(MotionData data, int index, float timestamp) {
        Data = data;
        Index = index;
        Timestamp = timestamp;
        Local = new Matrix4x4[Data.Source.Bones.Length];
        World = new Matrix4x4[Data.Source.Bones.Length];
    }

    public Frame GetPreviousFrame() {
        return Data.Frames[Mathf.Clamp(Index-2, 0, Data.Frames.Length-1)];
    }

    public Frame GetNextFrame() {
        return Data.Frames[Mathf.Clamp(Index, 0, Data.Frames.Length-1)];
    }

    public Frame GetFirstFrame() {
        return Data.Frames[0];
    }

    public Frame GetLastFrame() {
        return Data.Frames[Data.Frames.Length-1];
    }

    public Matrix4x4[] GetBoneTransformations() {
        List<Matrix4x4> transformations = new List<Matrix4x4>();
        for(int i=0; i<World.Length; i++) {
            if(Data.Source.Bones[i].Active) {
                transformations.Add(GetBoneTransformation(i));
            }
        }
        return transformations.ToArray();
    }

    public Matrix4x4 GetBoneTransformation(int index, int smoothing = 0) {
        if(smoothing  == 0) {
            return Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Data.Scaling * Vector3.one) * World[index] * Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Data.Source.Bones[index].Alignment), Vector3.one); 
        } else {
            Frame[] frames = Data.GetFrames(Mathf.Clamp(Index - smoothing, 1, Data.GetTotalFrames()), Mathf.Clamp(Index + smoothing, 1, Data.GetTotalFrames()));
            Vector3 P = Vector3.zero;
            Vector3 Z = Vector3.zero;
            Vector3 Y = Vector3.zero;
            float sum = 0f;
            for(int i=0; i<frames.Length; i++) {
                float weight = 2f * (float)(i+1) / (float)(frames.Length+1);
                if(weight > 1f) {
                    weight = 2f - weight;
                }
                Matrix4x4 matrix = frames[i].World[index];
                P += weight * (Vector3)matrix.GetColumn(3);
                Z += weight * (Vector3)matrix.GetColumn(2).normalized;
                Y += weight * (Vector3)matrix.GetColumn(1).normalized;
                sum += weight;
            }
            P /= sum;
            Z /= sum;
            Y /= sum;
            return Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Data.Scaling * Vector3.one) * Matrix4x4.TRS(P, Quaternion.LookRotation(Z, Y), Vector3.one);
        }
    }

    public Vector3[] GetBoneVelocities() {
        List<Vector3> velocities = new List<Vector3>();
        for(int i=0; i<World.Length; i++) {
            if(Data.Source.Bones[i].Active) {
                velocities.Add(GetBoneVelocity(i));
            }
        }
        return velocities.ToArray();
    }

    public Vector3 GetBoneVelocity(int index) {
        if(Index == 1) {
            return GetNextFrame().GetBoneVelocity(index);
        } else {
            return (GetBoneTransformation(index).GetColumn(3) - GetPreviousFrame().GetBoneTransformation(index).GetColumn(3)) * Data.Framerate;
        }
    }

    public Matrix4x4 GetRootTransformation() {
        return Matrix4x4.TRS(GetRootPosition(), GetRootRotation(), Vector3.one);
    }

    public Vector3 GetRootPosition() {
        return Utility.ProjectGround(GetBoneTransformation(0, Data.RootSmoothing).GetColumn(3), Data.Ground);
    }

    public Quaternion GetRootRotation() {
        Vector3 forward = GetBoneTransformation(0, Data.RootSmoothing).GetColumn(2).normalized;

        forward = Quaternion.FromToRotation(Vector3.forward, Data.GetAxis(Data.ForwardAxis)) * forward;

        forward.y = 0f;
        return Quaternion.LookRotation(forward.normalized, Vector3.up);
    }

    public Vector3 GetRootVelocity() {
        if(Index == 1) {
            return GetNextFrame().GetRootVelocity();
        } else {
            Vector3 velocity = (GetBoneTransformation(0, Data.RootSmoothing).GetColumn(3) - GetPreviousFrame().GetBoneTransformation(0, Data.RootSmoothing).GetColumn(3)) * Data.Framerate;
            velocity.y = 0f;
            return velocity;
        }
    }

    public float GetSpeed() {
        float length = 0f;
        Vector3[] positions = new Vector3[6];
        positions[0] = GetRootPosition();
        positions[0].y = 0f;
        for(int i=1; i<=5; i++) {
            Frame future = Data.GetFrame(Mathf.Clamp(Timestamp + (float)i/5f, 0f, Data.GetTotalTime()));
            positions[i] = future.GetRootPosition();
            positions[i].y = 0f;
        }
        for(int i=1; i<=5; i++) {
            length += Vector3.Distance(positions[i-1], positions[i]);
        }
        return length;
    }

}
#endif