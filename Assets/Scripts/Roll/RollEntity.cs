using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RollEntity : MonoBehaviour
{

    private RollView2D _rollView;


    public List<Vector3> PointVector3S { set; get; }



    public void OnIniti(BasePointSpawn pointSpawn)
    {
        PointVector3S = new List<Vector3>();
        PointVector3S = pointSpawn.Spawn();

        if (_rollView == null)
        {
            _rollView = gameObject.AddComponent<RollView2D>();
        }
        _rollView.OnIniti(PointVector3S);
    }

    public void OnIniti(List<Vector3> drawPoint, Vector3 edgePoint1, Vector3 edgePoint2)
    {

    }

    public void OnUpdate()
    {
        _rollView.OnUpdate();
    }



}
