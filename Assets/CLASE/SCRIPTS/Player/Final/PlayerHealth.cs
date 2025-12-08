using Fusion;
using UnityEngine;
using System.Collections;
using Fusion.Addons.KCC;

public class PlayerHealth : NetworkBehaviour
{
    [Networked] public int CurrentHealth { get; set; }
    [SerializeField] private int maxHealth = 100;

    private bool isDead = false;

    public override void Spawned()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount, PlayerRef attacker)
    {
        if (isDead) return;

        CurrentHealth -= amount;
        Debug.Log("Daño recibido: " + amount);

        if (CurrentHealth <= 0)
        {
            Die(attacker);
        }
    }

    private void Die(PlayerRef attacker)
    {
        isDead = true;
        GameManager.Instance.AddKill(attacker);

        GetComponent<KCC>().enabled = false;

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }

        Runner.StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        CurrentHealth = maxHealth;
        isDead = false;

        // ✅ Pedir un nuevo spawn cada vez
        Transform newSpawn = GameManager.Instance.GetRandomSpawnPoint();
        if (newSpawn != null)
        {
            transform.position = newSpawn.position;
            transform.rotation = newSpawn.rotation;
            Debug.Log("Respawn en nuevo punto: " + newSpawn.name);
        }
        else
        {
            Debug.LogError("No se encontró un spawn point en GameManager.");
        }

        GetComponent<KCC>().enabled = true;

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = true;
        }
    }
}
