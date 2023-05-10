
using System;
using UnityEngine;

public class KGrab : MonoBehaviour
{
    public ArticulationBody abR;
    public ArticulationBody abL;
    public Vector3 forceDir;
    public float limit;
    public float range;
    public bool switchFlag;
    public float damping;
    public float force;
    public float force1;
    public ForceMode forceMode;
    public void FixedUpdate()
    {
        abL.xDrive= new ArticulationDrive()
        {
            lowerLimit = -(limit+range),
            upperLimit = -limit,
            damping = damping
        };
        abR.xDrive = new ArticulationDrive()
        {
            lowerLimit = limit,
            upperLimit = limit+range,
            damping = damping
        };
        var forceTemp = force1;
        if (switchFlag)
        {
            forceTemp = force;
        }
        var tR = abR.transform;
        var tL = abL.transform;
        var axisR = tR.transform.rotation * abR.anchorRotation * Vector3.right;
        var axisL = tL.transform.rotation * abL.anchorRotation * Vector3.right;
        abR.AddTorque(axisR * forceTemp, forceMode);
        abL.AddTorque(-axisL * forceTemp, forceMode);
    }

    private bool _finish = true;
    public bool Finish => _finish;
    
    public void Release()
    {
        switchFlag = false;
        _finish = false;
        StartCoroutine(KTimer.TimeWait(() => { _finish = true; }, 1f));
    }

    public void Grab()
    {
        switchFlag = true;
        _finish = false;
        StartCoroutine(KTimer.TimeWait(() => { _finish = true; }, 1f));
    }

    private void OnDrawGizmos()
    {
        /*
        var tR = abR.transform;
        var tL = abL.transform;
        var pR = tR.position;
        var pL = tL.position;
        var axisR = tR.transform.rotation * abR.anchorRotation * Vector3.right;
        var axisL = tL.transform.rotation * abL.anchorRotation * Vector3.right;
        Gizmos.color=Color.green;
        Gizmos.DrawLine(pR, pR + Quaternion.AngleAxis(limit, axisR) * Vector3.down);
        Gizmos.DrawLine(pL, pL + Quaternion.AngleAxis(-limit, axisL) * Vector3.down);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pR, pR + Quaternion.AngleAxis((limit+range), axisR) * Vector3.down);
        Gizmos.DrawLine(pL, pL + Quaternion.AngleAxis(-(limit+range), axisL) * Vector3.down);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(pR, pR + axisR*10);
        Gizmos.DrawLine(pL, pL + axisL*10);*/
    }
}
