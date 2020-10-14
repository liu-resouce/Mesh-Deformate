using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roll2D : MonoBehaviour
{


    [SerializeField] private RollEntity _rollEntity;




    private void Awake()
    {
        BasePointSpawn s = new ArchimedesSpiresPointSpawn();
        s.Initi(810, 0.1f, 0.1f);
        _rollEntity.OnIniti(s);
    }


    private void Start()
    {
        
    }


    private void Update()
    {
        _rollEntity.OnUpdate();
    }


}
