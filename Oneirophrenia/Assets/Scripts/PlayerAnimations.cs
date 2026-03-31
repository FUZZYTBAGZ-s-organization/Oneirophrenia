using UnityEditor.Build;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{

    //checks for the ground
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    Vector3 velocity;
    public bool isGrounded;




    Animator PlayerAnim;
    int IsWalkingHash;
    int IsRunningHash;
    int IsJumpingHash;
   

    void Start()
    {
        PlayerAnim = GetComponent<Animator>();
        IsWalkingHash = Animator.StringToHash("IsWalking");
        IsRunningHash = Animator.StringToHash("IsRunning");
        IsJumpingHash = Animator.StringToHash("Jump");
      




    }

    void Update()
    {

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);


        bool IsJumping = PlayerAnim.GetBool(IsJumpingHash);
        bool IsRunning = PlayerAnim.GetBool(IsRunningHash);
        bool IsWalking = PlayerAnim.GetBool(IsWalkingHash);
        bool forwardPressed = Input.GetKey("w");
        bool runPressed = Input.GetKey("left shift");
        bool JumpPressed = Input.GetKey("space");
        

        if (!IsWalking && forwardPressed)
        {
            PlayerAnim.SetBool(IsWalkingHash, true);
        }

        if (IsWalking && !forwardPressed)
        {
            PlayerAnim.SetBool(IsWalkingHash, false);
        }

        if (!IsRunning && (forwardPressed && runPressed))
        {
            PlayerAnim.SetBool(IsRunningHash, true);
        }

        if (IsRunning && (!forwardPressed || !runPressed))
        {
            PlayerAnim.SetBool(IsRunningHash, false);
        }

        if (!runPressed)
        {
            PlayerAnim.SetBool(IsRunningHash, false);
        }

        if (JumpPressed && isGrounded)
        {
            PlayerAnim.SetBool(IsJumpingHash, true);
        }

        if (isGrounded && !JumpPressed)
        {
            PlayerAnim.SetBool(IsJumpingHash, false);
        }

    }
}

