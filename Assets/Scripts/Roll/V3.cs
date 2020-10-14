using UnityEngine;
using System.Collections;

public struct V3
{

    public float X;
    public float Y;
    public float Z;


    public int CutIndex { set; get; }

    public bool IsCutPoint { set; get; }

    public int CutSegmentIndex { set; get; }

    public V3(Vector3 vector3, bool isCutPoint, int cutIndex, int segmentIndex)
    {
        X = vector3.x;
        Y = vector3.y;
        Z = vector3.z;

        IsCutPoint = isCutPoint;
        CutIndex = cutIndex;
        CutSegmentIndex = segmentIndex;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(this.X, this.Y, this.Z);
    }

}
