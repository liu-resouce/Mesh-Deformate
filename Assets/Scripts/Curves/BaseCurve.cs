using UnityEngine;
using System.Collections;

public abstract class BaseCurve
{
    public Transform Pendulum { set; get; }

    public abstract void InitiPendulum(Transform transform);

    public abstract void Initi(params float[] paramsFloats);

    public abstract void OnUpdate();

}
