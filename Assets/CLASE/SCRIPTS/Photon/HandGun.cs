using Fusion;
using UnityEngine;

public class HandGun : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef projectilePrefab;
    [SerializeField] private Transform firePoint;

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (data.shoot)
            {
                FireProjectile();
            }
        }
    }

    private void FireProjectile()
    {
        if (Runner != null && projectilePrefab != null && firePoint != null)
        {
            Debug.Log("Disparo realizado por: " + Object.InputAuthority);

            Runner.Spawn(
                projectilePrefab,
                firePoint.position,
                firePoint.rotation,
                Object.InputAuthority
            );
        }
    }
}
