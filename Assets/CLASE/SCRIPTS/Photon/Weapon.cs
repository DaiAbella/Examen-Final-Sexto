using Fusion;
using UnityEngine;

public abstract class Weapon : NetworkBehaviour
{

    [SerializeField] public ShootType type;
    [SerializeField] protected Transform shootPoint;
    [SerializeField] protected NetworkPrefabRef bullet;
    [SerializeField] protected Camera playerCam;

    [SerializeField] protected float damage;
    [SerializeField] protected float range;
    [SerializeField] protected int actualAmmo;
    [SerializeField] protected LayerMask LayerMask;

    public abstract void RigidBodyShoot();

    public abstract void RpcRaycastShoot();

}

public enum ShootType
{
    RigidBody, Raycast
}