using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    public bool doubleJump = true;

    [Header("Base Combat Info")]
    public GameObject weaponHand;
    public GameObject weaponBack;
    public bool armed;

    [Header("RaycastInfo")]
    public LayerMask everything;
    public LayerMask climbing;
    public LayerMask water;
    public RaycastHit lastCachedhit;
    public string lastCachedtag;
    public float distanceFromWall;
    public float grabHeight;
    [Space]
    [SerializeField] bool testRaycast;
    [SerializeField] private RayCastTesting casts;

    [Header("GroundedInfo")]
    public bool grounded;
    public bool ccGrounded;
    public bool ignoreAirtime;

    [Header("IK")]
    [SerializeField] Transform headIK;
    [SerializeField] UseIK ikInUse = UseIK.none;
    private Vector3 leftHandPos;
    private Vector3 rightHandPos;

    public enum UseIK { none, left, right, both };

    [Header("Climbing")]
    public Vector3 heightOffset;
    public float forwardOffset;

    [Header("References")]
    public OrbitCamera oc;

    #region public hidden
    [HideInInspector] public CharacterController cc;
    [HideInInspector] public LockOnLookat lol;
    [HideInInspector] public TargetingSystem ts;
    [HideInInspector] public Animator anim;
    [HideInInspector] public bool lockedOn;
    [HideInInspector] public PlayerControls pc;
    [HideInInspector] public StateMachine stateMachine;
    [HideInInspector] public bool lockRotation;
    #endregion

    void Start()
    {
        cc = GetComponent<CharacterController>();
        ts = GetComponent<TargetingSystem>();
        anim = GetComponent<Animator>();
        pc = GetComponent<PlayerControls>();

        lol = GetComponentInChildren<LockOnLookat>();

        lol.gameObject.SetActive(false);

        SetupStateMachine();
    }

    void SetupStateMachine()
    {
        Locomotion lm = new Locomotion(this);
        InAir ia = new InAir(this);
        Climbing cl = new Climbing(this);
        Sliding sl = new Sliding(this);
        Swimming sw = new Swimming(this);
        Underwater uw = new Underwater(this);

        stateMachine = new StateMachine(lm, ia, cl, sl, sw, uw);
        stateMachine.SwitchState(typeof(Locomotion));
    }

    void Update()
    {
        if (testRaycast)
        {
            casts.RunTests(transform);
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            CameraShake();
        }
        ccGrounded = cc.isGrounded;
        stateMachine.StateUpdate();
        Targeting();
        anim.SetBool("Land", grounded);
    }

    private void FixedUpdate()
    {
        stateMachine.OnFixedUpdate();
    }

    void Targeting()
    {
        if (lockedOn)
        {
            RotateTowardsCamera();
            if (Input.mouseScrollDelta.y != 0)
            {
                ts.SwitchTarget(oc);
                lol.target = ts.currentTarget;
            }
            if (Vector3.Distance(transform.position, ts.currentTarget.transform.position) > ts.loseTargetRange)
            {
                LoseTarget();
            }
        }
    }
    
    public void CanTarget()
    {
        if (Input.GetKeyDown(pc.target))
        {
            switch (oc.currentState)
            {
                case OrbitCamera.CamState.onPlayer:
                    if (ts.SelectTarget(oc))
                    {
                        anim.SetBool("Target", true);
                        lockedOn = true;
                        lol.gameObject.SetActive(true);
                        lol.target = ts.currentTarget;
                    }
                    break;
                case OrbitCamera.CamState.onTarget:
                    LoseTarget();
                    break;
                default:
                    break;
            }
        }
    }

    void LoseTarget()
    {
        lockedOn = false;
        anim.SetBool("Target", false);
        oc.ChangeCamState(OrbitCamera.CamState.onPlayer);
        ts.currentTarget = null;
        lol.gameObject.SetActive(false);
    }

    public void RotateTowardsCamera()
    {
        Quaternion newLookAt = Quaternion.LookRotation(ts.currentTarget.transform.position - transform.position);
        newLookAt.x = 0;
        newLookAt.z = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, newLookAt, Time.deltaTime * 5);
    }

    [Header("Ground Raycast")]
    public float startHeight = 0.5f;
    public float range = 1;
    public float airtime;

    public bool Grounded()
    {
        if (ignoreAirtime)
        {
            return true;
        }
        Vector3[] offsets = new Vector3[] {transform.right * 0.2f, -transform.right * 0.2f , transform.forward * 0.2f , -transform.forward * 0.2f , Vector3.zero };
        for (int i = 0; i < offsets.Length; i++)
        {
            if (RayHit(transform.position + offsets[i] + (transform.up * startHeight), (Vector3.down), range, everything, false, Color.magenta))
            {
                grounded = true;
                if (airtime != 0)
                {
                    //Debug.Log("Total airtime was " + airtime.ToString("F2"));
                }
                airtime = 0;
                GroundTagManager();
                return true;
            }
        }
        airtime += Time.deltaTime;
        if (airtime < 1.25f)
        {
            anim.SetFloat("LandingState", anim.GetFloat("y+x"));
        }
        else
        {
            anim.SetFloat("LandingState", 2);
        }
        grounded = false;
        return false;
    }

    void GroundTagManager()
    {
        float target = 0;
        switch (lastCachedtag)
        {
            case "Beam":
                target = 1;
                break;
            case "":
                break;
            default:
                target = 0;
                break;
        }

        float currentTarget = anim.GetFloat("AnimationState");
        if (anim.GetFloat("AnimationState") > target)
        {
            currentTarget -= Time.deltaTime * 2;
        }
        else if (anim.GetFloat("AnimationState") < target)
        {
            currentTarget += Time.deltaTime * 2;
        }

        if (currentTarget < 0)
            currentTarget = 0;

        anim.SetFloat("AnimationState", currentTarget);
    }

    public bool RayHit(Vector3 start, Vector3 dir, float length, LayerMask lm, bool drawRay = false, Color rayColor = default)
    {
        RaycastHit hit;
        Ray ray = new Ray(start, dir);
        if (drawRay)
        {
            Debug.DrawRay(start, dir * length, rayColor, 5f);
        }
        if (Physics.Raycast(ray, out hit, length, lm))
        {
            lastCachedhit = hit;
            lastCachedtag = hit.collider.tag;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void LerpToPosition(bool enableRoot, bool ignoreAir, params LerpInfo[] pos)
    {
        ignoreAirtime = ignoreAir;
        Vector3 previousLerp = transform.position;
        foreach (var lerpLine in pos)
        {
            Debug.DrawLine(previousLerp, lerpLine.target, Color.yellow, 5);
            previousLerp = lerpLine.target;
        }
        StartCoroutine(LerpToPos(pos, enableRoot, ignoreAir));
    }

    public IEnumerator LerpToPos(LerpInfo[] positions, bool enableRoot, bool ignoreAir)
    {
        ignoreAirtime = ignoreAir;
        anim.applyRootMotion = false;
        lockRotation = true;

        foreach (var destination in positions)
        {
            ikInUse = destination.ikUse;
            leftHandPos = destination.leftIK;
            rightHandPos = destination.rightIK;

            Vector3 startPos = transform.position;
            float lerpTime = destination.timeToLerp;

            for (float t = 0; t < 1; t += Time.deltaTime / lerpTime)
            {
                transform.position = Vector3.Lerp(startPos, destination.target, t);
                yield return new WaitForEndOfFrame();
            }
            transform.position = destination.target;
        }

        if (ignoreAirtime)
        {
            airtime = 0;
            ignoreAirtime = false;
        }

        yield return new WaitForEndOfFrame();

        ikInUse = UseIK.none;
        anim.applyRootMotion = enableRoot;
        lockRotation = false;
    }

    public bool PlayerCanStand(Vector3 posToStand)
    {
        RaycastHit rayhit;
        Ray ray = new Ray(posToStand, Vector3.up);

        if (Physics.Raycast(ray, out rayhit, 2f,everything))
        {
            Debug.DrawRay(ray.origin, ray.direction * 2, Color.red, 5);
            return false;
        }
        return true;
    }

    public bool PlayerToWall(PlayerBehaviour pb, Vector3 dir, bool lerp, float checkYOffset)
    {
        RaycastHit hit;
        float range = 2;
        Vector3 playerHeight = new Vector3(pb.transform.position.x, pb.transform.position.y + checkYOffset, pb.transform.position.z);
        Debug.DrawRay(playerHeight, dir * range, Color.red, 5);
        if (Physics.Raycast(playerHeight, dir, out hit, range))
        {
            Vector3 temp = pb.transform.position - hit.point;
            temp.y = 0;
            Vector3 positionToSend = pb.transform.position - temp;
            positionToSend -= (pb.transform.forward * distanceFromWall);
            if (lerp)
            {
                pb.LerpToPosition(false, true, new LerpInfo(0.25f, positionToSend));
            }
            else
            {
                transform.position = positionToSend;
            }
            return true;
        }
        return false;
    }

    public bool PlayerFaceWall(PlayerBehaviour pb, Vector3 startOffset, Vector3 dir, float range)
    {
        RaycastHit hit;
        Vector3 playerHeight = pb.transform.position;
        playerHeight += startOffset;
        Debug.DrawRay(playerHeight, dir * range, Color.blue, 5);
        if (Physics.Raycast(playerHeight, dir, out hit, range))
        {
            pb.transform.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
            return true;
        }
        return false;
    }

    public bool LedgeInfo()
    {
        if (!RayHit(transform.position + (transform.up * 1.7f), transform.forward, 0.45f, climbing))
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position + (transform.forward * 0.45f) + (transform.up * 1.7f), Vector3.down);
            Debug.DrawRay(transform.position + (transform.forward * 0.45f) + (transform.up * 1.7f), Vector3.down * 0.35f, Color.black, 3f);
            if (Physics.Raycast(ray, out hit, 0.35f, climbing))
            {
                lastCachedhit = hit;
                string tagObject = hit.collider.tag;
                switch (tagObject)
                {
                    case "Ledge":
                        if (stateMachine.IsInState(typeof(InAir)))
                        {
                            anim.Play("AirToClimb");
                        }
                        stateMachine.SwitchState(typeof(Climbing));
                        break;
                    case "Example":
                        break;
                    default:
                        break;
                }
                return true;
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator LerpCharacterControllerCenter(float to, float lerpTime)
    {
        float timeElapsed = 0;
        float beginFloat = cc.center.y;
        while (timeElapsed < lerpTime)
        {
            beginFloat = Mathf.Lerp(beginFloat, to, timeElapsed / 0.25f);
            Vector3 temp = new Vector3(0, beginFloat, 0);
            cc.center = temp;
            timeElapsed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Vector3 finalY = new Vector3(0, to, 0);
        cc.center = finalY;
    }

    // Called in climb animation
    public void GoToLocomotion()
    {
        stateMachine.SwitchState(typeof(Locomotion));
    }

    public void _DoubleJump(float targetFloat)
    {
        StartCoroutine(LerpCharacterControllerCenter(targetFloat, 0.2f));
    }

    public void AddRotation(Vector3 rot)
    {
        transform.Rotate(rot);
    }

    public void CameraShake()
    {
        EZCameraShake.CameraShaker.Instance.ShakeOnce(5, 5, 0.5f, 1f);
    }

    float currentLeftWeight;
    float currentRightWeight;

    private void OnAnimatorIK(int layerIndex)
    {
        anim.SetLookAtWeight(0.3f);
        anim.SetLookAtPosition(headIK.position);

        switch (ikInUse)
        {
            case UseIK.none:
                currentLeftWeight = 0;
                currentRightWeight = 0;

                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                break;
            case UseIK.left:
                if (currentLeftWeight < 1)
                    currentLeftWeight += Time.deltaTime;

                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, currentLeftWeight);

                anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPos);
                break;
            case UseIK.right:
                if (currentRightWeight < 1)
                    currentRightWeight += Time.deltaTime;

                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.6f);

                anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandPos);
                break;
            case UseIK.both:

                if (currentRightWeight < 1)
                    currentRightWeight += Time.deltaTime;
                if (currentLeftWeight < 1)
                    currentLeftWeight += Time.deltaTime;

                anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0.6f);
                anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.6f);

                anim.SetIKPosition(AvatarIKGoal.LeftHand, leftHandPos);
                anim.SetIKPosition(AvatarIKGoal.RightHand, rightHandPos);
                break;
            default:
                break;
        }
    }

    public bool Underwater(float rayLength, bool showRay = false)
    {
        return RayHit(transform.position + (Vector3.up * 2), Vector3.down, rayLength, water, showRay, Color.black);
    }

    float turnSmoothVelocity;
    public void RotateToCam(bool rawInput = true, float rotationMulitplyer = 1)
    {
        float x = 0;
        float y = 0;
        if (rawInput)
        {
            x = Input.GetAxisRaw("Horizontal");
            y = Input.GetAxisRaw("Vertical");
        }
        else
        {
            x = Input.GetAxis("Horizontal") * rotationMulitplyer;
            y = Input.GetAxis("Vertical") * rotationMulitplyer;
        }
        Vector3 dir = new Vector3(x, 0f, y).normalized;

        if (dir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + oc.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, 0.1f);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(lastCachedhit.point, 0.05f);
    }
}

public struct LerpInfo
{
    public float timeToLerp;
    public Vector3 target;

    public PlayerBehaviour.UseIK ikUse;

    public Vector3 leftIK;
    public Vector3 rightIK;
    
    public LerpInfo(float lerpTime, Vector3 targetPos, PlayerBehaviour.UseIK ikType = PlayerBehaviour.UseIK.none, Vector3 ikLeft = default, Vector3 ikRight = default)
    {
        timeToLerp = lerpTime;
        target = targetPos;

        ikUse = ikType;

        leftIK = ikLeft;
        rightIK = ikRight;
    }
}

[System.Serializable]
class RayCastTesting
{
    [SerializeField] List<RayToTest> rays = new List<RayToTest>();

    public void RunTests(Transform player)
    {
        foreach (RayToTest ray in rays)
        {
            Debug.DrawRay(ray.GetStartPosition(player), ray.GetDirection(player) * ray.GetRayLength(), ray.GetRayColor());
        }
    }
}

[System.Serializable]
class RayToTest
{
    [SerializeField] Vector3 startOffset;
    [SerializeField] float rayLength;
    [SerializeField] Color rayColor = Color.white;
    [SerializeField] Direction direction;
    enum Direction {left, right, up, down, back, forward }

    public Vector3 GetStartPosition(Transform player) { return player.transform.position + startOffset; }

    public float GetRayLength() { return rayLength; }

    public Color GetRayColor() { return rayColor; }

    public Vector3 GetDirection(Transform player)
    {
        Vector3 dir = Vector3.zero;
        switch (direction)
        {
            case Direction.left:
                dir = -player.transform.right;
                break;
            case Direction.right:
                dir = player.transform.right;
                break;
            case Direction.up:
                dir = player.transform.up;
                break;
            case Direction.down:
                dir = -player.transform.up;
                break;
            case Direction.back:
                dir = -player.transform.forward;
                break;
            case Direction.forward:
                dir = player.transform.forward;
                break;
            default:
                break;
        }
        return dir;
    }
}