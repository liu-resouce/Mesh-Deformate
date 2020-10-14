using UnityEngine;
using System.Collections.Generic;

public class RollView2D : MonoBehaviour
{

    private List<Vector3> _drawPoints;
    private List<Vector3> _originPoints;
    private List<Vector3> _localVector3S;
    private Vector3 _targetDir;
    private Vector3 _dirVector3;

    private bool _isSpread;
    private bool _isRoll;

    private int _rotateIndex;
    private float _totalTime;
    private float _curTime;
    private float _length;
    private float _angle;
    private float _width;



    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    private List<Vector3> _verticles;
    private List<Vector2> _uvList;
    private List<int> _triangles;


    public void OnIniti(List<Vector3> drawPoints)
    {
        _drawPoints = new List<Vector3>();
        _localVector3S = new List<Vector3>();
        _originPoints = new List<Vector3>();
        foreach (var t in drawPoints)
        {
            _originPoints.Add(t);
        }
        _drawPoints = drawPoints;
        GeneratorMesh(_drawPoints);
    }


    public void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0) && !_isSpread)
        {
            _isSpread = true;
            _rotateIndex = 0;
            SpreadReiniti();
        }

        if (Input.GetMouseButtonDown(1) && !_isRoll)
        {
            _isRoll = true;
            _rotateIndex = _drawPoints.Count - 2;
            RollReiniti();
        }
        SpreadUpdate();
        RollUpdate();
    }






    private void SpreadUpdate()
    {
        if (_isSpread)
        {
            _curTime += Time.deltaTime * 2f;
            if (_curTime >= _totalTime)
            {
                _curTime = _totalTime;
                UpdateSpreadVerticles(1);
                _rotateIndex++;
                SpreadReiniti();
            }
            else
            {
                UpdateSpreadVerticles(_curTime / _totalTime);
            }
        }
    }

    private void SpreadReiniti()
    {
        _localVector3S.Clear();
        if (_rotateIndex >= _drawPoints.Count - 1)
        {
            _isSpread = false;
            return;
        }
        if (_rotateIndex + 1 < _drawPoints.Count)
        {
            _targetDir = _drawPoints[_rotateIndex + 1] - _drawPoints[_rotateIndex];
            _length = _targetDir.magnitude;
            _curTime = 0;
            _angle = Vector3.Angle(_targetDir, Vector3.right);
            _totalTime = _angle / 40f;
        }

        for (int i = _rotateIndex + 2; i < _drawPoints.Count; i++)
        {
            Vector3 offset = _drawPoints[i] - _drawPoints[_rotateIndex + 1];
            _localVector3S.Add(offset);
        }
    }

    private void UpdateSpreadVerticles(float percent)
    {
        Vector3 d = Vector3.Lerp(_targetDir, Vector3.right * _length, percent);
        if (_rotateIndex + 1 < _drawPoints.Count)
        {
            _drawPoints[_rotateIndex + 1] = _drawPoints[_rotateIndex] + d;
        }
        for (int i = _rotateIndex + 2; i < _drawPoints.Count; i++)
        {
            Vector3 offset = Quaternion.Euler(0, 0, -_angle * percent) * _localVector3S[i - _rotateIndex - 2];
            _drawPoints[i] = _drawPoints[_rotateIndex + 1] + offset;
        }
        UpdateMesh(_drawPoints);
    }



    private void RollUpdate()
    {
        if (_isRoll)
        {
            _curTime += Time.deltaTime * 2f;
            if (_curTime >= _totalTime)
            {
                _curTime = _totalTime;
                UpdateRollVerticles(1);
                _rotateIndex--;
                RollReiniti();
            }
            else
            {
                UpdateRollVerticles(_curTime / _totalTime);
            }
        }
    }

    private void RollReiniti()
    {
        _localVector3S.Clear();
        if (_rotateIndex <= 0)
        {
            _isRoll = false;
            return;
        }
        if (_rotateIndex + 1 < _drawPoints.Count)
        {
            _targetDir = _originPoints[_rotateIndex + 1] - _originPoints[_rotateIndex];
            _dirVector3 = _originPoints[_rotateIndex] - _originPoints[_rotateIndex - 1];

            _length = _targetDir.magnitude;
            _curTime = 0;
            _angle = Vector3.Angle(_targetDir, _dirVector3);
            _totalTime = _angle / 40f;
        }

        if (_rotateIndex != _drawPoints.Count - 2)
        {
            for (int i = _rotateIndex + 2; i < _drawPoints.Count; i++)
            {
                Vector3 offset = _drawPoints[i] - _drawPoints[_rotateIndex + 1];
                _localVector3S.Add(offset);
            }
        }
    }

    private void UpdateRollVerticles(float percent)
    {
        if (_rotateIndex == _drawPoints.Count - 2)
        {
            Vector3 t = Quaternion.AngleAxis(percent * _angle, Vector3.forward) * Vector3.right;
            _drawPoints[_rotateIndex + 1] = _drawPoints[_rotateIndex] + t * _length;
        }
        else
        {
            Vector3 t = Quaternion.AngleAxis(percent * _angle, Vector3.forward) * Vector3.right;
            if (_rotateIndex + 1 < _drawPoints.Count)
            {
                _drawPoints[_rotateIndex + 1] = _drawPoints[_rotateIndex] + t * _length;
            }

            for (int i = _rotateIndex + 2; i < _drawPoints.Count; i++)
            {
                Vector3 offset = Quaternion.Euler(0, 0, _angle * percent) * _localVector3S[i - _rotateIndex - 2];
                _drawPoints[i] = _drawPoints[_rotateIndex + 1] + offset;
            }
        }
        UpdateMesh(_drawPoints);
    }



    private void GeneratorMesh(List<Vector3> drawPoints)
    {
        _width = 0.1f;
        _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        _meshFilter = gameObject.AddComponent<MeshFilter>();

        _mesh = new Mesh();
        _verticles = new List<Vector3>();
        _triangles = new List<int>();

        for (int i = 0; i < drawPoints.Count; i++)
        {
            if (i == drawPoints.Count - 1)
            {
                Vector3 d1 = drawPoints[i] - drawPoints[i - 1];
                Vector3 normal1 = Quaternion.Euler(0, 0, -90) * d1.normalized;
                _verticles.Add(drawPoints[i] + normal1 * _width);
                _verticles.Add(drawPoints[i] - normal1 * _width);
            }
            else if (i == 0)
            {
                Vector3 normal1 = Vector3.up;
                _verticles.Add(drawPoints[i] - normal1 * _width);
                _verticles.Add(drawPoints[i] + normal1 * _width);
            }
            else
            {
                Vector3 dir = drawPoints[i + 1] - drawPoints[i];
                Vector3 normal = Quaternion.Euler(0, 0, -90) * dir.normalized;
                _verticles.Add(drawPoints[i] + normal * _width);
                _verticles.Add(drawPoints[i] - normal * _width);
            }
        }

        for (int i = 0; i < drawPoints.Count - 1; i++)
        {
            _triangles.Add(i * 2); _triangles.Add(i * 2 + 1); _triangles.Add(i * 2 + 3);
            _triangles.Add(i * 2); _triangles.Add(i * 2 + 3); _triangles.Add(i * 2 + 2);
        }


        _mesh.vertices = _verticles.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _meshFilter.mesh = _mesh;
    }

    private void UpdateMesh(List<Vector3> drawPoints)
    {
        for (int i = 0; i < drawPoints.Count; i++)
        {
            Vector3 normal;
            if (i == drawPoints.Count - 1)
            {
                Vector3 d1 = drawPoints[i] - drawPoints[i - 1];
                normal = Quaternion.Euler(0, 0, -90) * d1.normalized;

                _verticles[i * 2] = drawPoints[i] + normal * _width;
                _verticles[i * 2 + 1] = drawPoints[i] - normal * _width;
            }
            else if (i == 0)
            {
                normal = Vector3.up;

                _verticles[i * 2] = drawPoints[i] - normal * _width;
                _verticles[i * 2 + 1] = drawPoints[i] + normal * _width;
            }
            else
            {
                Vector3 dir = drawPoints[i + 1] - drawPoints[i];
                normal = Quaternion.Euler(0, 0, -90) * dir.normalized;

                _verticles[i * 2] = drawPoints[i] + normal * _width;
                _verticles[i * 2 + 1] = drawPoints[i] - normal * _width;
            }
        }

        _mesh.vertices = _verticles.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _meshFilter.mesh = _mesh;
    }


}
