
using System;
using Unity.Mathematics;
using UnityEngine;

public class KMathTest : MonoBehaviour
{
    public Vector3 vec0;
    public Vector3 vec1;
    
    private void OnDrawGizmosSelected()
    {
        var pos = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + vec0);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, pos + vec1);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + (Vector3) math.cross(vec0, vec1));
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pos, pos + Vector3.Cross(vec0, vec1));
    }
}
