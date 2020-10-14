using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dest.Math;
using Dest.Math.Tests;

public class RollView3D : MonoBehaviour
{


    [SerializeField] private float _moveSpeed = 200f;



    private bool _isSpread;
    private bool _isRoll;
    private float _thickness;
    private float _length;
    private int _rotateIndex;
    private float _totalTime;
    private float _curTime;
    private float _angle;

    private Vector3 _moveVector3;
    private Vector3 _dirVector3;
    private Vector3 _targetVector3;

    private Dictionary<int, List<Vector2>> _cutIntersectionCache;
    private List<Vector3> _localVector3S;
    private Dictionary<int, List<Vector3>> _offsetsCache;
    private Dictionary<int, List<V3>> _cutTopCache;
    private Dictionary<int, List<Vector3>> _cutBottomCache;
    private Dictionary<int, Vector3> _normalCache;


    private List<Vector3> _referenceVerticles;
    private List<Vector3> _topVerticles;
    private List<Vector3> _bottomVerticles;
    private List<Vector3> _surroundVerticles;
    private List<int> _leftInts;
    private List<int> _rightInts;

    private List<int> _topTriangles;
    private List<int> _bottomTriangles;
    private List<int> _surroundTriangles;

    private List<Vector2> _uvList;
    private List<Vector3> _cutPoints;
    private List<Vector3> _originPoints;

    private Mesh _topMesh;
    private Mesh _bottomMesh;
    private Mesh _surroundMesh;

    private MeshFilter _topMeshFilter;
    private MeshFilter _bottomMeshFilter;
    private MeshFilter _surroundMeshFilter;

    private Material _topMaterial;
    private Material _bottomMaterial;
    private Material _surroundMaterial;

    private RollType _rollType;



    public void OnInitiMat(Material top, Material bottom, Material surround)
    {
        _topMaterial = top;
        _bottomMaterial = bottom;
        _surroundMaterial = surround;
    }

    public void OnIniti(List<Vector3> drawPoints, Vector3 edgePoint1, Vector3 edgePoint2)
    {
        _rollType = RollType.RollReady;

        _localVector3S = new List<Vector3>();
        _offsetsCache = new Dictionary<int, List<Vector3>>();
        _originPoints = new List<Vector3>();
        _normalCache = new Dictionary<int, Vector3>();

        _cutTopCache = new Dictionary<int, List<V3>>();
        _cutIntersectionCache = new Dictionary<int, List<Vector2>>();
        _leftInts = new List<int>();
        _rightInts = new List<int>();

        GeneratorMesh(drawPoints, edgePoint1, edgePoint2);
    }

    public void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            switch (_rollType)
            {
                case RollType.RollReady:
                    _rollType = RollType.Rolling;
                    _rotateIndex = _cutPoints.Count - 2;
                    RollReiniti();
                    break;
                case RollType.SpreadReady:
                    _rollType = RollType.Spreading;
                    _rotateIndex = 0;
                    SpreadReiniti();
                    break;
            }
        }
        
        RollUpdate();
        SpreadUpdate();
    }



    private void GeneratorMesh(List<Vector3> drawPoints, Vector3 edgePoint1, Vector3 edgePoint2)
    {
        _thickness = 0.05f;
        _topVerticles = new List<Vector3>();
        _topTriangles = new List<int>();

        _bottomVerticles=new List<Vector3>();
        _bottomTriangles = new List<int>();

        _surroundVerticles = new List<Vector3>();
        _surroundTriangles = new List<int>();

        _referenceVerticles = new List<Vector3>();
        float max = 0;

        Vector2 e1 = new Vector2(edgePoint1.x, edgePoint1.z);
        Vector2 e2 = new Vector2(edgePoint2.x, edgePoint2.z);
        Line2 line2 = new Line2(e1, e2 - e1);
        for (int i = 0; i < drawPoints.Count; i++)
        {
            Vector2 p = new Vector2(drawPoints[i].x, drawPoints[i].z);
            float l = Distance.Point2Line2(ref p, ref line2);
            if (l > max && Mathf.Abs(l - max) > 0.01f)
            {
                max = l;
            }
        }

        _length = max;
        float a = GetConstantA(6.5f * Mathf.PI, max);
        _dirVector3 = (edgePoint2 - edgePoint1).normalized;
        _moveVector3 = Quaternion.AngleAxis(-90, Vector3.up) * _dirVector3;
        FillMeshVerticles(a, e1, e2, drawPoints);

        InitiFace();
    }

    private float GetConstantA(float angle, float length)
    {
        float v = angle + Mathf.Sqrt(1 + angle * angle);
        float e = (float)Math.E;
        float t = angle / 2 * Mathf.Sqrt(1 + angle * angle) + Mathf.Log(v, e) / 2f;

        return length / t;
    }

    private void FillMeshVerticles(float a, Vector2 edgePoint1, Vector2 edgePoint2, List<Vector3> drawPoints)
    {
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < drawPoints.Count; i++)
        {
            Vector3 t = drawPoints[i];
            points.Add(new Vector2(t.x, t.z));
        }
        var ps = GetArchimePoints(a, edgePoint1);

        Polygon2 polygon2 = new Polygon2(points.ToArray());
        Line2ConvexPolygon2Intr intr;
        _cutPoints = new List<Vector3>();
        for (int i = 0; i < ps.Count; i++)
        {
            _cutPoints.Add(new Vector3(ps[i].x, 0, ps[i].y));
        }

        int startIndex = 0;
        for (int i = 0; i < _cutPoints.Count; i++)
        {
            Vector2 center1 = new Vector2(_cutPoints[i].x, _cutPoints[i].z);
            Line2 line2 = new Line2(center1, edgePoint2 - edgePoint1);
            bool b = Intersection.FindLine2ConvexPolygon2(ref line2, polygon2, out intr);
            if (b)
            {
                switch (intr.IntersectionType)
                {
                    case IntersectionTypes.Point:
                        _cutIntersectionCache.Add(i, new List<Vector2>() { intr.Point0 });
                        break;
                    case IntersectionTypes.Segment:
                        float angle = Vector2.Angle(intr.Point1 - intr.Point0, edgePoint2 - edgePoint1);
                        _cutIntersectionCache.Add(i,
                            Mathf.Abs(angle) < 10f
                                ? new List<Vector2>() { intr.Point0, intr.Point1 }
                                : new List<Vector2>() { intr.Point1, intr.Point0 });
                        break;
                }
            }

            if (i >= 1)
            {

                bool isAdd = false;
                Vector2 c1 = new Vector2(_cutPoints[i - 1].x, _cutPoints[i - 1].z);
                Vector2 c2 = new Vector2(_cutPoints[i].x, _cutPoints[i].z);
                float maxlength = Vector3.Distance(c1, c2);

                _cutTopCache.Add(i - 1, new List<V3>());
                if (_cutIntersectionCache.ContainsKey(i - 1))
                {
                    for (int j = 0; j < _cutIntersectionCache[i - 1].Count; j++)
                    {
                        var p = _cutIntersectionCache[i - 1][j];
                        Vector3 temp = new Vector3(p.x, 0, p.y);
                        int flag;
                        if (_cutIntersectionCache[i - 1].Count == 1)
                            flag = 0;
                        else
                            flag = j == 0 ? -1 : 1;
                        _cutTopCache[i - 1].Add(new V3(temp, true, i - 1, flag * i));
                    }
                }


                Line2 l1 = new Line2(c1, edgePoint2 - edgePoint1);
                for (int j = startIndex; j < drawPoints.Count; j++)
                {
                    Vector3 temp = drawPoints[j];
                    Vector2 p2 = new Vector2(temp.x, temp.z);
                    float d = Distance.Point2Line2(ref p2, ref l1);

                    if (d < maxlength && Mathf.Abs(d) > 0.05f && Mathf.Abs(d - maxlength) > 0.05f)
                    {
                        _cutTopCache[i - 1].Add(new V3(temp, false, -1, int.MaxValue));
                    }
                    else
                    {
                        if (!isAdd)
                        {
                            isAdd = true;
                            startIndex = j;

                            if (_cutIntersectionCache.ContainsKey(i))
                            {
                                for (int k = _cutIntersectionCache[i].Count - 1; k >= 0; k--)
                                {
                                    var p = _cutIntersectionCache[i][k];
                                    Vector3 temp1 = new Vector3(p.x, 0, p.y);
                                    int flag;
                                    if (_cutIntersectionCache[i].Count == 1)
                                        flag = 0;
                                    else
                                        flag = k == 0 ? -1 : 1;

                                    _cutTopCache[i - 1].Add(new V3(temp1, true, i, flag * (i + 1)));
                                }

                            }
                        }
                    }
                }
            }
        }
    }



    private List<Vector2> GetArchimePoints(float a, Vector2 center)
    {
        List<Vector2> archimeList = new List<Vector2>();
        _originPoints = CurvesTools.GetArchimedesPoint(a, 0.1f, 360 * 3 + 90);
        float length = 0;
        for (int i = 0; i < _originPoints.Count; i++)
        {
            Vector2 v = new Vector2(_moveVector3.x, _moveVector3.z);
            if (i == 0)
                length = 0;
            else
                length += (_originPoints[i] - _originPoints[i - 1]).magnitude;
            Vector2 v1 = center + v * length;
            archimeList.Add(v1);
        }
        return archimeList;
    }

    private void InitiFace()
    {

        GameObject bottom = new GameObject("Bottom");
        bottom.transform.SetParent(transform);
        bottom.transform.localPosition = Vector3.zero;
        bottom.transform.localScale = Vector3.one;
        bottom.AddComponent<MeshRenderer>().material = _bottomMaterial;
        _bottomMeshFilter = bottom.AddComponent<MeshFilter>();
        _bottomMesh = new Mesh();

        GameObject top = new GameObject("Top");
        top.transform.SetParent(transform);
        top.transform.localPosition = Vector3.zero;
        top.transform.localScale = Vector3.one;
        top.AddComponent<MeshRenderer>().material = _topMaterial;
        _topMeshFilter = top.AddComponent<MeshFilter>();
        _topMesh = new Mesh();

        GameObject surround = new GameObject("Surround");
        surround.transform.SetParent(transform);
        surround.transform.localPosition = Vector3.zero;
        surround.transform.localScale = Vector3.one;
        surround.AddComponent<MeshRenderer>().material = _surroundMaterial;
        _surroundMeshFilter = surround.AddComponent<MeshFilter>();
        _surroundMesh = new Mesh();



        int index = 0;
        int verticleCount = 0;
        List<int> tempInts = new List<int>();

        foreach (var item in _cutTopCache)
        {
            List<Vector3> points = new List<Vector3>();
            var list = item.Value;
            int start = GetCutPointFirstIndex(list);
            bool isRight = true;
            for (int i = 0; i < list.Count; i++)
            {
                int cutIndex = (i + start) % list.Count;
                int t1 = 0;
                if (list[cutIndex].IsCutPoint)
                {
                    t1 = list[cutIndex].CutSegmentIndex;
                    if (t1 < 0 && isRight)
                    {
                        isRight = false;
                    }
                }
                if (isRight)
                {
                    if (list[cutIndex].IsCutPoint)
                    {
                        if (!tempInts.Contains(t1))
                        {
                            tempInts.Add(t1);
                            _rightInts.Add(cutIndex + verticleCount);
                        }
                          
                    }
                    else
                    {
                        _rightInts.Add(cutIndex + verticleCount);
                    }
                }
                else
                {
                    if (list[cutIndex].IsCutPoint)
                    {
                        if (!tempInts.Contains(t1))
                        {
                            tempInts.Add(t1);
                            _leftInts.Add(cutIndex + verticleCount);
                        }
                    }
                    else
                    {
                        _leftInts.Add(cutIndex + verticleCount);
                    }
                }
                points.Add(list[i].ToVector3());
            }

            verticleCount += points.Count;
            _offsetsCache.Add(item.Key, new List<Vector3>());

            Vector3 normal = CurvesTools.GetPolygonNormal(points);
            for (int i = 0; i < points.Count; i++)
            {
                _referenceVerticles.Add(points[i]);

                _bottomVerticles.Add(points[i] - normal * _thickness);
                _topVerticles.Add(points[i] + normal * _thickness);
                Vector3 offset = points[i] - _cutPoints[item.Key];
                Vector3 weights;
                CurvesTools.SetAxisWeight(out weights, _dirVector3, _moveVector3, Vector3.up, offset);
                _offsetsCache[item.Key].Add(weights);
            }
            _normalCache.Add(item.Key, Vector3.zero);

            var t = CurvesTools.GetTriByPoins(points).ToList();
            t.Reverse();
            for (int i = 0; i < t.Count; i++)
            {
                t[i] += index;
            }
            index += points.Count;
            _bottomTriangles.AddRange(t);
        }
       
        _bottomMesh.vertices = _bottomVerticles.ToArray();
        _bottomMesh.triangles = _bottomTriangles.ToArray();
        _bottomMeshFilter.mesh = _bottomMesh;

        for (int i = 0; i < _bottomTriangles.Count; i++)
        {
            _topTriangles.Add(_bottomTriangles[i]);
        }

        _topTriangles.Reverse();
        _topMesh.vertices = _topVerticles.ToArray();
        _topMesh.triangles = _topTriangles.ToArray();
        _topMeshFilter.mesh = _topMesh;


        int temp = _leftInts[0];
        _leftInts.RemoveAt(0);
        _leftInts.Insert(1, temp);
        _leftInts.Reverse();
        _rightInts.AddRange(_leftInts);
        for (int i = 0; i < _rightInts.Count; i++)
        {
            _surroundVerticles.Add(_bottomVerticles[_rightInts[i]]);
            _surroundVerticles.Add(_topVerticles[_rightInts[i]]);
        }
        for (int i = 0; i < _rightInts.Count - 1; i++)
        {
            _surroundTriangles.Add(i * 2); _surroundTriangles.Add((i + 1) * 2 + 1); _surroundTriangles.Add(i * 2 + 1);
            _surroundTriangles.Add(i * 2); _surroundTriangles.Add((i + 1) * 2); _surroundTriangles.Add((i + 1) * 2 + 1);
        }

        _surroundVerticles.Reverse();
        _surroundMesh.vertices = _surroundVerticles.ToArray();
        _surroundMesh.triangles = _surroundTriangles.ToArray();
        _surroundMeshFilter.mesh = _surroundMesh;

    }

    private void UpdateMesh(List<Vector3> cutPoints)
    {
        int index = 0;
        List<Vector3> plane = new List<Vector3>();
        foreach (var item in _cutTopCache)
        {
            Vector3 dir = cutPoints[item.Key + 1] - cutPoints[item.Key];

            for (int i = 0; i < item.Value.Count; i++)
            {
                float x = _offsetsCache[item.Key][i].x;
                float y = _offsetsCache[item.Key][i].y;
                Vector3 t = x * _dirVector3 + y * dir.normalized;
                _referenceVerticles[i + index] = t + cutPoints[item.Key];
                plane.Add(_referenceVerticles[i + index]);
            }

            Vector3 normal = CurvesTools.GetPolygonNormal(plane);
            _normalCache[item.Key] = normal;
            plane.Clear();

            for (int i = 0; i < item.Value.Count; i++)
            {
                if (item.Value[i].IsCutPoint)
                {
                    if (item.Value[i].CutIndex == 0)
                    {
                        _topVerticles[i + index] = _referenceVerticles[i + index] + _normalCache[0] * _thickness;
                        _bottomVerticles[i + index] = _referenceVerticles[i + index] - _normalCache[0] * _thickness;
                    }
                    else
                    {
                        int k = item.Value[i].CutIndex - 1;
                        _topVerticles[i + index] = _referenceVerticles[i + index] + _normalCache[k] * _thickness;
                        _bottomVerticles[i + index] = _referenceVerticles[i + index] - _normalCache[k] * _thickness;
                    }
                }
                else
                {
                    _topVerticles[i + index] = _referenceVerticles[i + index] + _normalCache[item.Key] * _thickness;
                    _bottomVerticles[i + index] = _referenceVerticles[i + index] - _normalCache[item.Key] * _thickness;
                }
               
            }

            index += item.Value.Count;
        }


        for (int i = 0; i < _rightInts.Count; i++)
        {
            _surroundVerticles[i * 2] = _bottomVerticles[_rightInts[i]];
            _surroundVerticles[i * 2 + 1] = _topVerticles[_rightInts[i]];
        }

        _bottomMesh.vertices = _bottomVerticles.ToArray();
        _bottomMesh.triangles = _bottomTriangles.ToArray();

        _topMesh.vertices = _topVerticles.ToArray();
        _topMesh.triangles = _topTriangles.ToArray();

        _surroundMesh.vertices = _surroundVerticles.ToArray();
        _surroundMesh.triangles = _surroundTriangles.ToArray();

    }



    private void RollUpdate()
    {
        if (_rollType == RollType.Rolling)
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
            _rollType = RollType.SpreadReady;
            return;
        }
        if (_rotateIndex + 1 < _cutPoints.Count)
        {
            var t1 = _originPoints[_rotateIndex + 1] - _originPoints[_rotateIndex];
            var t2 = _originPoints[_rotateIndex] - _originPoints[_rotateIndex - 1];

            _length = t1.magnitude;
            _curTime = 0;
            _angle = Vector3.Angle(t1, t2);
            _totalTime = _angle / _moveSpeed;
        }

        if (_rotateIndex != _cutPoints.Count - 2)
        {
            for (int i = _rotateIndex + 2; i < _cutPoints.Count; i++)
            {
                Vector3 offset = _cutPoints[i] - _cutPoints[_rotateIndex + 1];
                _localVector3S.Add(offset);
            }
        }
    }

    private void UpdateRollVerticles(float percent)
    {
        if (_rotateIndex == _cutPoints.Count - 2)
        {
            Vector3 t = Quaternion.AngleAxis(-percent * _angle, _dirVector3) * _moveVector3.normalized;
            _cutPoints[_rotateIndex + 1] = _cutPoints[_rotateIndex] + t * _length;
        }
        else
        {
            Vector3 t = Quaternion.AngleAxis(-percent * _angle, _dirVector3) * _moveVector3.normalized;
            if (_rotateIndex + 1 < _cutPoints.Count)
            {
                _cutPoints[_rotateIndex + 1] = _cutPoints[_rotateIndex] + t * _length;
            }
        
            for (int i = _rotateIndex + 2; i < _cutPoints.Count; i++)
            {
                Vector3 offset = Quaternion.AngleAxis(-percent * _angle, _dirVector3) * _localVector3S[i - _rotateIndex - 2];
                _cutPoints[i] = _cutPoints[_rotateIndex + 1] + offset;
            }
        }
        UpdateMesh(_cutPoints);
    }



    private void SpreadUpdate()
    {
        if (_rollType == RollType.Spreading)
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
        if (_rotateIndex >= _cutPoints.Count - 1)
        {
            _rollType = RollType.RollReady;
            return;
        }
        if (_rotateIndex + 1 < _cutPoints.Count)
        {
            _targetVector3 = _cutPoints[_rotateIndex + 1] - _cutPoints[_rotateIndex];
            _length = _targetVector3.magnitude;
            _curTime = 0;
            _angle = Vector3.Angle(_targetVector3, _moveVector3);
            _totalTime = _angle / _moveSpeed;
        }

        for (int i = _rotateIndex + 2; i < _cutPoints.Count; i++)
        {
            Vector3 offset = _cutPoints[i] - _cutPoints[_rotateIndex + 1];
            _localVector3S.Add(offset);
        }
    }

    private void UpdateSpreadVerticles(float percent)
    {
        Vector3 d = Vector3.Lerp(_targetVector3, _moveVector3 * _length, percent);
        if (_rotateIndex + 1 < _cutPoints.Count)
        {
            _cutPoints[_rotateIndex + 1] = _cutPoints[_rotateIndex] + d;
        }
        for (int i = _rotateIndex + 2; i < _cutPoints.Count; i++)
        {
            Vector3 offset = Quaternion.AngleAxis(percent * _angle, _dirVector3) * _localVector3S[i - _rotateIndex - 2];
            _cutPoints[i] = _cutPoints[_rotateIndex + 1] + offset;
        }
        UpdateMesh(_cutPoints);
    }



    private int GetCutPointFirstIndex(List<V3> cutPoints)
    {
        int value = 0;
        for (int i = 0; i < cutPoints.Count; i++)
        {
            var temp = cutPoints[i];
            if (temp.IsCutPoint)
            {
                if (temp.CutSegmentIndex >= 0)
                {
                    return i;
                }
            }
        }
        return value;
    }



}
