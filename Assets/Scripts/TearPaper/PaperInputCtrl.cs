using System;
using UnityEngine;
using System.Collections;

public class PaperInputCtrl
{

    private Action<Vector3> _touchDownAction;

    public PaperInputCtrl(Action<Vector3> touchDownAction)
    {
        _touchDownAction = touchDownAction;
    }


    public void OnUpdate(float deltaTime)
    {
        if (Input.GetMouseButtonDown(0))
        {
            _touchDownAction.Invoke(Input.mousePosition);
        }
    }



}
