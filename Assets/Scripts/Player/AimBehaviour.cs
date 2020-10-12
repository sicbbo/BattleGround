using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimBehaviour : GenericBehaviour
{
    public Texture2D crossHair;
    public float aimTurnSmoothing = 0.15f;
    public Vector3 aimPivotOffset = new Vector3(0.5f, 1.2f, 0.0f);
    public Vector3 aimCamOffset = new Vector3(0.0f, 0.4f, -0.7f);

    private int aimBool;
    private bool isAim;
    private int conerBool;
    private bool isPeekCorner;
    private Vector3 initialRootRotation;
    private Vector3 initialHipRotation;
    private Vector3 initialSpineRotation;
    private Transform playerTrans;

    private void Start()
    {
        playerTrans = transform;

        aimBool = Animator.StringToHash(AnimatorKey.Aim);
        conerBool = Animator.StringToHash(AnimatorKey.Corner);

        Transform hips = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        initialRootRotation = (hips.parent == transform) ? Vector3.zero : hips.parent.localEulerAngles;
        initialHipRotation = hips.localEulerAngles;
        initialSpineRotation = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Spine).localEulerAngles;
    }

    private void Rotating()
    {
        Vector3 forward = behaviourController.playerCamTrans.TransformDirection(Vector3.forward);
        forward.y = 0.0f;
        forward = forward.normalized;

        Quaternion targetRotation = Quaternion.Euler(0.0f, behaviourController.GetCamScript.AngleH, 0.0f);
        float minSpeed = Quaternion.Angle(transform.rotation, targetRotation) * aimTurnSmoothing;

        if (isPeekCorner == true)
        {
            playerTrans.rotation = Quaternion.LookRotation(-behaviourController.GetLastDirection());
            targetRotation *= Quaternion.Euler(initialRootRotation);
            targetRotation *= Quaternion.Euler(initialHipRotation);
            targetRotation *= Quaternion.Euler(initialSpineRotation);
            Transform spine = behaviourController.GetAnimator.GetBoneTransform(HumanBodyBones.Spine);
            spine.rotation = targetRotation;
        }
        else
        {
            behaviourController.SetLastDirection(forward);
            playerTrans.rotation = Quaternion.Slerp(playerTrans.rotation, targetRotation, minSpeed * Time.deltaTime);
        }
    }

    private void AimManagement()
    {
        Rotating();
    }

    private IEnumerator ToggleAimOn()
    {
        yield return new WaitForSeconds(0.05f);

        if (behaviourController.GetTempLockStatus(behaviourCode) || behaviourController.IsOverriding(this))
        {
            yield return false;
        }
        else
        {
            isAim = true;
            int signal = 1;
            if (isPeekCorner == true)
            {
                signal = (int)Mathf.Sign(behaviourController.GetH);
            }
            aimCamOffset.x = Mathf.Abs(aimCamOffset.x) * signal;
            aimPivotOffset.x = Mathf.Abs(aimPivotOffset.x) * signal;
            yield return new WaitForSeconds(0.1f);
            behaviourController.GetAnimator.SetFloat(speedFloat, 0.0f);
            behaviourController.OverrideWithBehaviour(this);
        }
    }

    private IEnumerator ToggleAimOff()
    {
        isAim = false;
        yield return new WaitForSeconds(0.3f);
        behaviourController.GetCamScript.ResetTargetOffsets();
        behaviourController.GetCamScript.ResetMaxVerticalAngle();
        yield return new WaitForSeconds(0.1f);
        behaviourController.RevokeOverridingBehaviour(this);
    }

    public override void LocalFixedUpdate()
    {
        if(isAim == true)
        {
            behaviourController.GetCamScript.SetTargetOffset(aimPivotOffset, aimCamOffset);
        }
    }

    public override void LocalLateUpdate()
    {
        AimManagement();
    }

    private void Update()
    {
        isPeekCorner = behaviourController.GetAnimator.GetBool(conerBool);

        if (Input.GetAxisRaw(ButtonName.Aim) != 0 && isAim == false)
        {
            StartCoroutine(ToggleAimOn());
        }
        else
        if (Input.GetAxisRaw(ButtonName.Aim) == 0 && isAim == true)
        {
            StartCoroutine(ToggleAimOff());
        }

        canSprint = !isAim;
        if (isAim == true && Input.GetButtonDown(ButtonName.Shoulder) == true && isPeekCorner == false)
        {
            aimCamOffset.x = -aimCamOffset.x;
            aimPivotOffset.x = -aimPivotOffset.x;
        }

        behaviourController.GetAnimator.SetBool(aimBool, isAim);
    }

    private void OnGUI()
    {
        if (crossHair != null)
        {
            float length = behaviourController.GetCamScript.GetCurrentPivotMagnitude(aimPivotOffset);
            if (length < 0.05f)
            {
                GUI.DrawTexture(new Rect(Screen.width * 0.5f - (crossHair.width * 0.5f), Screen.height * 0.5f - (crossHair.height * 0.5f),
                    crossHair.width, crossHair.height), crossHair);
            }
        }
    }
}