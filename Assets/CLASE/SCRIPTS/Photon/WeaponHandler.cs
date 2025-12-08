using Fusion;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] private Weapon actualWeapon;

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            if (input.shoot)
            {
                switch (actualWeapon.type)
                {
                    case ShootType.Raycast:
                        actualWeapon.RpcRaycastShoot();
                        break;

                    case ShootType.RigidBody:
                        actualWeapon.RigidBodyShoot();
                        break;
                }
            }
        }
    }
}
