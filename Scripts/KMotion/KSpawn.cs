
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class KSpawn : MonoBehaviour
{
    public Vector3 spawnPoint;
    public Quaternion spawnRotation;

    public void Spawn()
    {
        var rig = GetComponent<Rigidbody>();
        rig.velocity = Vector3.zero;
        rig.inertiaTensorRotation = Quaternion.identity;
        var tran = transform;
        tran.position = spawnPoint;
        tran.rotation = spawnRotation;
    }
}

[CustomEditor(typeof(KSpawn))]
public class KSpawnEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var spawn = target as KSpawn;
        if(spawn == null) return;
        if (GUILayout.Button("记录重生点"))
        {
            var tran = spawn.transform;
            spawn.spawnPoint = tran.position;
            spawn.spawnRotation = tran.rotation;
        }

        if (GUILayout.Button("重生"))
        {
            spawn.Spawn();
        }
    }
}
