using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class KRRRRRRJointController : MonoBehaviour
{
    public KJointController root;
    public KJointController pos0;
    public KJointController pos1;
    public KJointController rot0;
    public KJointController rot1;
    public KJointController rot2;
    

    public float LengthAllSum => pos0.length + pos1.length + rot0.length + rot1.length + rot2.length;
    public float LengthPosSum => pos0.length + pos1.length;
    public float LengthRotSum => rot0.length + rot1.length + rot2.length;

    public void TryReach(Vector3 position,Quaternion rotation)
    {
        var lengthAllSum = LengthAllSum;
        var lengthPosSum = LengthPosSum;
        var posRoot = root.jointBall.transform.position;
        var lengthReach = Vector3.Distance(posRoot, position);
        if (lengthReach > lengthAllSum)
        {
            Debug.Log("Cant reach!");
            return;
        }

        if (lengthReach > lengthPosSum)
        {
            //确定Pos需要达到的地点.
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}

[CustomEditor(typeof(KRRRRRRJointController))]
public class KRRRRRRJointControllerEditor : Editor
{
    private bool _initFlag = false;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var jointController = target as KRRRRRRJointController;
        if (jointController == null) return;
        if (GUILayout.Button("Init"))
        {
            if(!jointController.root) { Debug.LogError("No root!"); return; }
            if(!jointController.pos0) { Debug.LogError("No pos0!"); return; }
            if(!jointController.pos1) { Debug.LogError("No pos1!"); return; }
            if(!jointController.rot0) { Debug.LogError("No rot0!"); return; }
            if(!jointController.rot1) { Debug.LogError("No rot1!"); return; }
            if(!jointController.rot2) { Debug.LogError("No rot2!"); return; }
            
            jointController.root.nextJoint = jointController.pos0;
            jointController.pos0.nextJoint = jointController.pos1;
            jointController.pos1.nextJoint = jointController.rot0;
            jointController.rot0.nextJoint = jointController.rot1;
            jointController.rot1.nextJoint = jointController.rot2;
            jointController.rot2.nextJoint = null;

            jointController.root.prevJoint = null;
            jointController.pos0.prevJoint = jointController.root;
            jointController.pos1.prevJoint = jointController.pos0;
            jointController.rot0.prevJoint = jointController.pos1;
            jointController.rot1.prevJoint = jointController.rot0;
            jointController.rot2.prevJoint = jointController.rot1;
            
            jointController.root.rotationAxis = Vector3.up;
            jointController.pos0.rotationAxis = Vector3.right;
            jointController.pos1.rotationAxis = Vector3.right;
            jointController.rot0.rotationAxis = Vector3.up;
            jointController.rot1.rotationAxis = Vector3.right;
            jointController.rot2.rotationAxis = Vector3.up;
            
        }
    }
}
