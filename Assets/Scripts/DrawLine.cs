using UnityEngine;
using System.Collections.Generic;

public class DrawLine : MonoBehaviour
{

    [SerializeField] private Transform _posT;
    [SerializeField] private Transform _directionT;
    [SerializeField] private Transform _pendulum;
    [SerializeField] private float _drawInterval;
    [SerializeField] private float _width;


    private float _totalTime;
    private float _addAngle;
    private bool _isCreate;
    private bool _isLoosen;
    private bool _isClick;


    private List<Vector3> _drawPoints;
    private List<Vector3> _verticles;
    private List<int> _triangles;

    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private Mesh _mesh;
    private RaycastHit _hit;

    private int _index;
    private int _colorIndex;
    private Vector3 _startPos;
    private Vector3 _clickPos;
    private Vector3 _mouseDownPos;
    private Vector2 _offset;
    private BaseCurve _baseCurve;


    private void Awake()
    {
        _colorIndex = 0;
        _verticles = new List<Vector3>();
        _drawPoints = new List<Vector3>();
        _triangles = new List<int>();

        Create();
    }

    private void Update()
    {
        if (_isLoosen)
        {
            _baseCurve.OnUpdate();
            _totalTime += Time.deltaTime;
            if (_totalTime >= _drawInterval)
            {
                DrawPoints("Ground");
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, 1000f, 1 << LayerMask.NameToLayer("Hammer")))
            {
                _isClick = true;
                _isLoosen = false;
                _clickPos = Camera.main.WorldToScreenPoint(_posT.position);
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (_isClick)
            {
                var v = Input.mousePosition;
                _offset = v - _clickPos;

                float z = _offset.sqrMagnitude / 1000f;
                z = Mathf.Clamp(z, 0, 30f);
                float angle = Vector2.Angle(_offset, Vector2.right);

                Vector3 v1 = Vector3.Cross(new Vector3(_offset.x, _offset.y, 0), Vector3.right);
                if (v1.z < 0)
                {
                    angle = 360 - angle;
                }
                _pendulum.transform.eulerAngles = new Vector3(0, angle, z);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (_isClick)
            {
                _isLoosen = true;
                float t = Mathf.Abs(_offset.x / 100f);
                CreateCurve(_offset.magnitude / 5f, t);
            }
        }
    }




    private void RefreshMesh()
    {
        if (!_isCreate)
            return;
        if (_drawPoints.Count == 2)
        {
            Vector3 dir = _drawPoints[1] - _drawPoints[0];
            Vector3 normal = Quaternion.Euler(0, -90, 0) * dir.normalized;
            _verticles.Add(_drawPoints[0] + normal * _width);
            _verticles.Add(_drawPoints[0] - normal * _width);
            _verticles.Add(_drawPoints[1] + normal * _width);
            _verticles.Add(_drawPoints[1] - normal * _width);

            _triangles.Add(0); _triangles.Add(2); _triangles.Add(1);
            _triangles.Add(1); _triangles.Add(2); _triangles.Add(3);

        }
        else if (_drawPoints.Count > 2)
        {
            int index = _drawPoints.Count - 1;
            Vector3 dir = _drawPoints[index] - _drawPoints[index - 1];
            Vector3 normal = Quaternion.Euler(0, -90, 0) * dir.normalized;
            _verticles.Add(_drawPoints[index] + normal * _width);
            _verticles.Add(_drawPoints[index] - normal * _width);
            int c = _verticles.Count - 1;

            _triangles.Add(c - 3); _triangles.Add(c - 1); _triangles.Add(c - 2);
            _triangles.Add(c - 2); _triangles.Add(c - 1); _triangles.Add(c);
        }
        _mesh.vertices = _verticles.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _meshFilter.mesh = _mesh;
    }

    private void CreateCurve(params float[] paramsFloats)
    {
        _baseCurve = new EllipseCurve();
        _baseCurve.InitiPendulum(_pendulum);
        _baseCurve.Initi(paramsFloats);
    }

    private void DrawPoints(string ground)
    {
        int layer = 1 << LayerMask.NameToLayer(ground);
        bool b1 = Physics.Raycast(_posT.position, -_directionT.up, out _hit, 100, layer);
        if (b1)
        {
            _drawPoints.Add(_hit.point + new Vector3(0, 0.1f, 0));
            RefreshMesh();
            if (_drawPoints.Count >= 200)
            {
                Create();
            }
        }
    }

    private void Create()
    {
        GameObject line = new GameObject("Line");
        line.transform.SetParent(transform);
        line.transform.localPosition = _startPos;

        _meshRenderer = line.AddComponent<MeshRenderer>();
        _meshFilter = line.AddComponent<MeshFilter>();
        _mesh = new Mesh();

        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.SetColor("_Color", Color.HSVToRGB(_colorIndex / 360f, 1, 1));
        _meshRenderer.material = mat;
        _isCreate = true;

        _verticles.Clear();
        _triangles.Clear();

        if (_drawPoints.Count > 0)
        {
            Vector3 t = _drawPoints[_drawPoints.Count - 1];
            _drawPoints.Clear();
            _drawPoints.Add(t);
        }
      
        _colorIndex += 5;
        _startPos += new Vector3(0, 0.0001f, 0);
    }



}
