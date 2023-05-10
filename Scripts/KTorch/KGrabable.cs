
using System;
using UnityEngine;

public class KGrabable : MonoBehaviour
{
    public Vector3 offset;
    public Quaternion rotation;
    public float width;


    public Quaternion GrabRotation => transform.rotation * rotation;
    public Vector3 GrabPosition => transform.position + transform.rotation * offset;
    
    private void OnDrawGizmos()
    {
        var rot = transform.rotation;
        var pos = GrabPosition;
        var dir = rot * rotation * Vector3.up * width;
        var handDir = rot * rotation * Vector3.right * width;
        var rotPos = pos - dir;
        Gizmos.DrawLine(rotPos, rotPos + dir * 2);
        Gizmos.DrawLine(pos, pos + handDir);
    }

    public string GetDetail(Transform tran)
    {
        var rot = tran.rotation;
        var pos = tran.position;
        var x = rot * Vector3.right;
        var y = rot * Vector3.up;
        var z = rot * Vector3.forward;
        var posOffset = GrabPosition - pos;
        Debug.DrawLine(pos, GrabPosition);
        var localOffset = new Vector3(Vector3.Dot(x, posOffset), Vector3.Dot(y, posOffset),
            Vector3.Dot(z, posOffset));
        var tranRotOffset = Quaternion.Inverse(GrabRotation) * rot;
        var angle = tranRotOffset.eulerAngles;
        return $"{localOffset.x},{localOffset.y},{localOffset.z},{angle.x},{angle.y},{angle.z},{width}";
    }

    public string GetDetail()
    {
        return "";
    }
}
        
