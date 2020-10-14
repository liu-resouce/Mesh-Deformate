using UnityEngine;

/// <summary>
/// 阿基米德曲线
/// </summary>
public class ArchimedesSpireCurve : BaseCurve
{

    private Vector3 _baseVector1;
    private Vector3 _baseVector2;
    private Vector3 _blobPos;


    private float _angle;
    private float _radius;
    private float _k;
    private float _b;
    private float _rotateSpeed;

    public override void InitiPendulum(Transform transform)
    {
        Pendulum = transform;
        _blobPos = Pendulum.position;
    }

    public override void Initi(params float[] paramsFloats)
    {
        _b = paramsFloats[0];
        _rotateSpeed = paramsFloats[1];

        _baseVector1 = Vector3.right;
        _baseVector2 = Vector3.forward;

        float t = Pendulum.eulerAngles.z;
        _k = 11 * Mathf.Tan(Mathf.Deg2Rad * t);
    }

    public override void OnUpdate()
    {
        float rad = Mathf.Deg2Rad * _angle;
        _radius = _k + Mathf.Sin(rad) * _b;
        _angle += _rotateSpeed * (1 + Mathf.Abs(Mathf.Sin(Mathf.Deg2Rad * _angle)) * 0.5f);
        _baseVector1 = Quaternion.Euler(0, 10 * Time.deltaTime, 0) * _baseVector1;
        _baseVector2 = Quaternion.Euler(0, 10 * Time.deltaTime, 0) * _baseVector2;
        Vector3 p1 = _baseVector1 * _radius * Mathf.Cos(rad);
        Vector3 p2 = _baseVector2 * _radius * Mathf.Sin(rad);
        Vector3 p3 = p1 + p2;

        Pendulum.up = _blobPos - p3;
    }

}
