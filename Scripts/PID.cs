using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PID : MonoBehaviour
{
    public float target;
    public float p, i, d; 
    private float _errorSum = 0;
    private float _preError = 0;

    private Rigidbody _rigidbody;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        var ray = new Ray(transform.position, Vector3.down);
        Physics.Raycast(ray, out var raycastHit, 50.0f);
        _rigidbody.AddForce(Vector3.up * PidUpdate(raycastHit.distance), ForceMode.Force);
    }
    
    

    private float PidUpdate(float current)
    {
        var error = target - current;
        var dError = error - _preError;
        var output = p * error + i * _errorSum + d * dError;
        _errorSum += error;
        _preError = error;
        return output;
    }
}
