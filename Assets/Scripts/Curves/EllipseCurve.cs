using UnityEngine;

/// <summary>
/// 椭圆曲线
/// </summary>
public class EllipseCurve : BaseCurve
{

    private Vector3 _baseVector1;
    private Vector3 _baseVector2;
    private Vector3 _blobPos;

    private float _a;
    private float _b;
    private float _k;
    private float _angle;
    private float _rotateSpeed;
    private float _offsetSpeed;



    public override void InitiPendulum(Transform transform)
    {
        Pendulum = transform;
        _blobPos = Pendulum.position;
    }


    public override void Initi(params float[] paramsFloats)
    {
        _rotateSpeed = paramsFloats[0];
        _offsetSpeed = paramsFloats[1];

        float t = Pendulum.eulerAngles.z;
        _angle = Pendulum.eulerAngles.y;
        _a = 40 * Mathf.Tan(Mathf.Deg2Rad * t);
        _b = _a / 2f;

        _baseVector1 = Vector3.right;
        _baseVector2 = Vector3.forward;

        _baseVector1 = Quaternion.Euler(0, _angle, 0) * _baseVector1;
        _baseVector2 = Quaternion.Euler(0, -90, 0) * _baseVector1;

        _angle = 0;
    }


    public override void OnUpdate()
    {
        _angle += _rotateSpeed * (_k + 1) * Time.deltaTime;
        Vector3 p1 = _a * _baseVector1 * Mathf.Cos(Mathf.Deg2Rad * _angle);
        Vector3 p2 = _b * _baseVector2 * Mathf.Sin(-Mathf.Deg2Rad * _angle);
        Vector3 p3 = p1 + p2;

        _k = Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * _angle)) * 0.5f;

        _baseVector1 = Quaternion.Euler(0, _offsetSpeed * Time.deltaTime, 0) * _baseVector1;
        _baseVector2 = Quaternion.Euler(0, _offsetSpeed * Time.deltaTime, 0) * _baseVector2;

        Pendulum.up = _blobPos - p3;
    }


}
