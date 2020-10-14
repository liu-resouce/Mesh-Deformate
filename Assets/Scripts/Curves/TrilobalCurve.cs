using UnityEngine;
using UnityEditor;


/// <summary>
/// 三叶草曲线
/// </summary>
public class TrilobalCurve : BaseCurve
{

    private float _radius;
    private float _speed;
    private float _angle;
    private int _leafCount;

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
        _radius = paramsFloats[0];
        _speed = paramsFloats[1];
        _leafCount = (int)paramsFloats[2];

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
        delta = Time.deltaTime * _speed * (2 - 1 * Mathf.Abs(Mathf.Cos(_leafCount * Mathf.Deg2Rad * _angle)));

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
        float r = _radius * Mathf.Cos(_leafCount * rad);
        Vector3 p1 = _baseVector1 * r * Mathf.Cos(rad);
        Vector3 p2 = _baseVector2 * r * Mathf.Sin(rad);
        return p1 + p2;
    }


}