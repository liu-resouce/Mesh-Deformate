using UnityEngine;
using System.Collections;

/// <summary>
/// 毛雷尔玫瑰曲线
/// </summary>
public class MaurerRoseCurve : BaseCurve
{

    private float _radius;
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
       
    }

    private Vector3 GetTargetPos(float delta)
    {
        _angle += delta;
        float rad = Mathf.Deg2Rad * _angle;
        Vector3 p1 = _baseVector1 * _radius * Mathf.Sin(rad);
        Vector3 p2 = _baseVector2 * _radius * Mathf.Sin(rad);
        return p1 + p2;
    }

}
