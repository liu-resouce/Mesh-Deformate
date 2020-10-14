using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// 伯努利双纽线
/// </summary>
public class BernoulliLemniscateCurve : BaseCurve
{


    private class Quadrant
    {
        public int Start { set; get; }
        public int End { set; get; }
        public bool IsPositive { set; get; }

        public Quadrant(int start, int end)
        {
            Start = start;
            End = end;
            IsPositive = End > Start;
        }
    }



    private float _speed;
    private float _radius;
    private float _angle;
    private float _mul;
    private int _quadrantIndex;
    private Quadrant _curQuadrant;
    private List<Quadrant> _quadrants;

    private Vector3 _baseVector1;
    private Vector3 _baseVector2;
    private Vector3 _posVector3;
    private Vector3 _blobPos;


    private Vector3 _curVector3;
    private Vector3 _targetVector3;
    private Vector3 _dirVector3;


    public override void InitiPendulum(Transform transform)
    {
        Pendulum = transform;
        _blobPos = Pendulum.position;
    }

    public override void Initi(params float[] paramsFloats)
    {
        _radius = paramsFloats[0];
        _speed = paramsFloats[1];
        _quadrantIndex = 0;
        _quadrants = new List<Quadrant>();
        _quadrants.Add(new Quadrant(45, 90));
        _quadrants.Add(new Quadrant(180 + 90, 180 + 45));
        _quadrants.Add(new Quadrant(180 + 45, 180));
        _quadrants.Add(new Quadrant(0, 45));

        _curQuadrant = _quadrants[_quadrantIndex];
        _angle = _curQuadrant.Start;

        _baseVector1 = Vector3.right;
        _baseVector2 = Vector3.forward;

        float t = Pendulum.eulerAngles.z;
        _radius = 11 * Mathf.Tan(Mathf.Deg2Rad * t);
       
        _targetVector3 = GetTargetPos(0.5f);
        _curVector3 = _targetVector3;
        _dirVector3 = (_targetVector3 - _curVector3).normalized;

        Pendulum.up = _blobPos - _targetVector3;
    }

    public override void OnUpdate()
    {
        _baseVector1 = Quaternion.Euler(0, 2 * Time.deltaTime, 0) * _baseVector1;
        _baseVector2 = Quaternion.Euler(0, 2 * Time.deltaTime, 0) * _baseVector2;

        float delta = 0;
        if (_quadrantIndex == 2 || _quadrantIndex == 3)
            delta = Time.deltaTime * _speed * (1 + 0.1f * Mathf.Abs(Mathf.Cos(Mathf.Deg2Rad * _angle)));
        else
            delta = Time.deltaTime * _speed * (1 + 0.1f * Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * _angle)));

        Vector3 deltaV3 = _dirVector3 * delta;
        if (Vector3.Dot(_targetVector3 - _curVector3, _dirVector3) < delta)
        {
            _curVector3 = _targetVector3;
            _targetVector3 = GetTargetPos(0.5f);
            _dirVector3 = (_targetVector3 - _curVector3).normalized;
        }
        else
        {
            _curVector3 += deltaV3;
        }

        Pendulum.up = _blobPos - _curVector3;
    }


    private void UpdateAngle(float delta)
    {
        if (_curQuadrant.IsPositive)
        {
            if (_angle + delta >= _curQuadrant.End)
            {
                _quadrantIndex++;
                _quadrantIndex %= 4;
                _curQuadrant = _quadrants[_quadrantIndex];

                _angle = _curQuadrant.Start;
            }
            else
            {
                _angle += delta;
            }
        }
        else
        {
            if (_angle - delta <= _curQuadrant.End)
            {
                _quadrantIndex++;
                _quadrantIndex %= 4;
                _curQuadrant = _quadrants[_quadrantIndex];

                _angle = _curQuadrant.Start;
            }
            else
            {
                _angle -= delta;
            }
        }
    }


    private Vector3 GetTargetPos(float delta)
    {
        UpdateAngle(delta);

        float rad = Mathf.Deg2Rad * _angle;
        if (Mathf.Abs(_angle - 90) < 0.001f || Mathf.Abs(_angle - 270) < 0.001f)
        {
            _posVector3 = Vector3.zero;
        }
        else
        {
            Vector3 p1 = _baseVector1 * _radius * Mathf.Sqrt(2 * (Mathf.Sin(2 * rad))) * Mathf.Cos(rad);
            Vector3 p2 = _baseVector2 * _radius * Mathf.Sqrt(2 * (Mathf.Sin(2 * rad))) * Mathf.Sin(rad);
            _posVector3 = p1 + p2;
        }
        return _posVector3;
    }





}


