using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;



//这是一个特化的IK形式,并不具有通用性
[ExecuteAlways]
public class SolidIK : MonoBehaviour
{
    public float l0;
    public float l1;
    public float l2;
    public float l3_5;
    public float index;

    public SolidJoint joint0;
    public SolidJoint joint1;
    public SolidJoint joint2;
    public SolidJoint joint3;
    public SolidJoint joint4;
    public SolidJoint joint5;
    

    public GameObject target;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public Quaternion testRotation;
    public Vector3 testVec3;
    private void OnDrawGizmosSelected()
    {
        var rm = new SolidMath.RotationMatrix(testRotation);
        var testV3FromQuat = testRotation * testVec3;
        var testV3FromRm = rm * testVec3;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero, testV3FromQuat);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.zero, testV3FromRm);
        Debug.Log($"{testRotation.w},{testRotation.x},{testRotation.y},{testRotation.z}\n{testV3FromQuat}");
    }

    void Update()
    {
        if (!target) return;
        Reach(target.transform.position, target.transform.rotation);
    }

    public void Reach(Vector3 position,Quaternion rotation)
    {
        //获得根节点相对位置
        //var localPosition = position - joint0.transform.position;
        Vector3 offset;
        Vector3 target;
        //使用条件约束最后三个关节
        offset = rotation * (Vector3.up * l3_5 * index);
        target = position - offset;
        Debug.DrawLine(target,target+Vector3.up*10);
        var localizedJoint0 = joint0.Localize(target,false);
        //约束前三关节
        var joint0Angle = math.atan2(localizedJoint0.x, localizedJoint0.z);
        var localizedJoint12 = joint1.Localize(target,true);
        
        var (angle0, angle1, angle2) = SolidMath.GetRrrAnglePosition(localizedJoint0.x, localizedJoint0.z,
            localizedJoint12.y, localizedJoint12.z, l0 * index, l1 * index, l2 * index);
        joint0.SetJointAngle(angle0);
        joint1.SetJointAngle(angle1);
        joint2.SetJointAngle(angle2);
        var localizedRotation = joint2.Localize(rotation);
        var (angle3,angle4,angle5) = SolidMath.GetRrrAngleZyzRotation(localizedRotation);
        joint3.SetJointAngle(angle3);
        joint4.SetJointAngle(angle4);
        joint5.SetJointAngle(angle5);
    }
    
}
