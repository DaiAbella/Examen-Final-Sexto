using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(GroundCheck), typeof(KCC))]
public class MovementController : NetworkBehaviour
{
    private Rigidbody rbPlayer;
    private KCC kcc;
    [SerializeField] private Animator _animator;

    private void Awake()
    {
        rbPlayer = GetComponent<Rigidbody>();
        kcc = GetComponent<KCC>();
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            // ✅ El host (StateAuthority) aplica el movimiento
            if (GetInput(out NetworkInputData input))
            {
                Movement(input);
                UpdateAnimator(input);
            }
        }
    }

    private void UpdateAnimator(NetworkInputData input)
    {
        _animator.SetBool("IsWalking", input.move != Vector2.zero);
        _animator.SetBool("IsRunning", input.isRunning);
        _animator.SetFloat("WalkingZ", input.move.y);
        _animator.SetFloat("WalkingX", input.move.x);
    }

    #region Movimiento
    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float runSpeed = 7.7f;
    [SerializeField] private float crouchSpeed = 3.9f;

    private void Movement(NetworkInputData input)
    {
        Quaternion realRotation = Quaternion.Euler(0, input.yRotation, 0);
        Vector3 worldDirection = realRotation * new Vector3(input.move.x, 0, input.move.y);

        float speed = Speed(input);
        Vector3 velocity = worldDirection.normalized * speed;

        kcc.SetKinematicVelocity(velocity * Runner.DeltaTime);
    }

    private float Speed(NetworkInputData input)
    {
        return input.move.y < 0 || input.move.x != 0 ? walkSpeed :
            input.isRunning ? runSpeed : walkSpeed;
    }
    #endregion
}
