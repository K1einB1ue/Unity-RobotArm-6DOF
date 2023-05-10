using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class KJointInfo
{
    public Vector3 linkDir;
    public float linkTwist;
    public float linkLength;
    public float linkOffset;
    public float jointAngle;
    
    public void rotateUpdate(){}
    public void moveUpdate(){}
}

public class KJointController : MonoBehaviour
{
    private static float AxisLength = 10;
    public KJointController prevJoint;
    public KJointController nextJoint;
    public GameObject jointBall;
    public GameObject jointCylinder;
    public Vector3 rotationAxis = Vector3.up;
    public Quaternion defaultRotation = Quaternion.Euler(0,0,0);
    public float rotation;
    public float length;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    private void OnDrawGizmosSelected()
    {
        var position = transform.position;
        JointDisplay();
        JointRotationUpdate();
        JointPositionUpdate(position);
    }

    private void JointDisplay()
    {
        //计算旋转轴
        var axis = rotationAxis.normalized;
        //当前的变换对象
        var trans = transform;
        //旋转轴在变换后的方向
        axis = trans.rotation * axis;
        var position = trans.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(position-axis*AxisLength,position+axis*AxisLength); 
    }
    
    private void JointPositionUpdate(Vector3 rootPosition)
    {
        //圆柱体的变换对象
        var cylinderTrans = jointCylinder.transform;
        //圆柱体的局部缩放
        var cylinderScale = cylinderTrans.localScale;
        cylinderScale.z = length;
        cylinderTrans.localScale = cylinderScale;
        transform.position = rootPosition;
        if (jointBall)
        {
            var ballTrans = jointBall.transform;
            var ballPosLocal = ballTrans.localPosition;
            ballPosLocal.y = length / 50;
            ballTrans.localPosition = ballPosLocal;
            var ballPos = ballTrans.position;
            if (nextJoint)
            {
                nextJoint.JointPositionUpdate(ballPos);
            }
        }
    }

    private void JointRotationUpdate()
    {
        var defaultR = defaultRotation.normalized;
        var parR = transform.parent;
        if (prevJoint)
        {
            var invR = Quaternion.Inverse(prevJoint.defaultRotation);
            Debug.Log(invR);
            if (parR)
            {
                defaultR = parR.rotation * invR * defaultR;
            }
            else
            {
                throw new Exception();
            }
        }
        else
        {
            if (parR)
            {
                defaultR = parR.rotation * defaultR;
            }
        }
        

        transform.rotation = defaultR*Quaternion.AngleAxis(rotation, rotationAxis);
        if (nextJoint)
        {
            nextJoint.JointRotationUpdate();
        }
    }
}


