
using System;
using UnityEngine;

public class KTarget : MonoBehaviour
{
    public float displayLength = 20f;
    private void OnDrawGizmosSelected()
    {
        var tran = transform;
        var pos = tran.position;
        var rot = tran.rotation;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + rot * Vector3.right * displayLength);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + rot * Vector3.up * displayLength);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + rot * Vector3.forward * displayLength);
    }
}
