using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dest.Math;

public class PaperView : MonoBehaviour
{

    [SerializeField] private float _lineWidth;
    [SerializeField] private Material _bgMaterial;
    [SerializeField] private Material _borderMaterial;


    private Mesh _bgMesh;
    private List<Vector3> _pathNodes;
    private List<Vector3> _nodePoints;

    public void OnIniti()
    {
        _pathNodes = new List<Vector3>();
        _nodePoints = new List<Vector3>();
        _pathNodes.Add(new Vector3(0, 0, 0));
        _pathNodes.Add(new Vector3(10, 0, 0));
        _pathNodes.Add(new Vector3(10, 0, 10));
        _pathNodes.Add(new Vector3(0, 0, 10));

        _nodePoints.Add(new Vector3(3, 0, 3));
        _nodePoints.Add(new Vector3(7, 0, 3));
        _nodePoints.Add(new Vector3(7, 0, 8));
        _nodePoints.Add(new Vector3(3, 0, 7));
        _nodePoints.Add(new Vector3(1, 0, 5));

        GeneratorBgMesh();
        GeneratorBoderMesh();
    }

    public void OnUpdate()
    {


    }


    public void Check(Vector3 centerPos)
    {

    }



    private void GeneratorBgMesh()
    {
        _bgMesh = new Mesh();
        GameObject bg = new GameObject("BGMesh");

        List<int> triangles = new List<int>();
        triangles.Add(0); triangles.Add(2); triangles.Add(1);
        triangles.Add(0); triangles.Add(3); triangles.Add(2);
        _bgMesh.vertices = _pathNodes.ToArray();
        _bgMesh.triangles = triangles.ToArray();

        bg.AddComponent<MeshFilter>().mesh = _bgMesh;
        bg.AddComponent<MeshRenderer>().material = _bgMaterial;
        bg.transform.position = new Vector3(0f, 0, 0f);
        bg.transform.SetParent(transform);
    }


    private void GeneratorBoderMesh()
    {
        Mesh borderMesh = new Mesh();
        GameObject border = new GameObject("Border");

        List<int> triangles = new List<int>();
        List<Vector3> verticles = new List<Vector3>();

        List<Vector3> outLine = new List<Vector3>();
        for (int i = 0; i < _nodePoints.Count - 1; i++)
        {
            int t = (i + 2) % _nodePoints.Count;
            Vector3 s = _nodePoints[i];
            Vector3 m = _nodePoints[i + 1];
            Vector3 e = _nodePoints[t];
            FillIntersectionPoints(s, m, e, outLine);
        }

        Vector3 s1 = _nodePoints[_nodePoints.Count - 1];
        Vector3 m1 = _nodePoints[0];
        Vector3 e1 = _nodePoints[1];
        FillIntersectionPoints(s1, m1, e1, outLine);


        verticles.AddRange(_nodePoints);
        Vector3 v = outLine[outLine.Count - 1];
        outLine.RemoveAt(outLine.Count - 1);
        outLine.Insert(0, v);


        for (int i = 0; i < outLine.Count; i++)
        {
            Vector3 v1 = outLine[i];
            verticles.Add(v1);
        }

        for (int i = 0; i < _nodePoints.Count - 1; i++)
        {
            triangles.Add(i); triangles.Add(i + 1); triangles.Add(i + 1 + _nodePoints.Count);
            triangles.Add(i); triangles.Add(i + 1 + _nodePoints.Count); triangles.Add(i + _nodePoints.Count);
        }
        
        triangles.Add(_nodePoints.Count - 1);
        triangles.Add(0);
        triangles.Add(_nodePoints.Count);

        triangles.Add(_nodePoints.Count - 1);
        triangles.Add(_nodePoints.Count);
        triangles.Add(_nodePoints.Count - 1 + _nodePoints.Count);


        borderMesh.vertices = verticles.ToArray();
        borderMesh.triangles = triangles.ToArray();

        border.AddComponent<MeshFilter>().mesh = borderMesh;
        border.AddComponent<MeshRenderer>().material = _borderMaterial;
        border.transform.position = new Vector3(0f, 0.01f, 0f);
        border.transform.SetParent(transform);
    }




    private void FillIntersectionPoints(Vector3 s1, Vector3 m1, Vector3 e2, List<Vector3> outLine)
    {
        Vector3 e1 = m1;
        Vector3 normal1 = Quaternion.Euler(0, 90, 0) * (e1 - s1).normalized;
        Vector2 c1 = new Vector2(s1.x, s1.z);
        Vector2 d1 = new Vector2(normal1.x, normal1.z);
        Line2 line1 = new Line2(c1 + d1 * _lineWidth, new Vector2(e1.x - s1.x, e1.z - s1.z));

        Vector3 s2 = m1;
        Vector3 normal2 = Quaternion.Euler(0, 90, 0) * (e2 - s2).normalized;
        Vector2 c2 = new Vector2(s2.x, s2.z);
        Vector2 d2 = new Vector2(normal2.x, normal2.z);
        Line2 line2 = new Line2(c2 + d2 * _lineWidth, new Vector2(e2.x - s2.x, e2.z - s2.z));

        Line2Line2Intr intr;
        bool b = Intersection.FindLine2Line2(ref line1, ref line2, out intr);
        if (b)
        {
            Vector2 p = intr.Point;
            Vector3 p1 = new Vector3(p.x, 0, p.y);
            outLine.Add(p1);
        }
    }





}
