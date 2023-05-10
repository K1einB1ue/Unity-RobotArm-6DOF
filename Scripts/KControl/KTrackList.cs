
using System;
using UnityEngine;

public abstract class KTrackList : MonoBehaviour
{
     protected bool finish = true;
     public bool Finish => finish;

     private void Awake()
     {
          finish = true;
     }

     public abstract void TrackUpdate(KRobot robot);
}
