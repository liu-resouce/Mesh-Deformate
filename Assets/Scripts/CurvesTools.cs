using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dest.Math;
using Dest.Math.Tests;

public static class CurvesTools 
{



    public static List<Vector3> GetArchimedesPoint(float b, float minlength, int totalAngle)
    {
        List<Vector3> values = new List<Vector3>();
        List<Vector3> temps = new List<Vector3>();

        float t = Mathf.Deg2Rad * totalAngle;
        for (int i = totalAngle; i >= 0; i -= 10)
        {
            float angle = Mathf.Deg2Rad * i;
            float r = b * angle;

            float x = r * Mathf.Cos(t - angle - Mathf.PI / 2);
            float y = r * Mathf.Sin(t - angle - Mathf.PI / 2);
            Vector3 v = new Vector3(x, y, 0);

            temps.Add(v);
        }

        int index = 1;
        values.Add(temps[0]);
        Vector3 curVector3 = temps[0];
        Vector3 targetV3 = (temps[1] - temps[0]).normalized;

        while (true)
        {
            if (Vector3.Dot(temps[index] - curVector3, targetV3) < minlength)
            {
                float l = 0;
                float left = minlength - (temps[index] - curVector3).magnitude;
                float total = 0;
                int targetIndex = 0;

                for (int i = index; i < temps.Count; i++)
                {
                    if (i + 1 >= temps.Count - 1)
                    {
                        targetIndex = temps.Count - 1;
                        break;
                    }

                    total += (temps[i + 1] - temps[i]).magnitude;
                    if (total >= left)
                    {
                        targetIndex = i + 1;
                        l = total - left;
                        break;
                    }
                }

                if (targetIndex >= temps.Count - 1)
                    curVector3 = temps[temps.Count - 1];
                else
                    curVector3 = temps[targetIndex] - l * (temps[targetIndex] - temps[targetIndex - 1]).normalized;


                values.Add(curVector3);
                index = targetIndex;
                if (index >= temps.Count - 1)
                {
                    break;
                }
                targetV3 = (temps[index] - curVector3).normalized;
            }
            else
            {
                curVector3 += minlength * targetV3;
                values.Add(curVector3);
            }
        }
        return values;
    }

    public static float GetArchimeLength(float a, float angle)
    {
        float v = angle + Mathf.Sqrt(1 + angle * angle);
        float e = (float)Math.E;
        float t = angle / 2 * Mathf.Sqrt(1 + angle * angle) + Mathf.Log(v, e) / 2f;

        return t * a;
    }

    public static float GetDistancePoint2Line2(Vector2 point, Line2 line)
    {
        float length = Distance.Point2Line2(ref point, ref line);
        return length;
    }

    public static int[] GetTriByPoins(List<Vector3> points)
    {
        List<int> indices = new List<int>();
        bool[] markList = new bool[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            markList[i] = false;
        }
        int length = points.Count;
        int next = 0, pre = 0, mid = 0;
        int j = length;
        while (j > 2)
        {
            var threeIndex = GetThreeByorder(markList, mid, length);
            pre = threeIndex[0];
            mid = threeIndex[1];
            next = threeIndex[2];

            if (Snip(markList, points, next, mid, pre, length))
            {
                indices.Add(pre);
                indices.Add(mid);
                indices.Add(next);
                markList[mid] = true;
                j--;
            }
        }
        return indices.ToArray();
    }

    //判断点是顺时针添加，还是逆时针添加。如果返回的值大于0表示逆时针，否则就是顺时针。
    public static float Area(List<Vector3> points)
    {
        var n = points.Count;
        var A = 0.0f;
        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            var pval = points[p];
            var qval = points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }
        return (A * 0.5f);
    }
    //判断有没有其他的点在三角形内。
    public static bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x;
        ay = C.y - B.y;
        bx = A.x - C.x;
        by = A.y - C.y;
        cx = B.x - A.x;
        cy = B.y - A.y;
        apx = P.x - A.x;
        apy = P.y - A.y;
        bpx = P.x - B.x;
        bpy = P.y - B.y;
        cpx = P.x - C.x;
        cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }

    public static bool PointInTriangle(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        return SameSide(A, B, C, P) &&
               SameSide(B, C, A, P) &&
               SameSide(C, A, B, P);
    }

    public static Vector3 GetPolygonNormal(List<Vector3> polygonList)
    {
        for (int i = 0; i < polygonList.Count - 2; i++)
        {
            Vector3 d1 = polygonList[i + 1] - polygonList[i];
            Vector3 d2 = polygonList[i + 2] - polygonList[i + 1];
            float angle = Vector3.Angle(d1, d2);
            if (angle > 1f)
            {
                return Vector3.Cross(d1, d2).normalized;
            }
        }
        return Vector3.zero;
    }

    public static void SetAxisWeight(out Vector3 v, Vector3 b1, Vector3 b2, Vector3 b3,Vector3 target)
    {
        float m = GetFx(b1.x, b2.x, b3.x, b1.y, b2.y, b3.y, b1.z, b2.z, b3.z);
        if (Mathf.Abs(m) < 0.01f)
        {
            v = Vector3.zero;
            return;
        }

        v.x = GetFx(target.x, b2.x, b3.x, target.y, b2.y, b3.y, target.z, b2.z, b3.z) / m;
        v.y = GetFx(b1.x, target.x, b3.x, b1.y, target.y, b3.y, b1.z, target.z, b3.z) / m;
        v.z = GetFx(b1.x, b2.x, target.x, b1.y, b2.y, target.y, b1.z, b2.z, target.z) / m;
    }


    private static float GetFx(float a, float b, float c, float d, float e, float f, float g, float h, float i)
    {
        float result = a * e * i + b * f * g + c * d * h - a * f * h - b * d * i - c * e * g;
        return result;
    }













    private static bool SameSide(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
    {
        Vector3 AB = B - A;
        Vector3 AC = C - A;
        Vector3 AP = P - A;

        Vector3 v1 = Cross(AB, AC);
        Vector3 v2 = Cross(AB, AP);

     
        return Dot(v1, v2) >= 0;
    }

    private static Vector3 Cross(Vector3 v, Vector3 q)
    {
        return new Vector3(
            q.y * v.z - q.z * v.y,
            q.z * v.x - q.x * v.z,
            q.x * v.y - q.y * v.x);
    }

    private static float Dot(Vector3 v, Vector3 q)
    {
        return q.x * v.x + q.y * v.y + q.z * v.z;
    }

    private static int[] GetThreeByorder(bool[] mark, int origin, int length)
    {
        List<int> endValue = new List<int>();
        int num = 0;
        while (num < 3)
        {
            if (!mark[origin])
            {
                endValue.Add(origin);
                num++;
            }
            origin++;
            origin = origin % length;
        }
        return endValue.ToArray();
    }

    //判断连续的3个点是不是能组成三角形，如果不能就返回false，如果能并且还有其他的点在三角形内的话也返回false。
    //否则就返回true。
    private static bool Snip(bool[] mark, List<Vector3> points, int u, int v, int w, int n)
    {

        int p;
        var C = points[u];
        var B = points[v];
        var A = points[w];
        if (Mathf.Epsilon < (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;
        for (p = 0; p < n; p++)
        {
            if (mark[p])
                continue;
            var P = points[p];
            if ((A == P) || (B == P) || (C == P))
                continue;
            if (PointInTriangle(A, B, C, P))
            {
                return false;
            }
        }
        return true;
    }


}
