using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using UnityEditor;




public class KRobot6Dof : KRobot
{
    public override int RobotSize => 6;
    public Transform target;
    public KJoint joint0;
    public KJoint joint1;
    public KJoint joint2;
    public KJoint joint3;
    public KJoint joint4;
    public KJoint joint5;

    public bool initFlag;
    public Vector3 joint0Off;
    public Vector3 joint1Off;
    public Vector3 joint2Off;
    public Vector3 joint3Off;
    public Vector3 joint4Off;
    public Vector3 toolOffset;


    public Vector3 Joint0Off => joint0.transform.lossyScale.Eff(joint0Off);
    public Vector3 Joint1Off => joint1.transform.lossyScale.Eff(joint1Off);
    public Vector3 Joint2Off => joint2.transform.lossyScale.Eff(joint2Off);
    public Vector3 Joint3Off => joint3.transform.lossyScale.Eff(joint3Off);
    public Vector3 Joint23Off => Joint2Off + Joint3Off;
    public Vector3 Joint4Off => joint4.transform.lossyScale.Eff(joint4Off);
    public Vector3 ToolOffset => joint5.transform.lossyScale.Eff(toolOffset);

    
    public void Update()
    {
        if (Application.isEditor)
        {
            if (initFlag && target)
            {
                if (jacobianMode)
                {
                    JacobiAngleUpdate();
                }
                else
                {
                    AnalyticalUpdate();
                }
            }
        }
    }
    
    public void FixedUpdate()
    {
        if (Application.isPlaying)
        {
            if (initFlag && target)
            {
                if (jacobianMode)
                {
                    JacobiAngleUpdate();
                }
                else
                {
                    AnalyticalUpdate();
                }
            }
        }
    }

    public float Joint0Angle(Vector3 point)
    {
        //joint12旋转轴
        var xAxis = joint1.angleAxis.normalized;
        //joint0旋转轴
        var yAxis = joint0.angleAxis.normalized;
        //机械臂面朝方向
        var zAxis = Vector3.Cross(xAxis, yAxis).normalized;
        var xPoint = Vector3.Dot(point, xAxis);
        //var yPoint = Vector3.Dot(point, yAxis);
        var zPoint = Vector3.Dot(point, zAxis);
        var offset = Joint0Off + Joint1Off + Joint23Off;
        

        var radius = Vector3.Dot(xAxis, offset);
        
        if (math.abs(radius) > 1E-08)
        {
            var distance =  math.sqrt(xPoint * xPoint + zPoint * zPoint);
            var tempAngle = math.asin(radius / distance) * Mathf.Rad2Deg;
            var vecPoint = new Vector3(xPoint, 0, zPoint);
            var rotation = Quaternion.AngleAxis(-tempAngle, yAxis);
            var newDir = (rotation * vecPoint).normalized;
            var angle0 = math.acos(Vector3.Dot(zAxis, newDir));
            var angleDirection = Vector3.Dot(Vector3.Cross(zAxis, newDir), yAxis) >= 0;
            return angleDirection ? angle0 * Mathf.Rad2Deg : -angle0 * Mathf.Rad2Deg;
        }
        else
        {
            var angle0 = Mathf.Atan2(xPoint, zPoint);
            return angle0 * Mathf.Rad2Deg;
        }
    }

    public (float joint1Angle, float joint2Angle) Joint12Angle(Vector3 point)
    {
        //joint12旋转轴
        var xAxis = joint1.angleAxis.normalized;
        //joint0旋转轴
        var yAxis = joint0.angleAxis.normalized;
        //机械臂面朝方向
        var zAxis = Vector3.Cross(xAxis, yAxis).normalized;

        var offset1 = Joint1Off - Vector3.Dot(xAxis, Joint1Off) * xAxis;
        var offset2 = Joint23Off - Vector3.Dot(xAxis, Joint23Off) * xAxis;
        var l1 = offset1.magnitude;
        var l2 = offset2.magnitude;
        var z1 = Vector3.Dot(offset1, zAxis);
        var y1 = Vector3.Dot(offset1, yAxis);
        var z2 = Vector3.Dot(offset2, zAxis);
        var y2 = Vector3.Dot(offset2, yAxis);
        var defAngle1 = math.atan2(z1, y1);
        var defAngle2 = math.atan2(z2, y2) - defAngle1;
        var pZ = Vector3.Dot(point, zAxis);
        var pY = Vector3.Dot(point, yAxis);
        var tempSquare = pZ.Square() + pY.Square();
        var joint2Angle = math.acos((tempSquare - l1.Square() - l2.Square()) / (2.0f * l1 * l2));
        var joint1Offset = math.acos((l2.Square() - tempSquare - l1.Square()) / (-2.0f * l1 * math.sqrt(tempSquare)));
        var joint1Base = math.atan2(pZ, pY);
        var joint1Angle = 0f;
        if (joint2Angle >= 0)
        {
            joint1Angle = joint1Base - joint1Offset;
        }
        else
        {
            joint1Angle = joint1Base + joint1Offset;
        }

        return ((joint1Angle - defAngle1) * Mathf.Rad2Deg, (joint2Angle - defAngle2) * Mathf.Rad2Deg);
    }

    public (float joint3Angle, float joint4Angle, float joint5Angle) Joint345Angle(Quaternion rotation)
    {
        var rm =  new KMath.RotationMatrix(rotation);
        var joint4Angle = math.atan2(rm[1, 2], math.sqrt(rm[1, 0].Square() + rm[1, 1].Square()));
        var temp = math.cos(joint4Angle);
        var joint5Angle = math.atan2(-rm[0, 2] / temp, rm[2, 2] / temp);
        var joint3Angle = math.atan2(-rm[1, 0] / temp, rm[1, 1] / temp);
        return (joint3Angle * Mathf.Rad2Deg, joint4Angle * Mathf.Rad2Deg, joint5Angle * Mathf.Rad2Deg);
    }
    


    public void JacobiAngleUpdate()
    {
        var targetTran = target.transform;
        var targetPos = targetTran.position;
        var targetRot = targetTran.rotation;
        var originalTran = joint0.transform;
        var originalPos = originalTran.position;
        var originalRot = Quaternion.identity;
        var par = originalTran.parent;
        if (par) originalRot = par.transform.rotation;
        //世界到joint0
        //在一般的机械臂程序中KMath.Transform(Quaternion.identity,Vector3.zero)
        //此处需要取消物体来自世界空间而非joint0空间带来的影响
        var worldToJoint0 = new KMath.Transform(originalRot, originalPos);
        var worldToJoint1 = worldToJoint0.Next(joint0.angleAxis, joint0.Angle, Joint0Off);
        var worldToJoint2 = worldToJoint1.Next(joint1.angleAxis, joint1.Angle, Joint1Off);
        var worldToJoint3 = worldToJoint2.Next(joint2.angleAxis, joint2.Angle, Joint2Off);
        var worldToJoint4 = worldToJoint3.Next(joint3.angleAxis, joint3.Angle, Joint3Off);
        var worldToJoint5 = worldToJoint4.Next(joint4.angleAxis, joint4.Angle, Joint4Off);
        var worldToEnd = worldToJoint5.Next(joint5.angleAxis, joint5.Angle, ToolOffset);
        var vec0 = worldToEnd.offset - worldToJoint0.offset;
        var vec1 = worldToEnd.offset - worldToJoint1.offset;
        var vec2 = worldToEnd.offset - worldToJoint2.offset;
        var vec3 = worldToEnd.offset - worldToJoint3.offset;
        var vec4 = worldToEnd.offset - worldToJoint4.offset;
        var vec5 = worldToEnd.offset - worldToJoint5.offset;
        var pal0 = worldToJoint0.rotation * joint0.angleAxis.normalized;
        var pal1 = worldToJoint1.rotation * joint1.angleAxis.normalized;
        var pal2 = worldToJoint2.rotation * joint2.angleAxis.normalized;
        var pal3 = worldToJoint3.rotation * joint3.angleAxis.normalized;
        var pal4 = worldToJoint4.rotation * joint4.angleAxis.normalized;
        var pal5 = worldToJoint5.rotation * joint5.angleAxis.normalized;
        var vel0 = Vector3.Cross(pal0, vec0);
        var vel1 = Vector3.Cross(pal1, vec1);
        var vel2 = Vector3.Cross(pal2, vec2);
        var vel3 = Vector3.Cross(pal3, vec3);
        var vel4 = Vector3.Cross(pal4, vec4);
        var vel5 = Vector3.Cross(pal5, vec5);
        var mA = Matrix<float>.Build.DenseOfArray(new[,] {
            {vel0.x,vel1.x,vel2.x,vel3.x,vel4.x,vel5.x},
            {vel0.y,vel1.y,vel2.y,vel3.y,vel4.y,vel5.y},
            {vel0.z,vel1.z,vel2.z,vel3.z,vel4.z,vel5.z},
            {pal0.x,pal1.x,pal2.x,pal3.x,pal4.x,pal5.x},
            {pal0.y,pal1.y,pal2.y,pal3.y,pal4.y,pal5.y},
            {pal0.z,pal1.z,pal2.z,pal3.z,pal4.z,pal5.z}
        });

        var vec = targetPos - worldToEnd.offset;
        var qua = (worldToEnd.InvRotation * targetRot).eulerAngles;
        if (qua.x >= 180f)
        {
            qua.x -= 360f;
        }if (qua.y >= 180f)
        {
            qua.y -= 360f;
        }if (qua.z >= 180f)
        {
            qua.z -= 360f;
        }
        var vecScale = jacobianInfo.positionPid.Update(0f, vec.magnitude);
        var quaScale = jacobianInfo.rotationPid.Update(0f, qua.magnitude);
        vec = vec.normalized;
        vec *= -vecScale;
        qua *= -quaScale;
        Debug.Log(qua);
        var svd = mA.Svd();
        var invA = svd.VT.Transpose() * svd.W.PseudoInverse() * svd.U.Transpose();
        var predict = Matrix<float>.Build.DenseOfArray(new[,] {{vec.x, vec.y, vec.z, qua.x, qua.y, qua.z}});
        var deltaAngles = invA * predict.Transpose();
        //Debug.Log(deltaAngles);
        //Debug.Log(deltaAngles);
        joint0.Angle += deltaAngles[0, 0];
        joint1.Angle += deltaAngles[1, 0];
        joint2.Angle += deltaAngles[2, 0];
        joint3.Angle += deltaAngles[3, 0];
        joint4.Angle += deltaAngles[4, 0];
        joint5.Angle += deltaAngles[5, 0];
        //Debug.Log(angles);
    }


    public void AnalyticalUpdate()
    {
        var targetTran = target.transform;
        var targetPos = targetTran.position;
        var targetRot = targetTran.rotation;
        var originalTran = joint0.transform;
        var originalPos = originalTran.position;
        var originalRot = Quaternion.identity;
        var par = originalTran.parent;
        if (par) originalRot = par.transform.rotation;
        //世界到joint0
        //在一般的机械臂程序中KMath.Transform(Quaternion.identity,Vector3.zero)
        //此处需要取消物体来自世界空间而非joint0空间带来的影响
        var worldToJoint0 = new KMath.Transform(originalRot, originalPos);
        var targetPosition = targetPos - targetRot * (Joint4Off + ToolOffset);
        var pointTest = worldToJoint0.InvRotation * (targetPosition - worldToJoint0.Offset);
        
        var joint0Angle = Joint0Angle(pointTest);
        //世界至joint1
        var worldToJoint1 = worldToJoint0.Next(joint0.angleAxis, joint0Angle, Joint0Off);
        pointTest = worldToJoint1.InvRotation * (targetPosition - worldToJoint1.Offset);
        var (joint1Angle, joint2Angle) = Joint12Angle(pointTest);
        //世界至joint2
        var worldToJoint2 = worldToJoint1.Next(joint1.angleAxis, joint1Angle, Joint1Off);
        //世界至joint3(旋转),joint4(位置)
        var worldToJoint3T = worldToJoint2.Next(joint2.angleAxis, joint2Angle, Joint23Off);
        //得到相差的旋转
        var rotationTest = worldToJoint3T.InvRotation * targetRot;
        var (joint3Angle, joint4Angle, joint5Angle) = Joint345Angle(rotationTest);
        
        joint0.Angle = joint0Angle;
        joint1.Angle = joint1Angle;
        joint2.Angle = joint2Angle;
        joint3.Angle = joint3Angle;
        joint4.Angle = joint4Angle;
        joint5.Angle = joint5Angle;
    }
    
    public void InitJoint012Constant(Vector3 offset0, Vector3 offset1, Vector3 offset2, Vector3 offset3,
        Vector3 offset4, Vector3 targetOffset)
    {
        joint0Off = offset0;
        joint1Off = offset1;
        joint2Off = offset2;
        joint3Off = offset3;
        joint4Off = offset4;
        toolOffset = targetOffset;
    }

    public override IKError IK(List<float> ikStore,Vector3 vec, Quaternion rot)
    {
        var originalTran = joint0.transform;
        var originalPos = originalTran.position;
        var originalRot = Quaternion.identity;
        var par = originalTran.parent;
        if (par) originalRot = par.transform.rotation;
        //世界到joint0
        var worldToJoint0 = new KMath.Transform(originalRot, originalPos);
        //为了在接下来的Joint0Angle中不变换其它向量,所以把Vec反向旋转.
        var targetPosition = vec - rot * (Joint4Off + ToolOffset);
        var point = worldToJoint0.InvRotation * (targetPosition - worldToJoint0.Offset);
        var joint0Angle = Joint0Angle(point);
        //世界至joint1
        var worldToJoint1 = worldToJoint0.Next(joint0.angleAxis, joint0Angle, Joint0Off);
        point = worldToJoint1.InvRotation * (targetPosition - worldToJoint1.Offset);
        var (joint1Angle, joint2Angle) = Joint12Angle(point);
        //世界至joint2
        var worldToJoint2 = worldToJoint1.Next(joint1.angleAxis, joint1Angle, Joint1Off);
        //世界,位置至joint4,旋转至joint3
        var worldToJoint3T = worldToJoint2.Next(joint2.angleAxis, joint2Angle, Joint23Off);
        //得到相差的旋转
        var rotation = worldToJoint3T.InvRotation * rot;
        var (joint3Angle, joint4Angle, joint5Angle) = Joint345Angle(rotation);
        var error = IKError.None;
        error |= joint0.AngleCheck(joint0Angle);
        error |= joint1.AngleCheck(joint1Angle);
        error |= joint2.AngleCheck(joint2Angle);
        error |= joint3.AngleCheck(joint3Angle);
        error |= joint4.AngleCheck(joint4Angle);
        error |= joint5.AngleCheck(joint5Angle);
        if (error != IKError.None) return error;
        if (ikStore == null) return IKError.None;
        ikStore.Add(joint0Angle);
        ikStore.Add(joint1Angle);
        ikStore.Add(joint2Angle);
        ikStore.Add(joint3Angle);
        ikStore.Add(joint4Angle);
        ikStore.Add(joint5Angle);
        return IKError.None;
    }

    public override void Move(List<float> angles, int index)
    {
        joint0.Angle = angles[index * 6 + 0];
        joint1.Angle = angles[index * 6 + 1];
        joint2.Angle = angles[index * 6 + 2];
        joint3.Angle = angles[index * 6 + 3];
        joint4.Angle = angles[index * 6 + 4];
        joint5.Angle = angles[index * 6 + 5];
    }

    protected override List<(KMath.Transform,Vector3)> CurrentInfo 
    {
        get
        {
            var originalTran = joint0.transform;
            var originalPos = originalTran.position;
            var originalRot = Quaternion.identity;
            var par = originalTran.parent;
            if (par) originalRot = par.transform.rotation;
            var ret = new List<(KMath.Transform,Vector3)>();
            var worldToJoint0 = new KMath.Transform(originalRot, originalPos);
            var worldToJoint1 = worldToJoint0.Next(joint0.angleAxis, joint0.Angle, Joint0Off);
            var worldToJoint2 = worldToJoint1.Next(joint1.angleAxis, joint1.Angle, Joint1Off);
            var worldToJoint3 = worldToJoint2.Next(joint2.angleAxis, joint2.Angle, Joint2Off);
            var worldToJoint4 = worldToJoint3.Next(joint3.angleAxis, joint3.Angle, Joint3Off);
            var worldToJoint5 = worldToJoint4.Next(joint4.angleAxis, joint4.Angle, Joint4Off);
            var worldToEnd = worldToJoint5.Next(joint5.angleAxis, joint5.Angle, ToolOffset);
            ret.Add((worldToJoint0,joint0.angleAxis));
            ret.Add((worldToJoint1,joint1.angleAxis));
            ret.Add((worldToJoint2,joint2.angleAxis));
            ret.Add((worldToJoint3,joint3.angleAxis));
            ret.Add((worldToJoint4,joint4.angleAxis));
            ret.Add((worldToJoint5,joint5.angleAxis));
            ret.Add((worldToEnd,Vector3.zero));
            return ret;
        }
    }


}
