using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolidConstraint : MonoBehaviour
{
    public bool link = true;
    public GameObject pipe;
    public GameObject ball;
    public float length;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        ShapeUpdate();
    }

    public void ShapeUpdate()
    {
        if (pipe && ball)
        {
            var temp = pipe.transform.localScale;
            temp.z = length;
            pipe.transform.localScale = temp;
            temp = ball.transform.localPosition;
            temp.y = length / 50f;
            ball.transform.localPosition = temp;
        }
        if (link)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var sonTran = transform.GetChild(i);
                if (!sonTran.TryGetComponent<SolidConstraint>(out var son)) continue;
                sonTran.position = ball.transform.position;
                son.ShapeUpdate();
            }
        }
    }
}
