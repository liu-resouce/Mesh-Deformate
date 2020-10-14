using System.Collections.Generic;
using UnityEngine;

public class ArchimedesSpiresPointSpawn : BasePointSpawn
{

    private int _totalAngle;
    private float _a;
    private float _b;


    public override void Initi(params float[] paramsFloats)
    {
        _totalAngle = (int)paramsFloats[0];
        _a = paramsFloats[1];
        _b = paramsFloats[2];
    }


    public override List<Vector3> Spawn()
    {
        List<Vector3> values = new List<Vector3>();
        List<Vector3> temps = new List<Vector3>();

        float t = Mathf.Deg2Rad * _totalAngle;
        for (int i = _totalAngle; i >= 0; i -= 10)
        {
            float angle = Mathf.Deg2Rad * i;
            float r = _a + _b * angle;

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
            if (Vector3.Dot(temps[index] - curVector3, targetV3) < _a)
            {
                float l = 0;
                float left = _a - (temps[index] - curVector3).magnitude;
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
                curVector3 += _a * targetV3;
                values.Add(curVector3);
            }
        }
        return values;
    }


}