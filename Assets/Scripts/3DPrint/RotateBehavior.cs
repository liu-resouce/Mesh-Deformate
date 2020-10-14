using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateBehavior : MonoBehaviour
{

    [SerializeField] private float _xSpeed;
    [SerializeField] private float _ySpeed;

    [SerializeField] private Transform _transform;



    private float _angleX;
    private float _angleY;

    private Vector3 _mouseVector3;

    private void Update()
    {
        float delta = 0;
        if (Input.GetMouseButtonDown(0))
        {
            _mouseVector3 = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            Vector3 p = Input.mousePosition - _mouseVector3;
            if (Mathf.Abs(p.x) >= Mathf.Abs(p.y))
            {
                delta = p.x * Time.deltaTime * _xSpeed;
                _transform.Rotate(Vector3.up, delta, Space.Self);
            }
            else
            {
                delta = p.y * Time.deltaTime * _ySpeed;
                _transform.Rotate(Vector3.right, delta, Space.Self);
            }
            _mouseVector3 = Input.mousePosition;
        }
    }



}
