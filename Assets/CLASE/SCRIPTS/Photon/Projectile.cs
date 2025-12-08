using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private int damage = 25;

    private Rigidbody rb;

    public override void Spawned()
    {
        Debug.Log($"[Projectile] Instanciado por: {Object.InputAuthority}");
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = transform.forward * speed;
            Debug.Log($"[Projectile] Velocidad inicial asignada: {rb.linearVelocity}");
        }
        else
        {
            Debug.LogError("[Projectile] No se encontró Rigidbody en el proyectil.");
        }

        Invoke(nameof(Despawn), 5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Projectile] Impacto con: {collision.collider.name}");

        // Fuerza física si el objeto impactado tiene Rigidbody
        Rigidbody hitRb = collision.rigidbody;
        if (hitRb != null)
        {
            Vector3 forceDirection = transform.forward;
            float impactForce = 10f;
            hitRb.AddForce(forceDirection * impactForce, ForceMode.Impulse);
            Debug.Log($"[Projectile] Fuerza aplicada a {collision.collider.name}: {forceDirection * impactForce}");
        }

        // Buscar PlayerHealth en el objeto impactado o en sus padres
        PlayerHealth health = collision.collider.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            Debug.Log($"[Projectile] Aplicando daño: {damage} a {collision.collider.name}");
            health.TakeDamage(damage, Object.InputAuthority);
        }
        else
        {
            Debug.LogWarning($"[Projectile] No se encontró PlayerHealth en {collision.collider.name} o sus padres.");
        }

        // Desaparecer el proyectil
        if (Object.HasStateAuthority)
        {
            Debug.Log("[Projectile] Despawning proyectil tras impacto.");
            Runner.Despawn(Object);
        }
    }

    private void Despawn()
    {
        if (Object != null && Object.IsValid)
        {
            Debug.Log("[Projectile] Despawning proyectil por timeout.");
            Runner.Despawn(Object);
        }
    }
}
