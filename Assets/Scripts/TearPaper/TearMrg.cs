using UnityEngine;
using System.Collections;

public class TearMrg : MonoBehaviour
{

    [SerializeField] private PaperView _paperView;

    private PaperInputCtrl _inputCtrl;


    private void Awake()
    {
        _inputCtrl = new PaperInputCtrl(_paperView.Check);
    }

    private void Start()
    {
        _paperView.OnIniti();
    }

    private void Update()
    {
        _inputCtrl.OnUpdate(Time.deltaTime);
    }




}
