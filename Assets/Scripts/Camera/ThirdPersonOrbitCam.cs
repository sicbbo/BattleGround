using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonOrbitCam : MonoBehaviour
{
    public Transform playerTrans = null;
    public Vector3 pivotOffset = new Vector3(0.0f, 1.0f, 0.0f);
    public Vector3 camOffset = new Vector3(0.4f, 0.5f, -2.0f);

    public float smooth = 10f;                          // 카메라 반응 속도.
    public float horizontalAimingSpeed = 6.0f;          // 수평 회전 속도.
    public float verticalAimingSpeed = 6.0f;            // 수직 회전 속도.
    public float maxVerticalAngle = 30.0f;              // 카메라의 수직 최대 각도.
    public float minVerticalAngle = -60.0f;             // 카메라의 수직 최소 각도.
    public float recoilAngleBounce = 5.0f;              // 사격 반동 바운스 값.

    private float angleH = 0.0f;                        // 마우스 이동에 따른 카메라 수평이동 수치.
    private float angleV = 0.0f;                        // 마우스 이동에 따른 카메라 수직이동 수치.
    private Transform cameraTrans = null;               // 카메라 트랜스폼 캐싱.
    private Camera myCamera = null;
    private Vector3 relCameraPos = Vector3.zero;        // 플레이어로부터 카메라까지의 벡터.
    private float relCameraPosMag = 0f;                 // 플레이어로부터 카메라사이의 거리.
    private Vector3 smoothPivotOffset = Vector3.zero;   // 카메라 피봇 보간용 벡터.
    private Vector3 smoothCamOffset = Vector3.zero;     // 카메라 위치 보간용 벡터.
    private Vector3 targetPivotOffset = Vector3.zero;   // 카메라 피봇 보간용 벡터.
    private Vector3 targetCamOffset = Vector3.zero;     // 카메라 위치 보간용 벡터.
    private float defaultFOV = 0.0f;                    // 기본 시야 값.
    private float targetFOV = 0.0f;                     // 타겟 시야 값.
    private float targetMaxVerticalAngle = 0.0f;        // 카메라 수직 최대 각도.
    private float recoilAngle = 0.0f;                   // 사격 반동 각도.

    public float AngleH { get { return angleH; } }
    public float AngleV { get { return angleV; } }

    private void Awake()
    {
        cameraTrans = transform;
        myCamera = cameraTrans.GetComponent<Camera>();

        cameraTrans.position = playerTrans.position + Quaternion.identity * pivotOffset + Quaternion.identity * camOffset;
        cameraTrans.rotation = Quaternion.identity;

        relCameraPos = cameraTrans.position - playerTrans.position;
        relCameraPosMag = relCameraPos.magnitude - 0.5f;    // 플레이어의 충돌을 피하기위해 -0.5f

        smoothPivotOffset = pivotOffset;
        smoothCamOffset = camOffset;
        defaultFOV = myCamera.fieldOfView;
        angleH = playerTrans.eulerAngles.y;

        ResetTargetOffsets();
        ResetFOV();
        ResetMaxVerticalAngle();
    }

    public void ResetTargetOffsets()
    {
        targetPivotOffset = pivotOffset;
        targetCamOffset = camOffset;
    }

    public void ResetFOV()
    {
        targetFOV = defaultFOV;
    }

    public void ResetMaxVerticalAngle()
    {
        targetMaxVerticalAngle = maxVerticalAngle;
    }

    public void BounceVertical(float degree)
    {
        recoilAngle = degree;
    }

    public void SetTargetOffset(Vector3 newPivotOffset, Vector3 newCamOffset)
    {
        targetPivotOffset = newPivotOffset;
        targetCamOffset = newCamOffset;
    }

    public void SetFOV(float customFOV)
    {
        targetFOV = customFOV;
    }

    private bool ViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight)
    {
        Vector3 target = playerTrans.position + (Vector3.up * deltaPlayerHeight);
        if (Physics.SphereCast(checkPos, 0.2f, target - checkPos, out RaycastHit hit, relCameraPosMag))
        {
            if (hit.transform != playerTrans && hit.collider.isTrigger == false)
            {
                return false;
            }
        }
        return true;
    }

    private bool ReverseViewingPosCheck(Vector3 checkPos, float deltaPlayerHeight, float maxDistance)
    {
        Vector3 origin = playerTrans.position + (Vector3.up * deltaPlayerHeight);
        if (Physics.SphereCast(origin, 0.2f, checkPos - origin, out RaycastHit hit, maxDistance))
        {
            if (hit.transform != playerTrans && hit.transform != cameraTrans &&  hit.collider.isTrigger == false)
            {
                return false;
            }
        }
        return true;
    }

    private bool DoubleViewingPosCheck(Vector3 checkPos, float offset)
    {
        float playerFocusHeight = playerTrans.GetComponent<CapsuleCollider>().height * 0.75f;
        return ViewingPosCheck(checkPos, playerFocusHeight) && ReverseViewingPosCheck(checkPos, playerFocusHeight, offset);
    }

    private void Update()
    {
        // 마우스 이동 값.
        angleH += Mathf.Clamp(Input.GetAxis("Mouse X"), -1f, 1f) * horizontalAimingSpeed;
        angleV += Mathf.Clamp(Input.GetAxis("Mouse Y"), -1f, 1f) * verticalAimingSpeed;

        // 수직 이동 제한.
        angleV = Mathf.Clamp(angleV, minVerticalAngle, targetMaxVerticalAngle);

        // 수직 카메라 바운스.
        angleV = Mathf.LerpAngle(angleV, angleV + recoilAngle, 10f * Time.deltaTime);

        // 카메라 회전.
        Quaternion camYRotation = Quaternion.Euler(0.0f, angleH, 0.0f);
        Quaternion aimRotation = Quaternion.Euler(-angleV, angleH, 0.0f);
        cameraTrans.rotation = aimRotation;

        // FOV
        myCamera.fieldOfView = Mathf.Lerp(myCamera.fieldOfView, targetFOV, Time.deltaTime);

        Vector3 baseTempPosition = playerTrans.position + camYRotation * targetPivotOffset;
        Vector3 noCollisionOffset = targetCamOffset;    // 조준할때 카메라의 오프셋값, 조준할때와 평소와 다르다.
        for (float zOffset = targetCamOffset.z; zOffset <= 0f; zOffset += 0.5f)
        {
            noCollisionOffset.z = zOffset;
            if (DoubleViewingPosCheck(baseTempPosition + aimRotation * noCollisionOffset, Mathf.Abs(zOffset)) || zOffset == 0f)
            {
                break;
            }
        }

        // 카메라 위치 재설정
        smoothPivotOffset = Vector3.Lerp(smoothPivotOffset, targetPivotOffset, smooth * Time.deltaTime);
        smoothCamOffset = Vector3.Lerp(smoothCamOffset, noCollisionOffset, smooth * Time.deltaTime);

        cameraTrans.position = playerTrans.position + camYRotation * smoothPivotOffset + aimRotation * smoothCamOffset;

        if (recoilAngle > 0.0f)
        {
            recoilAngle -= recoilAngleBounce * Time.deltaTime;
        }
        else
        if (recoilAngle < 0.0f)
        {
            recoilAngle += recoilAngleBounce * Time.deltaTime;
        }
    }

    public float GetCurrentPivotMagnitude(Vector3 finalPivotOffset)
    {
        return Mathf.Abs((finalPivotOffset - smoothPivotOffset).magnitude);
    }
}