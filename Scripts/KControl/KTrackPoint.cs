
using System;
using UnityEngine;

[ExecuteInEditMode]
public class KTrackPoint : MonoBehaviour
{
   public static bool DestroyCallEnable = true;
   public KTrack root;

   

   private void OnDestroy()
   {
      if (DestroyCallEnable) root.Remove(this);
   }
}
