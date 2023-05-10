using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.HighDefinition;
using UnityEngine;

public class KoildJoint : MonoBehaviour
{
    public bool displayAxis;
    public bool displayJointSpace;
    public bool displayOffset;

    public float defaultAngle;
    public Vector3 rotationAxis;
    public Vector3 axisOffset;
    private void OnDrawGizmosSelected()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    public Vector3 Localize(Vector3 v)
    {
        var rotation = transform.rotation;
        var uY = rotation * Vector3.up;
        var uX = rotation * Vector3.right;
        var uZ = rotation * Vector3.forward;
        var vecRet = new Vector3(uX.Dot(v), uY.Dot(v), uZ.Dot(v));
        return vecRet;
    }

    public Quaternion Localize(Quaternion q)
    {
        return Quaternion.Inverse(transform.rotation) * q;
    }
    
    public KoildJoint Prev()
    {
        var parent = transform.parent;
        if (!parent) return null;
        return !TryGetComponent<KoildJoint>(out var joint) ? null : joint;
    }
    
    public KoildJoint Next()
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent<KoildJoint>(out var joint)) return joint;
        }
        return null;
    }
}
