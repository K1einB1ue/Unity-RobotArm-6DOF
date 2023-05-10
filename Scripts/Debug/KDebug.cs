using UnityEngine;

public class KDebug
{
    public static void Draw(Vector3 pos, Quaternion rot, Vector3 vec)
    {
        var uY = rot * Vector3.up;
        var uX = rot * Vector3.right;
        var uZ = rot * Vector3.forward;
        var x = uX.Dot(vec) *uX;
        var y = uY.Dot(vec) *uY;
        var z = uZ.Dot(vec) *uZ;
        Debug.DrawLine(pos, pos + x);
        Debug.DrawLine(pos, pos + y);
        Debug.DrawLine(pos, pos + z);
    }

    public static void DrawRot(Vector3 pos, Quaternion rot, Vector3 vec)
    {
        var uY = rot * Vector3.up;
        var uX = rot * Vector3.right;
        var uZ = rot * Vector3.forward;
        var x = vec.x *uX;
        var y = vec.y *uY;
        var z = vec.z *uZ;
        Debug.DrawLine(pos, pos + x);
        Debug.DrawLine(pos, pos + y);
        Debug.DrawLine(pos, pos + z);
    }

    public static void DrawRotation(Vector3 pos, Quaternion quat, float radius)
    {
        var uY = quat * Vector3.up;
        var uX = quat * Vector3.right;
        var uZ = quat * Vector3.forward;
        var x = radius *uX;
        var y = radius *uY;
        var z = radius *uZ;
        Debug.DrawLine(pos, pos + x);
        Debug.DrawLine(pos, pos + y);
        Debug.DrawLine(pos, pos + z);
    }

    public static void DrawMark(Vector3 pos)
    {
        Debug.DrawLine(pos, pos + Vector3.up * 2 + Vector3.right);
        Debug.DrawLine(pos, pos + Vector3.up * 2 + Vector3.left);
        Debug.DrawLine(pos + Vector3.up * 2 + Vector3.right, pos + Vector3.up * 2 + Vector3.left);
    }

    public static void DrawPlane(Vector3 pos, Vector3 nor,float radius = 20)
    {
        Debug.DrawLine(pos, pos + nor * radius);
        var vert = KTransform.GetVerticalVec(nor);
        var divNum = 36;
        var begin = pos + vert * radius;
        var from = begin;
        for (var i = 1; i < divNum; i++)
        {
            var to = pos + Quaternion.AngleAxis((360.0f / divNum) * i, nor) * vert * radius;
            Debug.DrawLine(from, to);
            from = to;
        }
        Debug.DrawLine(from, begin);
    }
}
