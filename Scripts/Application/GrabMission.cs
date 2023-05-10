
using System;
using UnityEngine;

public class GrabMission : MonoBehaviour
{
    public KStateMachine kStateMachine;
    public CamVisionMove camVisionMove;
    public KRobot robot;
    public Transform grabPosition;
    public Vector3 grabOffset;
    public Transform releasePosition;
    public Vector3 releaseOffset;

    public void Awake()
    {
        kStateMachine.Add(() => robot.MoveTo(grabPosition, 2, 100), () => !robot.Moving);
        kStateMachine.Add(() =>
        {
            camVisionMove.Enable = true;
        }, () =>
        {
            if (!camVisionMove.UpdateCamVisionMove()) return false;
            camVisionMove.Enable = false;
            return true;

        });
        kStateMachine.Add(() => robot.MoveTo(grabOffset, 2, 100), () => !robot.Moving);
        kStateMachine.Add(() => robot.MoveTo(-grabOffset, 2, 100), () => !robot.Moving);
        kStateMachine.Add(() => robot.MoveTo(grabPosition, 2, 100), () => !robot.Moving);
        kStateMachine.Add(() => robot.MoveTo(releasePosition, 2, 100), () => !robot.Moving);
        kStateMachine.Add(() => robot.MoveTo(releaseOffset, 2, 100), () => !robot.Moving);
        kStateMachine.Add(() => robot.MoveTo(-releaseOffset, 2, 100), () => !robot.Moving);
    }

    public void FixedUpdate()
    {
        kStateMachine.Update();
    }
}
