using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RotationTest : MonoBehaviour
{
    public Transform other;
    public Vector3 angle;
    private KMath.RotationMatrix rm;
    public float time;
    public Vector3 v0;
    public Vector3 v1;
    public Vector3 axis;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        var pos = transform.position;
        Gizmos.color = Color.white;
        Gizmos.DrawLine(pos, pos + axis);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + v0);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + v1);
        var slerp = new KMath.Vector3Slerp(pos + v0, pos + v1, axis, pos);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, slerp.Sample(time));

        //rm = new KMath.RotationMatrix(transform.rotation);
        //Debug.Log(rm);
    }

    public void Check()
    {
        Debug.Log((Quaternion.Inverse(other.rotation) * transform.rotation).eulerAngles);
    }
}

[CustomEditor(typeof(RotationTest))]
public class RotationTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var rt = target as RotationTest;
        base.OnInspectorGUI();
        if (GUILayout.Button("旋转"))
        {
            if (rt != null)
            {
                var rotation = rt.transform.rotation;
                var z = rotation * Vector3.forward;
                var y = rotation * Vector3.up;
                var x = rotation * Vector3.right;
                var rotZ = Quaternion.AngleAxis(rt.angle.z, z);
                var rotY = Quaternion.AngleAxis(rt.angle.y, y);
                var rotX = Quaternion.AngleAxis(rt.angle.x, x);
                rotation = rotX * rotation;
                rotation = rotY * rotation;
                rotation = rotZ * rotation;
                rt.transform.rotation = rotation;
            }

            
        }
        
        if (GUILayout.Button("Check"))
        {
            if (rt != null)
            {
                rt.Check();
            }
        }
    }

}