
using System.Collections.Generic;
using UnityEngine;

public class PhysicalGrabMission : MonoBehaviour
{
    public KStateMachine kStateMachine;
    //public CamVisionMove camVisionMove;
    public KRobot robot;
    public KGrab grab;
    public KTrackList tracksBeg;
    public Transform midPos;
    public Transform releasePos;
    public KTrackList tracksEnd;
    public KSpawn grabTarget;
    
    
    public void Awake()
    {
        kStateMachine.Add(() => tracksBeg.TrackUpdate(robot), () => tracksBeg.Finish);
        kStateMachine.Add(() => grab.Grab(), () => grab.Finish);
        kStateMachine.Add(() => robot.MoveTo(midPos, 4, 100), () => !robot.Moving);
        kStateMachine.Add(() => robot.MoveTo(releasePos, 4, 100), () => !robot.Moving);
        kStateMachine.Add(() => grab.Release(), () => grab.Finish);
        kStateMachine.Add(() => tracksEnd.TrackUpdate(robot), () => tracksEnd.Finish);
        kStateMachine.Add(() => grabTarget.Spawn(), () => true);
    }

    public void FixedUpdate()
    {
        kStateMachine.Update();
    }
}
