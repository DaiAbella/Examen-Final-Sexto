using Fusion;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [Networked] public string WeaponColor { get; set; }

    [Header("Player Skin Meshes")]
    public SkinnedMeshRenderer[] playerSkin;

    public override void Spawned()
    {
        Debug.Log("SPAWNED EJECUTADO");

        if (HasInputAuthority)
        {
          
            for (int i = 0; i < playerSkin.Length; i++)
            {
                playerSkin[i].enabled = false;
            }
        }
        else
        {
           
            for (int i = 0; i < playerSkin.Length; i++)
            {
                playerSkin[i].enabled = true;
            }
        }

      
        Debug.Log("[PlayerNetwork] Spawned() → Color sincronizado: " + WeaponColor);

        var weapon = GetComponentInChildren<WeaponColorApplier>();

        Debug.Log("[PlayerNetwork] ¿Encontré el arma? → " + (weapon != null));

        if (weapon != null)
        {
            weapon.ApplyColor(WeaponColor);
        }
        else
        {
            Debug.LogError("[PlayerNetwork] ERROR → No se encontró WeaponColorApplier en el jugador.");
        }
    }
}
