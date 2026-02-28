using UnityEngine;

public class Estatico : MonoBehaviour
{
    public static void MensajeDeError(string mensaje)
    {
        Debug.LogError(mensaje);   
    }   

    public static void MensajeDeAdvertencia(string mensaje)
    {
        Debug.LogWarning(mensaje);
    }

    public static int Sumar(int numl, int num2)
    {
        return numl + num2;
    }
}
