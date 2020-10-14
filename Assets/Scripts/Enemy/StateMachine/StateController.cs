using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StateController : MonoBehaviour
{
    public GeneralStats generalStats;
    public ClassStats statData;
    public string classID;

    private ClassStats.Param classStats = null;
    public ClassStats.Param ClassStats
    {
        get
        {
            if (classStats == null)
            {
                for (int i = 0; i < statData.sheets.Count; i++)
                {
                    ClassStats.Sheet sheet = statData.sheets[i];
                    for (int j = 0; j < sheet.list.Count; j++)
                    {
                        ClassStats.Param param = sheet.list[j];
                        if (param.ID.Equals(classID))
                        {
                            return param;
                        }
                    }
                }
                return null;
            }
            else
                return classStats;
        }
    }

    public State currentState;
    public State remainState;

    public Transform aimTarget;

    public List<Transform> patrolWaypoints;

    public int bullets;
    [Range(0f, 50f)]
    public float viewRadius;
    [Range(0f, 360f)]
    public float viewAngle;
    [Range(0f, 25f)]
    public float perceptionRadius;

    [HideInInspector] public float nearRadius;
    [HideInInspector] public NavMeshAgent nav;
    [HideInInspector] public int wayPointIndex;
    [HideInInspector] public int maximumBurst = 7;
    [HideInInspector] public float blindEngageTime = 30f;
    [HideInInspector] public bool targetInSight;
    [HideInInspector] public bool focusSight;
    [HideInInspector] public bool reloading;
    [HideInInspector] public bool hadClearShot;
    [HideInInspector] public bool haveClearShot;
    [HideInInspector] public int coverHash = -1;
    [HideInInspector] public EnemyVariables variables;
    [HideInInspector] public Vector3 personalTarget = Vector3.zero;
    [HideInInspector] public EnemyAnimation enemyAnimation;
    [HideInInspector] public CoverLookUp coverLookUp;

    private int magBullets;
    private bool aiActive;
    private bool checkedOnLoop, blockedSight;
    private static Dictionary<int, Vector3> coverSpot;
    public Vector3 CoverSpot { get { return coverSpot[GetHashCode()]; } set { coverSpot[GetHashCode()] = value; } }
    private bool strafing;
    public bool Strafing
    {
        get { return strafing; }
        set
        {
            enemyAnimation.anim.SetBool("Strafe", value);
            strafing = value;
        }
    }
    private bool aiming;
    public bool Aiming
    {
        get { return aiming; }
        set
        {
            if (aiming != value)
            {
                enemyAnimation.anim.SetBool("Aim", value);
                aiming = value;
            }
        }
    }

    public void TransitionToState(State nextState, Decision decision)
    {
        if (nextState != remainState)
        {
            currentState = nextState;
        }
    }

    public IEnumerator UnstuckAim(float delay)
    {
        yield return new WaitForSeconds(delay * 0.5f);
        aiming = false;
        yield return new WaitForSeconds(delay * 0.5f);
        aiming = true;
    }

    private void Awake()
    {
        if (coverSpot == null)
        {
            coverSpot = new Dictionary<int, Vector3>();
        }
        coverSpot[GetHashCode()] = Vector3.positiveInfinity;

        nav = GetComponent<NavMeshAgent>();
        aiActive = true;
        enemyAnimation = gameObject.AddComponent<EnemyAnimation>();
        magBullets = bullets;
        variables.shotsInRounds = maximumBurst;

        nearRadius = perceptionRadius * 0.5f;

        GameObject gameController = GameObject.FindGameObjectWithTag("GameController");
        coverLookUp = gameController.GetComponent<CoverLookUp>();
        if (coverLookUp == null)
        {
            coverLookUp = gameController.AddComponent<CoverLookUp>();
            coverLookUp.Setup(generalStats.coverMask);
        }

        Debug.Assert(aimTarget.root.GetComponent<HealthBase>(), "반드시 타겟에는 생명력관련 컴포넌트를 붙여주어야 합니다.");

    }

    private void Start()
    {
        currentState.OnEnableActions(this);
    }

    private void Update()
    {
        checkedOnLoop = false;
        if (!aiActive)
            return;

        currentState.DoActions(this);
        currentState.CheckTransitions(this);
    }

    private void OnDrawGizmos()
    {
        if (currentState != null)
        {
            Gizmos.color = currentState.sceneGizmoColor;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2.5f, 2f);
        }
    }

    public void EndReloadWeapon()
    {
        reloading = false;
        bullets = magBullets;
    }

    public void AlertCallback(Vector3 target)
    {
        if (!aimTarget.root.GetComponent<HealthBase>().isDead)
        {
            variables.hearAlert = true;
            personalTarget = target;
        }
    }

    public bool IsNearOtherSpot(Vector3 spot, float margin = 1f)
    {
        foreach(KeyValuePair<int, Vector3> usedSpot in coverSpot)
        {
            if (usedSpot.Key != gameObject.GetHashCode() && Vector3.Distance(spot, usedSpot.Value) <= margin)
            {
                return true;
            }
        }
        return false;
    }

    public bool BlockedSight()
    {
        if (!checkedOnLoop)
        {
            checkedOnLoop = true;
            Vector3 target = default;
            try
            {
                target = aimTarget.position;
            }catch(UnassignedReferenceException)
            {
                Debug.LogError(string.Format("조준 타겟을 지정해주세요 : {0}", transform.name));
            }

            Vector3 castOrigin = transform.position + Vector3.up * generalStats.aboveCoverHeight;
            Vector3 dirToTarget = target - castOrigin;

            blockedSight = Physics.Raycast(castOrigin, dirToTarget, out RaycastHit hit, dirToTarget.magnitude, generalStats.coverMask | generalStats.obstacleMask);

        }

        return blockedSight;
    }

    private void OnDestroy()
    {
        coverSpot.Remove(GetHashCode());
    }
}