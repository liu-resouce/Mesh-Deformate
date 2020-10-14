using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Dest.Math;

public class Roll3D : MonoBehaviour
{

    [SerializeField] private Material _topMaterial;
    [SerializeField] private Material _bottomMaterial;
    [SerializeField] private Material _surroundMaterial;


    private RollView3D _rollView3D;


    private void Awake()
    {
        _rollView3D = gameObject.AddComponent<RollView3D>();
    }

    private void Start()
    {
        List<Vector3> drawList = new List<Vector3>();

        drawList.Add(new Vector3(0, 0, 0));
        drawList.Add(new Vector3(5, 0, 0));
        drawList.Add(new Vector3(6, 0, 6));
        drawList.Add(new Vector3(5, 0, 8));
        drawList.Add(new Vector3(0, 0, 8));

        Vector3 edge1 = new Vector3(6, 0, 0);
        Vector3 edge2 = new Vector3(6, 0, 1);
        _rollView3D.OnInitiMat(_topMaterial, _bottomMaterial, _surroundMaterial);
        _rollView3D.OnIniti(drawList, edge1, edge2);
    }

    private void Update()
    {

    }

    private void LateUpdate()
    {
        _rollView3D.OnUpdate();
    }



}
