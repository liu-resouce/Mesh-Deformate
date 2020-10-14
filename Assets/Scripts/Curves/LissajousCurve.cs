using UnityEngine;
using System.Collections;

/// <summary>
/// 利萨茹曲线
/// </summary>
public class LissajousCurve : BaseCurve
{

    private float _radius1;
    private float _radius2;
    private float _p;
    private float _q;
    private float _phase;

    private float _speed;
    private float _angle;
   

    private Vector3 _baseVector1;
    private Vector3 _baseVector2;
    private Vector3 _blobPos;

    private Vector3 _curVector3;
    private Vector3 _targetVector3;
    private Vector3 _dirVector3;


    public override void InitiPendulum(Transform transform)
    {
        Pendulum = transform;
        _blobPos = Pendulum.position;
        _angle = 0;
    }

    public override void Initi(params float[] paramsFloats)
    {
        _speed = paramsFloats[0];
        _p = paramsFloats[1];
        _phase = paramsFloats[2];

        _baseVector1 = Vector3.right;
        _baseVector2 = Vector3.forward;

        float t = Pendulum.eulerAngles.z;
        _radius1 = 11 * Mathf.Tan(Mathf.Deg2Rad * t);
        _radius2 = _radius1;

        _targetVector3 = GetTargetPos(0.5f);
        _curVector3 = _targetVector3;
        _dirVector3 = (_targetVector3 - _curVector3).normalized;

        Pendulum.up = _blobPos - _targetVector3;
    }

    public override void OnUpdate()
    {
        float delta = 0;
        delta = Time.deltaTime * _speed * (2 - 1 * Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * _angle)));

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

    private Vector3 GetTargetPos(float delta)
    {
        _angle += delta;
        float rad = Mathf.Deg2Rad * _angle;
        Vector3 p1 = _baseVector1 * _radius1 * Mathf.Sin(rad);
        Vector3 p2 = _baseVector2 * _radius2 * Mathf.Sin(rad * _p + Mathf.Deg2Rad * _phase);
        return p1 + p2;
    }


}
