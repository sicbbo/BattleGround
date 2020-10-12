using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourController : MonoBehaviour
{
    private List<GenericBehaviour> behaviours = null;           // 동작들.
    private List<GenericBehaviour> overrideBehaviours = null;   // 우선시 되는 동작들.
    private int currentBehaviour = 0;                           // 현재 동작 해시코드.
    private int defaultBehaviour = 0;                           // 기본 동작 해시코드.
    private int behaviourLocked = 0;                            // 잠긴 동작 해시코드.

    // Caching
    public Transform playerCamTrans = null;                     // 플레이어의 카메라 트랜스폼.
    private Transform playerTrans = null;                       // 플레이어의 트랜스폼.
    private Animator playerAnimator = null;                     // 플레이어의 애니메이터.
    private Rigidbody playerRigidbody = null;                   // 플레이어의 리지드바디.
    private ThirdPersonOrbitCam camScript = null;               // 카메라의 스크립트.

    // Property
    public float turnSmoothing = 0.06f;                        // 카메라를 향하도록 움직일때 회전 속도.
    [SerializeField]
    private float sprintFOV = 100f;                             // 달리기 시야각.
    private float h = 0.0f;                                     // horizontal axis.
    private float v = 0.0f;                                     // vertical axis.
    private bool isChangedFOV;                                  // 카메라 시야각이 변경되었을때 달리기 동작이 저장되었는가.
    private Vector3 lastDirection;                              // 마지막으로 향했던 방향.
    private bool isSprint;                                      // 현재 달리는 중인가.
    private int hFloat;                                         // 애니메이터 관련 가로축 값.
    private int vFloat;                                         // 애니메이터 관련 세로축 값.
    private int groundedBool;                                   // 애니메이터 지상 확인 값.
    private Vector3 colExtents;                                 // 땅과의 충돌 체크를 위한 충돌체 영역.

    public float GetH { get { return h; } }
    public float GetV { get { return v; } }
    public ThirdPersonOrbitCam GetCamScript { get { return camScript; } }
    public Rigidbody GetRigidbody { get { return playerRigidbody; } }
    public Animator GetAnimator { get { return playerAnimator; } }
    public int GetDefaultbehaviour { get { return defaultBehaviour; } }

    private void Awake()
    {
        behaviours = new List<GenericBehaviour>();
        overrideBehaviours = new List<GenericBehaviour>();
        playerTrans = transform;
        playerAnimator = GetComponent<Animator>();
        hFloat = Animator.StringToHash(AnimatorKey.Horizontal);
        vFloat = Animator.StringToHash(AnimatorKey.Vertical);
        camScript = playerCamTrans.GetComponent<ThirdPersonOrbitCam>();
        playerRigidbody = GetComponent<Rigidbody>();

        groundedBool = Animator.StringToHash(AnimatorKey.Grounded);
        colExtents = GetComponent<Collider>().bounds.extents;
    }

    public bool IsMoving()
    {
        return Mathf.Abs(h) > Mathf.Epsilon || Mathf.Abs(v) > Mathf.Epsilon;
    }

    public bool IsHorizontalMoving()
    {
        return Mathf.Abs(h) > Mathf.Epsilon;
    }

    public bool CanSprint()
    {
        for (int i = 0; i < behaviours.Count; i++)
        {
            if (behaviours[i].AllowSprint == false)
                return false;
        }
        for (int i = 0; i < overrideBehaviours.Count; i++)
        {
            if (overrideBehaviours[i].AllowSprint == false)
                return false;
        }
        return true;
    }

    public bool IsSprinting()
    {
        return isSprint && IsMoving() && CanSprint();
    }

    public bool IsGrounded()
    {
        Ray ray = new Ray(playerTrans.position + Vector3.up * 2f * colExtents.x, Vector3.down);
        return Physics.SphereCast(ray, colExtents.x, colExtents.x + 0.2f);
    }

    private void Update()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        playerAnimator.SetFloat(hFloat, h, 0.1f, Time.deltaTime);
        playerAnimator.SetFloat(vFloat, v, 0.1f, Time.deltaTime);

        isSprint = Input.GetButton(ButtonName.Sprint);
        if (IsSprinting())
        {
            isChangedFOV = true;
            camScript.SetFOV(sprintFOV);
        }
        else
        if (isChangedFOV == true)
        {
            camScript.ResetFOV();
            isChangedFOV = false;
        }

        playerAnimator.SetBool(groundedBool, IsGrounded());
    }

    public void Repositioning()
    {
        if (lastDirection != Vector3.zero)
        {
            lastDirection.y = 0f;
            Quaternion targetRotation = Quaternion.LookRotation(lastDirection);
            Quaternion newRotation = Quaternion.Slerp(playerRigidbody.rotation, targetRotation, turnSmoothing);
            playerRigidbody.MoveRotation(newRotation);
        }
    }

    private void FixedUpdate()
    {
        bool isAnybehaviourActive = false;
        if (behaviourLocked > 0 || overrideBehaviours.Count == 0)
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours[i].isActiveAndEnabled && currentBehaviour == behaviours[i].BehaviourCode)
                {
                    isAnybehaviourActive = true;
                    behaviours[i].LocalFixedUpdate();
                }
            }
        }
        else
        {
            for (int i = 0; i < overrideBehaviours.Count; i++)
            {
                overrideBehaviours[i].LocalFixedUpdate();
            }
        }

        if (isAnybehaviourActive == false && overrideBehaviours.Count == 0)
        {
            playerRigidbody.useGravity = true;
            Repositioning();
        }
    }

    private void LateUpdate()
    {
        if (behaviourLocked > 0 || overrideBehaviours.Count == 0)
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                if (behaviours[i].isActiveAndEnabled && currentBehaviour == behaviours[i].BehaviourCode)
                {
                    behaviours[i].LocalLateUpdate();
                }
            }
        }
        else
        {
            for (int i = 0; i < overrideBehaviours.Count; i++)
            {
                overrideBehaviours[i].LocalLateUpdate();
            }
        }
    }

    public void SubScribebehaviour(GenericBehaviour behaviour)
    {
        behaviours.Add(behaviour);
    }

    public void RegisterDefaultbehaviour(int behaviourCode)
    {
        defaultBehaviour = behaviourCode;
        currentBehaviour = behaviourCode;
    }

    public void Registerbehaviour(int behaviourCode)
    {
        if (currentBehaviour == defaultBehaviour)
        {
            currentBehaviour = behaviourCode;
        }
    }

    public void UnRegisterbehaviour(int behaviourCode)
    {
        if (currentBehaviour == behaviourCode)
        {
            currentBehaviour = defaultBehaviour;
        }
    }

    public bool IsCurrentBehaviour(int behaviourCode)
    {
        return currentBehaviour == behaviourCode;
    }

    public bool OverrideWithBehaviour(GenericBehaviour behaviour)
    {
        if (overrideBehaviours.Contains(behaviour) == false)
        {
            if (overrideBehaviours.Count == 0)
            {
                for (int i = 0; i < behaviours.Count; i++)
                {
                    if (behaviours[i].isActiveAndEnabled && currentBehaviour == behaviour.BehaviourCode)
                    {
                        behaviour.OnOverride();
                        break;
                    }
                }
            }
            overrideBehaviours.Add(behaviour);
            return true;
        }
        return false;
    }

    public bool RevokeOverridingBehaviour(GenericBehaviour behaviour)
    {
        if (overrideBehaviours.Contains(behaviour) == true)
        {
            overrideBehaviours.Remove(behaviour);
            return true;
        }
        return false;
    }

    public bool IsOverriding(GenericBehaviour behaviour = null)
    {
        if (behaviour == null)
        {
            return overrideBehaviours.Count > 0;
        }
        return overrideBehaviours.Contains(behaviour);
    }

    public bool GetTempLockStatus(int behaviourCode = 0)
    {
        return (behaviourLocked != 0 && behaviourLocked != behaviourCode);
    }

    public void LockTempBehaviour(int behaviourCode)
    {
        if (behaviourLocked == 0)
        {
            behaviourLocked = behaviourCode;
        }
    }

    public void UnLockTempBehaviour(int behaviourCode)
    {
        if (behaviourLocked == behaviourCode)
        {
            behaviourLocked = 0;
        }
    }

    public Vector3 GetLastDirection()
    {
        return lastDirection;
    }

    public void SetLastDirection(Vector3 direction)
    {
        lastDirection = direction;
    }
}

public abstract class GenericBehaviour : MonoBehaviour
{
    protected int speedFloat;
    protected BehaviourController behaviourController;
    protected int behaviourCode;
    protected bool canSprint;

    public int BehaviourCode { get { return behaviourCode; } }
    public bool AllowSprint { get { return canSprint; } }

    private void Awake()
    {
        behaviourController = GetComponent<BehaviourController>();
        speedFloat = Animator.StringToHash(AnimatorKey.Speed);
        canSprint = true;

        // 동작 타입을 해시코드로 가지고 있다가 추후에 구별용으로 사용.
        behaviourCode = GetType().GetHashCode();
    }

    public virtual void LocalLateUpdate()
    {

    }
    public virtual void LocalFixedUpdate()
    {

    }
    public virtual void OnOverride()
    {

    }
}