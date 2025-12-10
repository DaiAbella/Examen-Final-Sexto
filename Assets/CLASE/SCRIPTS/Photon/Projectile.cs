using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;
using System.Collections;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private int damage = 25;
    [SerializeField] private int lifetime = 5;

    private PlayerRef shooter;
    private KCC kcc;

    public override void Spawned()
    {
        kcc = GetComponent<KCC>();
        if (kcc != null)
        {
            kcc.SetKinematicVelocity(transform.forward * speed);
            Debug.Log($"[Projectile] Velocidad inicial con KCC: {transform.forward * speed}");
        }
        else
        {
            Debug.LogError("[Projectile] No se encontró KCC en el proyectil.");
        }

        StartCoroutine(DespawnAfterTime());
    }

    public void SetProjectile(PlayerRef shooter, int damage)
    {
        this.shooter = shooter;
        this.damage = damage;
    }

    private IEnumerator DespawnAfterTime()
    {
        yield return new WaitForSeconds(lifetime);
        if (Object != null && Object.IsValid)
        {
            Runner.Despawn(Object);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Projectile] Impacto con: {other.name}");

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage, shooter);
            Debug.Log($"[Projectile] Daño aplicado: {damage}");
        }

        if (Object.HasStateAuthority)
        {
            Runner.Despawn(Object);
        }
    }
}
