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
                // Cliente pide disparo → host lo ejecuta
                Rpc_FireProjectile(data.fireDirection);
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void Rpc_FireProjectile(Vector3 direction)
    {
        if (Runner != null && projectilePrefab != null && firePoint != null)
        {
            Debug.Log("RPC Disparo realizado por: " + Object.InputAuthority);

            Quaternion rotation = Quaternion.LookRotation(direction);

            // ✅ Spawn del proyectil en el host
            var proj = Runner.Spawn(
                projectilePrefab,
                firePoint.position,
                rotation,
                Object.InputAuthority
            );

            // ✅ Asignar shooter y daño al proyectil
            if (proj != null)
            {
                Projectile projectileScript = proj.GetComponent<Projectile>();
                if (projectileScript != null)
                {
                    projectileScript.SetProjectile(Object.InputAuthority, 25);
                }
            }
        }
    }
}
