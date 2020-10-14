using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BasePointSpawn
{
    public abstract void Initi(params float[] paramsFloats);

    public abstract List<Vector3> Spawn();

}
