using System;
using UnityEngine;
using UnityEngine.Events;

public class Eventos : MonoBehaviour
{
    [SerializeField] UnityEvent eventoDeEjemplo;

    public event Action eventos;
    PhotonManager photonmanager; 

    private void OnEnable()
    {
        eventos += Evento1;
        eventos += Evento2;
        eventos += Evento3;
    }
    private void OnDisable()
    {
        eventos -= Evento1;
        eventos -= Evento2;
        eventos -= Evento3;
    }

    void Start()
    {

        eventos.Invoke();
        eventos -= Evento2;
    }

    [ContextMenu("Invocar Eventos")]
    public void Evento1()
    {
        Debug.Log("Evento1");
    }

    public void Evento2()
    {
        Debug.Log("Evento2");
        eventos -= Evento2;
    }

    public void Evento3()
    {
        Debug.Log("Evento3");
    }
}
