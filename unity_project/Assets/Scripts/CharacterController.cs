using UnityEngine;
using System;
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
    private Transform offsetTf;

    void Reset() {
    }

    void Awake() {
        // actor = GameObject.Find("character_simulated/mixamorig1_Hips").GetComponent<Actor>();
        actor = GetComponent<Actor>();
        // initPose = transform.localToWorldMatrix;

    }

    void Start() { 
        // offsetTf = GameObject.Find("character_simulated").transform;
        // Debug.Log("Position: " + offsetTf.position + ", Rotation: " + offsetTf.rotation);

        animationClip = (AnimationClip)AssetDatabase.LoadAssetAtPath("Assets/Motions/rumba_dancing.fbx", typeof(AnimationClip));
        // Debug.Log("Actor: ", actor);
        // motionData = (MotionData)AssetDatabase.LoadMainAssetAtPath("Assets/Motions/rumba_dancing.asset");
        // Debug.Log("Number of frames: " + motionData.Frames.Length);
        // Debug.Log("Root quaternion: " + frames.GetRootRotation());

        // destination+"/"+data.name+".asset";
        // Utility.SetFPS(60);
        // for(int i=0; i<Actor.Bones.Length; i++) {
        //     Debug.Log(Actor.Bones[i].GetName());
        // }
    }

    void Update() {
        currTime += Time.deltaTime;

        // if (currTime > motionData.GetTotalTime()){
        //     currTime = 0.0f;
        // }
        // currTime = Time.realtimeSinceStartup;

        if (currTime > animationClip.length){
            currTime = 0.0f;
        }
        animationClip.SampleAnimation(actor.gameObject, currTime);
        // Debug.Log("Updating");

        // GameObject.Find("character_simulated").transform.position += offsetTf.position;

        // actor.transform.rotation *= offsetTf.rotation;

        // while(currTime > motionData.GetTotalTime()){
        //     currTime -= motionData.GetTotalTime();
        // }
        // var frame = motionData.GetFrame(currTime);
        // Animate(frame);
    }

    void LoadMocap(){
    }

    private void Animate(Frame frame) {
        //Assign Posture
        // transform.position = Trajectory.Points[RootPointIndex].GetPosition();
        // transform.rotation = Trajectory.Points[RootPointIndex].GetRotation();

        var tf = frame.GetBoneTransformation(0);
        actor.Bones[0].Transform.position = tf.GetPosition() + offsetTf.position;
        actor.Bones[0].Transform.rotation = tf.GetRotation();

        for(int i=1; i<actor.Bones.Length; i++) {
            tf = frame.GetBoneTransformation(i-1);
            actor.Bones[i].Transform.position = tf.GetPosition() + offsetTf.position;
            actor.Bones[i].Transform.rotation = tf.GetRotation();
        }
        // actor.Bones[0].Transform.rotation *= offsetTf.rotation;
        // GameObject.Find("charater_simulated").transform.rotation = offsetTf.rotation;
        // actor.GetRoot().rotation *= offsetTf.rotation;
        // actor.GetRoot().rotation = Quaternion.AngleAxis(180, transform.up);
    }
}