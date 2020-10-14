using UnityEngine;
using System.Collections;

public class MeshDeformate : MonoBehaviour
{

    [SerializeField] private bool _isUsingPush;
    [SerializeField] private bool _isRepair;
    [SerializeField] private bool _isDrag;

    [Header("Push")]
    [SerializeField] private float _minYpoint = 0;
    [SerializeField] private float _pushRadius = 0.1f;
    [SerializeField] private float _pushIntsity = 0.2f;
    [SerializeField] private float _pushFalloff = 2f;

    [Header("Drag")]
    [SerializeField] private float _dragRadius = 0.5f;
    [SerializeField] private float _dragIntensity = 0.2f;
    [SerializeField] private float _dragFalloff = 5f;
    [SerializeField] private float DragStr = 0.1f;


    [SerializeField] private float _repairSpeed;
    [SerializeField] private float _rubber;
    [SerializeField] private float maxMagnitude = 0.2f;





    private Vector3[] _verts;
    private Vector3[] _originVerts;
    private Vector3[] _oldhPos = new Vector3[5];
    private Vector3[] _movePos = new Vector3[5];

    private float _pushMagnitude;
    private float _pushMagnitude2;
    private float _speedMagnitude;

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    private bool _firstPush;

    private void Awake()
    {
        _meshCollider = transform.GetComponent<MeshCollider>();
        _meshFilter = transform.GetComponent<MeshFilter>();
        _mesh = _meshFilter.mesh;

        _verts = _mesh.vertices;
        _originVerts = _mesh.vertices;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _firstPush = true;
        }
        if (Input.GetMouseButton(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider)
                {
                    ModifyMesh(hit, 0);
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {

        }
    }

    private void FixedUpdate()
    {
        if (_isRepair)
        {
            for (int i = 0; i < _verts.Length; i++)
            {
                Vector2 v1 = new Vector2(_verts[i].x, _verts[i].z);
                Vector2 v2 = new Vector2(_originVerts[i].x, _originVerts[i].z);
                float dist2 = 1 + Vector2.Distance(v1, v2) * _rubber;

                _verts[i] = Vector3.Lerp(_verts[i], _originVerts[i], _repairSpeed * Time.deltaTime * dist2);
            }

            _mesh.vertices = _verts;
            _meshCollider.sharedMesh = _mesh;
            _meshFilter.mesh = _mesh;
            _meshFilter.mesh.RecalculateNormals();
        }
    }


    private void ModifyMesh(RaycastHit hit, int tochId)
    {
        if (_firstPush)
        {
            _oldhPos[tochId] = hit.point;
            _firstPush = false;
        }

        _movePos[tochId] = _oldhPos[tochId] - hit.point;

        var point = transform.InverseTransformPoint(hit.point);
        _movePos[tochId].y = 0;
        _speedMagnitude = 0;
        if (_movePos[tochId].magnitude < maxMagnitude)
        {
            _speedMagnitude = maxMagnitude - _movePos[tochId].magnitude;
            _pushMagnitude = 1 + _movePos[tochId].magnitude * 2;
            _pushMagnitude2 = 1 + _movePos[tochId].magnitude * 2;
        }

        Debug.LogError(_movePos[tochId]);

        for (int i = 0; i < _verts.Length; i++)
        {
            float dist = Vector2.Distance(new Vector2(_verts[i].x, _verts[i].z), new Vector2(point.x, point.z));

            if (_isUsingPush)
            {
                if (point.y > _minYpoint)
                {
                    if (dist < _pushRadius)
                    {
                        _verts[i].y = Mathf.Lerp(_verts[i].y, 0,
                            Mathf.Pow(1 - Mathf.Pow(dist / _pushRadius, _pushFalloff * _pushMagnitude2), 2) *
                            _pushIntsity * _pushMagnitude);
                    }
                }
            }

            if (_isDrag)
            {
                if (dist < _dragRadius)
                {
                    float distance2 = Mathf.Pow((1 - Vector3.Distance(_verts[i], point) / _dragRadius / 2), _dragFalloff);
                    float drag = _dragIntensity * distance2 * DragStr * _speedMagnitude;
                    _verts[i] -= _movePos[tochId] * drag;
                }
            }

        }

        _oldhPos[tochId] = hit.point;
      
        _mesh.vertices = _verts;
        _meshCollider.sharedMesh = _mesh;
        _meshFilter.mesh = _mesh;
        _meshFilter.mesh.RecalculateNormals();
    }



}
