using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBehaviour : GenericBehaviour
{
    public float walkSpeed = 0.15f;
    public float runSpeed = 1.0f;
    public float sprintSpeed = 2.0f;
    public float speedDampTime = 0.1f;

    public float jumpHeight = 1.5f;
    public float jumpInertialForce = 10f;
    public float speed, speedSeeker;
    private bool isJump;
    private int jumpBool;
    private int groundedBool;
    private bool isColliding;
    private CapsuleCollider capsuleCollider;
    private Transform playerTrans;

    private void Start()
    {
        playerTrans = transform;
        capsuleCollider = GetComponent<CapsuleCollider>();
        jumpBool = Animator.StringToHash(AnimatorKey.Jump);
        groundedBool = Animator.StringToHash(AnimatorKey.Grounded);
        behaviourController.GetAnimator.SetBool(groundedBool, true);

        behaviourController.SubScribebehaviour(this);
        behaviourController.RegisterDefaultbehaviour(this.BehaviourCode);
        speedSeeker = runSpeed;
    }

    private Vector3 Rotating(float horizontal, float vertical)
    {
        Vector3 forward = behaviourController.playerCamTrans.TransformDirection(Vector3.forward);
        forward.y = 0.0f;
        forward = forward.normalized;

        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);
        Vector3 targetDirection = forward * vertical + right * horizontal;

        if (behaviourController.IsMoving() == true && targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            Quaternion newRotation = Quaternion.Slerp(behaviourController.GetRigidbody.rotation, targetRotation, behaviourController.turnSmoothing);
            behaviourController.GetRigidbody.MoveRotation(newRotation);
            behaviourController.SetLastDirection(targetDirection);
        }
        if ((Mathf.Abs(horizontal) > 0.9f || Mathf.Abs(vertical) > 0.9f) == false)
        {
            behaviourController.Repositioning();
        }

        return targetDirection;
    }

    private void RemoveVerticalVelocity()
    {
        Vector3 verticalVelocity = behaviourController.GetRigidbody.velocity;
        verticalVelocity.y = 0;
        behaviourController.GetRigidbody.velocity = verticalVelocity;
    }

    private void MovementManagement(float horizontal, float vertical)
    {
        if (behaviourController.IsGrounded() == true)
        {
            behaviourController.GetRigidbody.useGravity = true;
        }
        else
        if (behaviourController.GetAnimator.GetBool(jumpBool) == false && behaviourController.GetRigidbody.velocity.y > 0)
        {
            RemoveVerticalVelocity();
        }
        Rotating(horizontal, vertical);
        Vector2 dir = new Vector2(horizontal, vertical);
        speed = Vector2.ClampMagnitude(dir, 1.0f).magnitude;
        speedSeeker += Input.GetAxis("Mouse ScrollWheel");
        speedSeeker = Mathf.Clamp(speedSeeker, walkSpeed, runSpeed);
        speed *= speedSeeker;
        if (behaviourController.IsSprinting() == true)
        {
            speed = sprintSpeed;
        }
        behaviourController.GetAnimator.SetFloat(speedFloat, speed, speedDampTime, Time.deltaTime);
    }

    private void OnCollisionStay(Collision collision)
    {
        isColliding = true;
        if (behaviourController.IsCurrentBehaviour(behaviourCode) && collision.GetContact(0).normal.y <= 0.1f)
        {
            float vel = behaviourController.GetAnimator.velocity.magnitude;
            Vector3 tangentMove = Vector3.ProjectOnPlane(playerTrans.forward, collision.GetContact(0).normal).normalized * vel;
            behaviourController.GetRigidbody.AddForce(tangentMove, ForceMode.VelocityChange);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }

    private void JumpManagement()
    {
        if (isJump && behaviourController.GetAnimator.GetBool(jumpBool) == false && behaviourController.IsGrounded())
        {
            behaviourController.LockTempBehaviour(behaviourCode);
            behaviourController.GetAnimator.SetBool(jumpBool, true);
            if (behaviourController.GetAnimator.GetFloat(speedFloat) > 0.1f)
            {
                capsuleCollider.material.dynamicFriction = 0f;
                capsuleCollider.material.staticFriction = 0f;
                RemoveVerticalVelocity();
                float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                velocity = Mathf.Sqrt(velocity);
                behaviourController.GetRigidbody.AddForce(Vector3.up * velocity, ForceMode.VelocityChange);
            }
        }
        else
        if (behaviourController.GetAnimator.GetBool(jumpBool) == true)
        {
            if (behaviourController.IsGrounded() == false && isColliding == false && behaviourController.GetTempLockStatus())
            {
                behaviourController.GetRigidbody.AddForce(playerTrans.forward * jumpInertialForce * Physics.gravity.magnitude * sprintSpeed, ForceMode.Acceleration);
            }
            if (behaviourController.GetRigidbody.velocity.y < 0f && behaviourController.IsGrounded())
            {
                behaviourController.GetAnimator.SetBool(groundedBool, true);
                capsuleCollider.material.dynamicFriction = 0.6f;
                capsuleCollider.material.staticFriction = 0.6f;
                isJump = false;
                behaviourController.GetAnimator.SetBool(jumpBool, false);
                behaviourController.UnLockTempBehaviour(behaviourCode);
            }
        }
    }

    private void Update()
    {
        if (isJump == false && Input.GetButtonDown(ButtonName.Jump) && behaviourController.IsCurrentBehaviour(behaviourCode) && 
            behaviourController.IsOverriding() == false)
        {
            isJump = true;
        }
    }

    public override void LocalFixedUpdate()
    {
        MovementManagement(behaviourController.GetH, behaviourController.GetV);
        JumpManagement();
    }
}