using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 move;
    public Vector2 look;
    public bool isRunning;

    public float yRotation;

    public bool shoot;

    public Vector3 fireDirection; // ✅ dirección del disparo
}
