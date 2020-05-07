using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBaseComponent : MonoBehaviour
{
    protected int myIndexNumInGMActorList;
    public int GetIndexNumInGMActorList { get { return myIndexNumInGMActorList; } }

    [SerializeField, Range(-89f, 89f)]
    protected float minVerticalAngle = -30f, maxVerticalAngle = 60f;
    public float MinVerticalAngle { get { return minVerticalAngle; } }
    public float MaxVerticalAngle { get { return maxVerticalAngle; } }
    [SerializeField]
    protected bool isUpperRotationSeperated;
    [SerializeField]
    protected float rotationSpeed = 90f;
    [SerializeField]
    protected Transform playerInputSpace = default;
    [SerializeField]
    protected Collider movementCollider;
    [SerializeField, Range(0f, 100f)]
    protected float maxSpeed = 10f;
    [SerializeField, Range(0f, 100f)]
    protected float maxAcceleration = 10f, maxAirAcceleration = 1f;
    [SerializeField]
    protected int bunnyHopAllowFrame = 0;
    [SerializeField, Range(0f, 10f)]
    protected float jumpHeight = 2f;
    [SerializeField, Range(0, 5)]
    protected int maxAirJumps = 0;
    [SerializeField, Range(0, 90)]
    protected float maxGroundAngle = 25f, maxStairsAngle = 50f;
    [SerializeField, Range(0f, 100f)]
    protected float maxSnapSpeed = 100f;
    [SerializeField, Min(0f)]
    protected float probeDistance = 1.8f;//1f;
    public float disableProbeTime;
    [SerializeField]
    protected LayerMask probeMask = -1, stairsMask = -1;
    [SerializeField, Range(0, 1)]
    protected float jumpVelocityMultiplier = 0.3f;
    [SerializeField]
    protected float SteepDuration = 3f;
    [SerializeField, Range(0.01f, 1)]
    protected float rotationSpeedMultiplyWhenZoom = 1;

    public struct RigidbodyVelocity
    {
        public Vector3 position;
        public Vector3 velocity;
    }
    protected List<RigidbodyVelocity> velocityHistory;
    public List<RigidbodyVelocity> PositionHistory { get { return velocityHistory; } }
    [SerializeField]
    int positionhistoryMaxStorage = 60;

    [SerializeField]
    Collider[] myColliders;
    //List<Collider> thisColliderList;

    public enum ShoulderStatus { Left = 0, Center, Right }
    [SerializeField]
    protected ShoulderStatus shoulderStatus = ShoulderStatus.Right;
    public ShoulderStatus CurrentShoulderStatus { get { return shoulderStatus; } }
    [SerializeField]
    protected float shoulderChangeSpeed = 1f;
    [SerializeField]
    protected float wallCheckOffset = 1;
    public enum ShoulderSwapType { None = 0, AlwaysCheckDistance, CheckWhenAttachWall, AttachWallCheckAndUpdateUntilDetach }
    [SerializeField]
    protected ShoulderSwapType shoulderSwapType = ShoulderSwapType.None;

    [SerializeField]
    protected Transform gunpoint;
    [SerializeField] protected Transform gunModelPoint;
    [SerializeField] bool isWeaponModelVisible = true;
    [SerializeField] protected RangeWeapon[] rangeWeapon;
    public RangeWeapon CarriedRangeWeapon(int i) { { return rangeWeapon[i]; } }
    protected int currentRangeWeaponNum=0;
    protected int prevRangeWeaponNum = 1;
    protected GameObject[] weaponModel;
    public int CurrentRangeWeaponNum { get { return currentRangeWeaponNum; } }
    protected float chamberLoadTime = 0;
    protected int firedBullet = 0;
    protected int burstFired = 0;
    [SerializeField] protected Bullet[] bullets;
    [SerializeField] protected MeshCollider[] directionalRangeAttackArea;
    protected Collider[] neighbours;
    [SerializeField] int maxNeighbours = 16;

    [SerializeField]
    protected float gunShotSoundRange = 30f;
    [SerializeField]
    protected float impactPointSoundRange = 8f;

    [SerializeField]
    protected Throwable[] throwableEquipment;
    protected int throwNum;
    protected private Throwable throwingObject;
    [SerializeField]
    protected Throwable.AddVelocityType addBodyVelocityOnThrowable;
    [SerializeField]
    protected Transform throwPoint;
    public Vector3 ThrowPoint { get { return throwPoint.position; } }
    [SerializeField]
    protected float throwPower = 10f;
    [SerializeField]
    protected float throwUpPower = 2f;

    [SerializeField]
    protected float recoveryDelay = 3f;
    protected float lastDamageTakenTime;
    [SerializeField]
    protected float recoverHealthPerSec = 0.33f;



    [SerializeField]
    protected LayerMask aimLayerMask = -1, enemyLayerMask;//=LayerMask.NameToLayer("Enemy");

    public Rigidbody body, connectedBody, previousConnectedBody;
    public Rigidbody Body { get { return body; } }
    public Transform upperBody;

    public enum GroundStatus { Ground = 0, SteepOnGround, SteepOnAir, Air }
    protected GroundStatus status;
    public GroundStatus Status { get { return status; } }

    public enum ColliderType { None = 0, Box, Sphere, Capsule }

    protected Vector3 velocity, desiredVelocity, connectionVelocity;
    protected bool desiredJump;
    protected bool desiredAim,desiredSwap;//, aimStop;
    public bool IsAiming { get { return /*!aimStop*/desiredAim; } }
    protected bool desiredThrow, desiredThrowReady;
    protected bool desiredEvade;
    protected float aimAxis;
    protected float aimTime;
    protected bool isTriggerPull;//, desiredStopFire, hammerReady;
    public bool IsTriggerPull { get { return isTriggerPull; } }
    protected bool desiredPickupWeapon;
    protected bool desiredChangeWeapon;
    protected int desiredChangeWeaponTo = 0;
    protected float triggerPullTime;
    public Ray trajectory;
    protected int jumpPhase;
    public int ForcingJumpPhase { set { jumpPhase = value; } }
    protected int stepsSinceLastGrounded, stepsSinceLastJump, stepsSinceLastAir;
    protected float onSteepTime;
    protected Vector3 upAxis, rightAxis, forwardAxis;
    protected Vector2 playerInput;
    protected Vector3 cameraInput;
    public Vector3 CameraInput;

    protected Vector3 contactNormal, steepNormal, firstWallBounceNormal, lastWallBounceNormal;

    protected int groundContactCount, steepContactCount, lastSteepContactCount;
    protected bool OnGround => groundContactCount > 0;
    public int StepsSinceLastGrounded { get { return stepsSinceLastGrounded; } }
    protected bool OnSteep => steepContactCount > 0;
    protected bool OnLastSteep;

    protected float minGroundDotProduct, minStairsDotProduct;

    protected Vector3 connectionWorldPosition;
    
    public Vector3 upperOrbitAngles;
    public Vector3 pevisOrbitAngles;

    public float hp=1;

    public delegate void GameOver();
    public static GameOver GameOverCall;
    //private Renderer directionalRangeAttackRenderer;

    public RangeAttackEffect[] rangeAttackEffect;


    protected bool isLive;

    //private void OnValidate()
    //{
    //    if (triggerType == TriggerType.SemiAuto)
    //        fireRate = Mathf.Min(forceReleaseTriggerDurtaion, fireRate);
    //}

    public virtual void Damage(float value, Vector3 attackerPos)
    {
        //Debug.Log("Damage from " + attackerPos+"/ damage : "+value);
        hp -= value;
        if(hp < 0)
        {
            Death();
        }
    }

    public virtual void Death()
    {
        Body.useGravity = true;
        Body.isKinematic = false;
        if(movementCollider!=null)
        movementCollider.material = null;

        if (throwingObject)
            throwingObject.transform.parent = null;
    }

    public virtual void OnUpdate()
    {
        UpdateInput();

        if (isTriggerPull/*desiredFire*/)
        {
            triggerPullTime += Time.deltaTime;
        }
        else
        {
            triggerPullTime = 0;
        }

        if(transform.position.y < -100f)
        {
            Death();

            gameObject.SetActive(false);
        }
    }

    public virtual void UpdateInput()
    {

    }
   
    //public Vector3 lateOrbitAngles;
  
    public delegate void OnSoundEvent(Vector3 sourcePosition, float reachRange);
    public static OnSoundEvent OnSound;

    public void InvokeSound(Vector3 sourcePosition, float reachRange)
    {
        if(OnSound!=null)
        OnSound(sourcePosition, reachRange);
    }

    public Transform bulletHole;
    public Transform bulletHoleParent;

    public void PunchHole(Vector3 pos, Vector3 normal, Transform holeParent = null)
    {
        return;
       // if (!bulletHole) 

        Transform hole = Instantiate(bulletHole, pos + normal* 0.0001f,
            Quaternion.LookRotation(normal, Vector3.up), holeParent);
    }

    public virtual void OnAwake()
    {
        myIndexNumInGMActorList = GameManager.instance.GetMyIndexInActors(this);

        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        velocityHistory = new List<RigidbodyVelocity>();

        complexDamaged = new ComplexDamage[GameManager.instance.actors.Length];
        for(int i =0; i < complexDamaged.Length; ++i)
        {
            complexDamaged[i].Init(myColliders.Length);
        }

        //thisColliderList = new List<Collider>();
        //for(int i =0; i < thisColliders.Length; ++i)
        //{
        //    thisColliderList.Add(thisColliders[i]);
        //}


        neighbours = new Collider[maxNeighbours];
        currentRangeWeaponNum = 0;
        prevRangeWeaponNum = 1;
        throwNum = 0;
        rangeAttackEffect = new RangeAttackEffect[rangeWeapon.Length];
        weaponModel = new GameObject[rangeWeapon.Length];
        directionalRangeAttackArea = new MeshCollider[rangeWeapon.Length];
        if(rangeWeapon.Length>0)
        InitWeapon(rangeWeapon[currentRangeWeaponNum]);
        UpdatePositionHistory();
        if (movementCollider == null)
        {
            movementCollider = GetComponent<Collider>();
        }
        //if (rangeWeapon.DirectionalRangeAttackArea)
        //{
        //    directionalRangeAttackRenderer = rangeWeapon.DirectionalRangeAttackArea.GetComponent<Renderer>();
        //}

        isLive = true;

        if (isUpperRotationSeperated)
        {
            upperOrbitAngles = upperBody.eulerAngles;
            upperOrbitAngles = ExtendedMathmatics.ConstrainAngles(upperOrbitAngles, minVerticalAngle, maxVerticalAngle);
            upperBody.rotation = Quaternion.Euler(upperOrbitAngles);

            pevisOrbitAngles = transform.eulerAngles;
            pevisOrbitAngles = ExtendedMathmatics.ConstrainAngles(pevisOrbitAngles, minVerticalAngle, maxVerticalAngle);
            transform.rotation = Quaternion.Euler(pevisOrbitAngles);
        }
        else
        {

            upperOrbitAngles = transform.eulerAngles;
            upperOrbitAngles = ExtendedMathmatics.ConstrainAngles(upperOrbitAngles, minVerticalAngle, maxVerticalAngle);
            transform.rotation = Quaternion.Euler(upperOrbitAngles);
        }


        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
    }

    public void UpdatePositionHistory()
    {
        if (velocityHistory.Count > positionhistoryMaxStorage)
        {
            velocityHistory.RemoveAt(0);
        }

        RigidbodyVelocity thisVelocity = new RigidbodyVelocity { position = body.transform.position, velocity = body.velocity };
        velocityHistory.Add(thisVelocity);
    }

    public RigidbodyVelocity GetPrevPosition(int lastFrame)
    {
        return velocityHistory[Mathf.Max(Mathf.Min(velocityHistory.Count - 1, positionhistoryMaxStorage) - lastFrame, 0)];
    }

    public virtual void OnFixedUpdate()
    {
        if (hp <= 0) return;
    //    Debug.Log("FixedUpdating");
        FirstFixedUpdate();

        UpdateTrajectory();

        if(desiredChangeWeapon)
        SwapWeapon();

        if (desiredChangeWeaponTo > 0)
        {
            SwapWeapon(desiredChangeWeaponTo - 1);
        }

        if (desiredPickupWeapon)
        {
            desiredPickupWeapon = false;
            if(nearByWeapon)
            PickupWeapon(nearByWeapon);
        }

        Attack();

        UpdateRotation();


      //  LastFixedUpdate();        //Call On GameManager After OnFixedUpdate()
    }

    public virtual void FirstFixedUpdate()
    {
        DesireVelocity();

        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);

        UpdateState();
        ShoulderSwapBySteepNormal(steepNormal);
        AdjustVelocity();


        if (desiredJump)
        {
            desiredJump = false;
            Jump(gravity);
        }

        ApplyGravity(gravity);

        ApplyTotalVelocity();
    }

    public virtual void ApplyTotalVelocity()
    {
        body.velocity = velocity;
    }

    public virtual void LastFixedUpdate()
    {
        UpdateHP();
        ClearState();
        UpdatePositionHistory();
    }

    public delegate void OnJumpEvent();
    public static OnJumpEvent OnJump;

    float GetMinDot(int layer)
    {
        return (stairsMask & (1 << layer)) == 0 ?
            minGroundDotProduct : minStairsDotProduct;
    }

    bool SnapToGround()
    {
        if (disableProbeTime > 0)
        {
            return false;
        }

        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance, probeMask))
        {
            return false;
        }
        float upDot = Vector3.Dot(upAxis, hit.normal);
        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }
        groundContactCount = 1;
        contactNormal = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        connectedBody = hit.rigidbody;
        return true;
    }

    bool CheckSteepContacts()
    {
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();

            if (steepNormal != firstWallBounceNormal)
            {
                //  Debug.Log("Wall Changed");
                lastWallBounceNormal = firstWallBounceNormal;

                firstWallBounceNormal = steepNormal;
            }

            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }
    public virtual void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastAir += 1;
        stepsSinceLastJump += 1;
        velocity = body.velocity;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            if (steepContactCount == 0)
            {
                status = GroundStatus.Ground;
            }
            else
            {
                status = GroundStatus.SteepOnGround;
            }

            stepsSinceLastGrounded = 0;
            jumpPhase = 0;
            if (groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            if (steepContactCount == 0)
            { status = GroundStatus.Air; }
            else
            {
                status = GroundStatus.SteepOnAir;
            }
            stepsSinceLastAir = 0;
            contactNormal = upAxis;
        }

        //  ControlVolume(windSound, OnGround || OnSteep ? Mathf.Max(windSound.volume - Time.deltaTime, 0)
        //       : Mathf.MoveTowards(windSound.volume, velocity.magnitude / maxSpeed, 1));

        if (connectedBody)
        {
            UpdateConnectionState();
        }
    }

    void UpdateConnectionState()
    {
        Debug.Log("Connection Updating");
        if (connectedBody == previousConnectedBody)
        {
            //connectionVelocity = connectedBody.velocity;

            Vector3 connectionMovement =
            connectedBody.position - connectionWorldPosition;
            connectionVelocity = connectionMovement / Time.deltaTime;
        }
        connectionWorldPosition = connectedBody.position;
    }

    public delegate void OnSwapShoulder(ShoulderStatus changeShoulder);
    public static OnSwapShoulder OnSwapShoulderEvent;

    public virtual void ShoulderSwapBySteepNormal(Vector3 normal/*, bool isAir*/)
    {
        float dot = Vector3.Dot(normal, transform.right);

        if (dot > 0.5)
        {
            if(CurrentShoulderStatus == ShoulderStatus.Left)
            SwapShoulder(ShoulderStatus.Right);
        }
        else if (dot < -0.5)
        {
            if (CurrentShoulderStatus == ShoulderStatus.Right)
                SwapShoulder(ShoulderStatus.Left);
        }
        //   steepTilt = dot * Mathf.Abs(Vector3.Dot(Vector3.Cross(CustomGravity.GetUpAxis(Vector3.zero), normal), transform.forward));


    }

    public virtual void SwapShoulder()     //Dont Add this on delegate event!
    {
        if (shoulderStatus == ShoulderStatus.Center) return;

        shoulderStatus = shoulderStatus == ShoulderStatus.Left?ShoulderStatus.Right: ShoulderStatus.Left;
        if (OnSwapShoulderEvent != null)
        {
            OnSwapShoulderEvent(shoulderStatus);
        }
        if (gunpoint)
            gunpoint.localPosition = new Vector3(-gunpoint.localPosition.x, gunpoint.localPosition.y, gunpoint.localPosition.z);
        if (throwPoint)
            throwPoint.localPosition = new Vector3(-throwPoint.localPosition.x, throwPoint.localPosition.y, throwPoint.localPosition.z);
        if (gunModelPoint)
            gunModelPoint.localPosition = new Vector3(-gunModelPoint.localPosition.x, gunModelPoint.localPosition.y, gunModelPoint.localPosition.z);
        if (weaponModel != null)
            for (int i = 0; i < weaponModel.Length; i++)
            {
                if (weaponModel[i] != null)
                {
                    weaponModel[i].transform.localPosition = new Vector3(-weaponModel[i].transform.localPosition.x, weaponModel[i].transform.localPosition.y, weaponModel[i].transform.localPosition.z);
                }
            }

        desiredSwap = false;
    }

    public virtual void SwapShoulder(ShoulderStatus changeShoulder)     //Dont Add this on delegate event!
    {
        shoulderStatus = changeShoulder;
        if (OnSwapShoulderEvent != null)
        {
            OnSwapShoulderEvent(changeShoulder);
        }
        if(gunpoint)
        gunpoint.localPosition = new Vector3(Mathf.Abs(gunpoint.localPosition.x)*(changeShoulder== ShoulderStatus.Left?-1:1), gunpoint.localPosition.y, gunpoint.localPosition.z);
        if(throwPoint)
        throwPoint.localPosition = new Vector3(Mathf.Abs(throwPoint.localPosition.x) * (changeShoulder == ShoulderStatus.Left ? -1 : 1), throwPoint.localPosition.y, throwPoint.localPosition.z);
        if(gunModelPoint)
        gunModelPoint.localPosition = new Vector3(Mathf.Abs(gunModelPoint.localPosition.x) * (changeShoulder == ShoulderStatus.Left ? -1 : 1), gunModelPoint.localPosition.y, gunModelPoint.localPosition.z);
        if (weaponModel != null)
            for (int i = 0; i < weaponModel.Length; i++)
        {
            if (weaponModel[i] != null)
            {
                weaponModel[i].transform.localPosition = new Vector3(Mathf.Abs(weaponModel[i].transform.localPosition.x) * (changeShoulder == ShoulderStatus.Left ? -1 : 1), weaponModel[i].transform.localPosition.y, weaponModel[i].transform.localPosition.z);
            }
        }

        desiredSwap = false;
    }

    public virtual void SwapShoulderEvent(ShoulderStatus changeShoulder)
    {
        shoulderStatus = changeShoulder;

        if(gunpoint)
        gunpoint.localPosition = new Vector3(gunpoint.localPosition.x * -1, gunpoint.localPosition.y, gunpoint.localPosition.z);
        if(throwPoint)
        throwPoint.localPosition = new Vector3(throwPoint.localPosition.x * -1, throwPoint.localPosition.y, throwPoint.localPosition.z);
        if(gunModelPoint)
        gunModelPoint.localPosition = new Vector3(gunModelPoint.localPosition.x * -1, gunModelPoint.localPosition.y, gunModelPoint.localPosition.z);
        if(weaponModel!=null)
        for (int i = 0; i < weaponModel.Length; ++i)
        {
            if (weaponModel[i] != null)
            {
                weaponModel[i].transform.localPosition = new Vector3(-weaponModel[i].transform.localPosition.x, weaponModel[i].transform.localPosition.y, weaponModel[i].transform.localPosition.z);
            }
        }
    }

    public virtual void DesireVelocity()
    {
      //  Debug.Log("DesireVelocity");

        if (playerInputSpace)
        {
            rightAxis = ProjectDirectionOnPlane(playerInputSpace.right, upAxis);
            forwardAxis =
                ProjectDirectionOnPlane(playerInputSpace.forward, upAxis);
        }
        else
        {
            rightAxis = ProjectDirectionOnPlane(Vector3.right, upAxis);
            forwardAxis = ProjectDirectionOnPlane(Vector3.forward, upAxis);
        }

        float speed = maxSpeed;
        if (!OnGround)
        {
            Vector3 horizontalVelocity = new Vector3(body.velocity.x, 0, body.velocity.z);

            speed =  Mathf.Max(horizontalVelocity.magnitude, maxSpeed);
        }


        desiredVelocity =
              new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
    }

    public virtual void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;

        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        }
        else
        {
            return;
        }
        
        //leftVibration += 0.2f;
        //PlaySound(jumpSound);

        stepsSinceLastJump = 0;
        if (stepsSinceLastJump > 1)
        {
            jumpPhase = 0;
        }
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(2f * gravity.magnitude * jumpHeight);
        jumpDirection = (jumpDirection + upAxis).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        else
        {
            jumpSpeed = jumpSpeed + velocity.y * -1;
        }

        velocity += jumpDirection * jumpSpeed;

        Vector3 desiredToWorld = playerInputSpace.TransformVector(desiredVelocity);

        if (!OnSteep && desiredToWorld.magnitude > 0.1f)
        {
            velocity.x = desiredToWorld.x > 0 ? Mathf.Max(velocity.x, desiredToWorld.x * jumpVelocityMultiplier) : Mathf.Min(velocity.x, desiredToWorld.x * jumpVelocityMultiplier);
            velocity.z = desiredToWorld.z > 0 ? Mathf.Max(velocity.z, desiredToWorld.z * jumpVelocityMultiplier) : Mathf.Min(velocity.z, desiredToWorld.z * jumpVelocityMultiplier);
        }

        if (OnJump != null)
        {
            OnJump();
        }
    }

    public static Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }
    public virtual void AdjustVelocity()
    {
        if (!OnGround && playerInput.magnitude < 0.1f)
        {
            return;
        }

        Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

        Vector3 relativeVelocity = velocity - connectionVelocity;

        float currentX = Vector3.Dot(/*velocity*/relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(/*velocity*/relativeVelocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;// * playerInput.magnitude;
        float maxSpeedChange = acceleration * Time.deltaTime;


        float newX =
            Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);// *sleepMultiler;
        float newZ =
            Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);// * sleepMultiler;

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);

        
    }

    protected bool CheckWallDetach()
    {
        if (OnLastSteep && OnLastSteep != OnSteep)
        {
            if (firstWallBounceNormal != lastWallBounceNormal)
            {
                return true;
            }

        }
        return false;
    }
    protected bool CheckWallAttach()
    {
        return !OnLastSteep && OnLastSteep != OnSteep;
    }


    public delegate void OnSteepEnter(Vector3 normal, bool isAir);
    public static OnSteepEnter onSteepEnter;

    public delegate void OnSteepStay(Vector3 normal, bool isAir);
    public static OnSteepStay onSteepStay;

    public delegate void OnSteepExit(Vector3 normal, bool isAir);
    public static OnSteepExit onSteepExit;

    public virtual void ClearState()
    {
        if (CheckWallDetach())
        {
            onSteepTime = 0;
            lastWallBounceNormal = firstWallBounceNormal;
            if (onSteepExit != null)
                onSteepExit(lastWallBounceNormal, !OnGround);
        }
        if (OnSteep)
        {
            if (onSteepStay != null)
                onSteepStay(steepNormal, !OnGround);
        }
        if (CheckWallAttach())
        {
            // Debug.Log("Steep Attached!");
            firstWallBounceNormal = steepNormal;
            if (onSteepEnter != null)
                onSteepEnter(firstWallBounceNormal, !OnGround);
        }
        OnLastSteep = OnSteep;

        previousConnectedBody = connectedBody;
        connectedBody = null;

        lastSteepContactCount = steepContactCount;
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = connectionVelocity = Vector3.zero;
    }

    void OnCollisionEnter(Collision collision)
    {
        //leftVibration += 0.1f;
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        if (collision.transform.CompareTag("Throwable")) return;

        float minDot = GetMinDot(collision.gameObject.layer);

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= minDot)
            {
                groundContactCount += 1;
                contactNormal += normal;
                connectedBody = collision.rigidbody;
                lastWallBounceNormal = CustomGravity.GetUpAxis(Vector3.zero);
            }
            else if (upDot > -0.01f)
            {
                steepContactCount += 1;
                steepNormal += normal;
                if (groundContactCount == 0)
                {
                    connectedBody = collision.rigidbody;
                }
            }
        }

    }

    public virtual void ApplyGravity(Vector3 gravity)
    {
        
            velocity += gravity * Time.deltaTime;
        
    }

    public virtual void UpdateTrajectory()
    {
        trajectory = new Ray(gunpoint.position, gunpoint.forward);
    }

    public virtual void Attack()
    {
        if (throwableEquipment.Length > 0)
        {
            if (desiredThrowReady)
            {
                ThrowReady();
                return;
            }

            if (desiredThrow)
            {
                ThrowObject();
                return;
            }
        }

        if (rangeWeapon.Length<1)        return;

        if (rangeWeapon[currentRangeWeaponNum].CurrentTriggerType != RangeWeapon.TriggerType.ManualAction)
        {
            chamberLoadTime += Time.deltaTime;
        }


        float firerateMultiplier = 1;

        if (SceneLoader.instance)
        {
            if(SceneLoader.currentDifficulty()== Difficulty.Easy)
            {
                firerateMultiplier *=4;
            }
        }

        //if (rangeWeapon.AttackType == RangeWeapon.RangeAttackType.DirectionalRanged)
        //{ directionalRangeAttackRenderer.enabled = false;
        //  //  directionalRangeAttackArea.transform.position = gunpoint.position;
        //  //  directionalRangeAttackArea.transform.rotation = gunpoint.rotation;
        //}
        {
            switch (rangeWeapon[currentRangeWeaponNum].CurrentTriggerType)
            {
                case RangeWeapon.TriggerType.FullAuto:
                    if (chamberLoadTime > rangeWeapon[currentRangeWeaponNum].FireRate * firerateMultiplier && triggerPullTime > 0)
                    {
                        FireRangeWeapon();
                    }
                    break;
                case RangeWeapon.TriggerType.SemiAuto:
                    if (chamberLoadTime > Mathf.Max(rangeWeapon[currentRangeWeaponNum].ForceReleaseTriggerDuration, rangeWeapon[currentRangeWeaponNum].FireRate * firerateMultiplier) 
                        && triggerPullTime < rangeWeapon[currentRangeWeaponNum].ForceReleaseTriggerDuration && triggerPullTime > 0)
                    {
                        FireRangeWeapon();
                    }

                    break;
                case RangeWeapon.TriggerType.Burst:
                    if (burstFired == 0 && chamberLoadTime > rangeWeapon[currentRangeWeaponNum].BurstRate &&  triggerPullTime < rangeWeapon[currentRangeWeaponNum].ForceReleaseTriggerDuration && triggerPullTime > 0)
                    {
                            FireRangeWeapon();
                            burstFired++;
                            return;
                    }
                    if (burstFired >= 1&& chamberLoadTime > rangeWeapon[currentRangeWeaponNum].FireRate)
                    {
                        FireRangeWeapon();
                        if (++burstFired>= rangeWeapon[currentRangeWeaponNum].BurstShotCountWhenTriggerPulled)
                        {
                            burstFired = 0;
                        }
                        return;
                    }
                     
                    break;
                case RangeWeapon.TriggerType.ManualAction:
                    break;
            }
        }
        

        
    }

    public virtual void SwapWeapon()
    {
        desiredChangeWeapon = false;

        if (weaponModel[prevRangeWeaponNum] ==null)//rangeWeapon[prevRangeWeaponNum] == null)
        { return; }
        weaponModel[prevRangeWeaponNum].SetActive(true);
        weaponModel[currentRangeWeaponNum].SetActive(false);
        int changedWeaponNum = currentRangeWeaponNum;
        currentRangeWeaponNum = prevRangeWeaponNum;
        prevRangeWeaponNum = changedWeaponNum;
    }

    public virtual void SwapWeapon(int num)
    {
        desiredChangeWeapon = false;
        

        if (weaponModel[num]==null||num==currentRangeWeaponNum)//rangeWeapon[num] == null)
        { desiredChangeWeaponTo = 0; return; }

        desiredChangeWeaponTo = 0;

        weaponModel[currentRangeWeaponNum].SetActive(false);
        weaponModel[num].SetActive(true);
        int changedWeaponNum = currentRangeWeaponNum;
        currentRangeWeaponNum = num;
        prevRangeWeaponNum = changedWeaponNum;

    }

    public virtual void InitWeapon(RangeWeapon pickup)
    {
        RangeWeapon newWeapon = pickup;
        rangeWeapon[currentRangeWeaponNum] = newWeapon;

        if (directionalRangeAttackArea[currentRangeWeaponNum] != null)
            Destroy(directionalRangeAttackArea[currentRangeWeaponNum].gameObject);

        if (newWeapon.AttackType == RangeWeapon.RangeAttackType.DirectionalRanged)
        {
            directionalRangeAttackArea[currentRangeWeaponNum] = Instantiate(newWeapon.DirectionalRangeAttackArea, gunpoint.position, gunpoint.rotation, upperBody);
        }
        if (isWeaponModelVisible)
            weaponModel[currentRangeWeaponNum] = Instantiate(newWeapon.WeaponModel, gunModelPoint.position, gunModelPoint.rotation, upperBody);
        rangeAttackEffect[currentRangeWeaponNum] = rangeWeapon[currentRangeWeaponNum].InitAnimation(gunpoint);
    }

    public void WeaponResetting(RangeWeapon pickup, int num)
    {
        RangeWeapon newWeapon = pickup;
        rangeWeapon[num] = newWeapon;

        if (directionalRangeAttackArea[num] != null)
            Destroy(directionalRangeAttackArea[num].gameObject);

        if (newWeapon.AttackType == RangeWeapon.RangeAttackType.DirectionalRanged)
        {
            directionalRangeAttackArea[num] = Instantiate(newWeapon.DirectionalRangeAttackArea, gunpoint.position, gunpoint.rotation, upperBody);
        }
        if (isWeaponModelVisible)
        {
            weaponModel[num] = Instantiate(newWeapon.WeaponModel, gunModelPoint.position, gunModelPoint.rotation, upperBody);
        }
        rangeAttackEffect[num] = rangeWeapon[num].InitAnimation(gunpoint);
    }

    public void PickupWeapon(ThrowedWeapon pickup)
    {
        desiredChangeWeapon = false;

        for (int i = 0; i < rangeWeapon.Length; i++)
        {
            if (rangeWeapon[i] == null)
            {
                //RangeWeapon newWeapon = pickup.thisWeapon;
                //rangeWeapon[i] = newWeapon;

                //if (directionalRangeAttackArea[i] != null)
                //    Destroy(directionalRangeAttackArea[currentRangeWeaponNum].gameObject);

                //if (newWeapon.AttackType == RangeWeapon.RangeAttackType.DirectionalRanged)
                //{
                //    directionalRangeAttackArea[i] = Instantiate(newWeapon.DirectionalRangeAttackArea, gunpoint.position, gunpoint.rotation, upperBody);
                //}
                //if (isWeaponModelVisible)
                //    weaponModel[i] = Instantiate(newWeapon.WeaponModel, gunModelPoint.position, gunModelPoint.rotation, upperBody);
                //rangeAttackEffect[i] = rangeWeapon[i].InitAnimation(gunpoint);
                WeaponResetting(pickup.thisWeapon, i);
                SwapWeapon(i);
                desiredPickupWeapon = false;
                Destroy(pickup.gameObject);

                return;
            }
        }

        ThrowWeapon();

        WeaponResetting(pickup.thisWeapon, currentRangeWeaponNum);

        desiredPickupWeapon = false;

        Destroy(pickup.gameObject);
    }
    //return false;

    protected ThrowedWeapon nearByWeapon;
    public ThrowedWeapon NearByWeapon { get { return nearByWeapon; } set { nearByWeapon = value; } }

    public void ThrowWeapon()
    {
        ThrowedWeapon throwingWeapon = Instantiate(weaponModel[currentRangeWeaponNum], throwPoint.position, Quaternion.Euler(new Vector3(upperOrbitAngles.x, upperOrbitAngles.y + throwUpPower, upperOrbitAngles.z))).AddComponent<ThrowedWeapon>();
        throwingWeapon.thisWeapon = rangeWeapon[currentRangeWeaponNum];
        throwingWeapon.Grabable();

        ThrowObjectImmediately(throwingWeapon);

        rangeWeapon[currentRangeWeaponNum] = null;

        if (directionalRangeAttackArea[currentRangeWeaponNum] != null)
            Destroy(directionalRangeAttackArea[currentRangeWeaponNum].gameObject);

        //weaponModel[currentRangeWeaponNum] = null;
        Destroy(weaponModel[currentRangeWeaponNum]);

    }


    public virtual void ThrowReady()
    {
        desiredThrowReady = false;

        //if (throwableEquipment[throwNum].gameObject.activeInHierarchy) return;

        //throwingObject = Instantiate(throwableEquipment, throwPoint.position, Quaternion.Euler(new Vector3(upperOrbitAngles.x, upperOrbitAngles.y + throwUpPower, upperOrbitAngles.z)),this.transform);
        throwingObject = throwableEquipment[throwNum];

        throwingObject.transform.parent = this.throwPoint;
        throwingObject.transform.localPosition = Vector3.zero;//throwPoint.localPosition ;
        throwingObject.transform.localRotation = Quaternion.identity;
        throwingObject.gameObject.SetActive(true);
        

        throwingObject.Body.useGravity = false;
        throwingObject.Body.isKinematic = true;
        //throwingObject.transform.parent = this.transform;


        throwNum = throwNum >= throwableEquipment.Length - 1 ? 0 : throwNum + 1;

    }

    public virtual void ThrowReady(Throwable throwObj)
    {
        throwObj.transform.parent = this.throwPoint;
        throwObj.transform.localPosition = Vector3.zero;//throwPoint.localPosition ;
        throwObj.transform.localRotation = Quaternion.identity;
        throwObj.gameObject.SetActive(true);


        throwObj.Body.useGravity = false;
        throwObj.Body.isKinematic = true;
    }

    public virtual void ThrowObject()
    {
        desiredThrow = false;

        if (!throwingObject) return;

        throwingObject.transform.parent = null;

        throwingObject.Body.useGravity = throwingObject.enableCustomGravity ? false : true;
        throwingObject.Body.isKinematic = false;


        Vector3 add = Vector3.zero;
        switch (addBodyVelocityOnThrowable)
        {
            case Throwable.AddVelocityType.AllDirection:
                add = body.velocity;
                break;
            case Throwable.AddVelocityType.OnlyFrontal:
                Vector3 localBodyVelocity = playerInputSpace.InverseTransformVector(body.velocity);
                localBodyVelocity.x = 0;
                localBodyVelocity.y = 0;
                add = playerInputSpace.TransformVector(localBodyVelocity);
                break;
        }

        throwingObject.Throw( upperBody.forward * throwPower + new Vector3(0, throwUpPower, 0) + add);

        throwingObject = null;
    }

    public virtual void ThrowObject(Throwable throwObj, float throwForce, float throwUpForce)
    {
        throwObj.transform.parent = null;

        throwObj.Body.useGravity = throwObj.enableCustomGravity ? false : true;
        throwObj.Body.isKinematic = false;


        Vector3 add = Vector3.zero;
        switch (addBodyVelocityOnThrowable)
        {
            case Throwable.AddVelocityType.AllDirection:
                add = body.velocity;
                break;
            case Throwable.AddVelocityType.OnlyFrontal:
                Vector3 localBodyVelocity = playerInputSpace.InverseTransformVector(body.velocity);
                localBodyVelocity.x = 0;
                localBodyVelocity.y = 0;
                add = playerInputSpace.TransformVector(localBodyVelocity);
                break;
        }

        throwObj.Throw(upperBody.forward * throwForce + new Vector3(0, throwUpForce, 0) + add);
    }

    public virtual void ThrowObject(Throwable throwObj, Vector3 throwDirection, float throwForce, float throwUpForce)
    {
        throwObj.transform.parent = null;

        throwObj.Body.useGravity = throwObj.enableCustomGravity ? false : true;
        throwObj.Body.isKinematic = false;


        Vector3 add = Vector3.zero;
        switch (addBodyVelocityOnThrowable)
        {
            case Throwable.AddVelocityType.AllDirection:
                add = body.velocity;
                break;
            case Throwable.AddVelocityType.OnlyFrontal:
                Vector3 localBodyVelocity = playerInputSpace.InverseTransformVector(body.velocity);
                localBodyVelocity.x = 0;
                localBodyVelocity.y = 0;
                add = playerInputSpace.TransformVector(localBodyVelocity);
                break;
        }

        throwObj.Throw( throwDirection * throwForce + new Vector3(0, throwUpForce, 0) + add);
    }

    Throwable ThrowObjectImmediately(Throwable throwing)
    {
        
        throwing.transform.parent = null;

        throwing.Body.useGravity = throwing.enableCustomGravity ? false : true;
        throwing.Body.isKinematic = false;


        Vector3 add = Vector3.zero;
        switch (addBodyVelocityOnThrowable)
        {
            case Throwable.AddVelocityType.AllDirection:
                add = body.velocity;
                break;
            case Throwable.AddVelocityType.OnlyFrontal:
                Vector3 localBodyVelocity = playerInputSpace.InverseTransformVector(body.velocity);
                localBodyVelocity.x = 0;
                localBodyVelocity.y = 0;
                add = playerInputSpace.TransformVector(localBodyVelocity);
                break;
        }

        throwing.Throw(  upperBody.forward * throwPower + new Vector3(0, throwUpPower, 0) + add);

        return throwing;
    }


    public virtual /*bool*/void FireRangeWeapon()
    {
        switch (rangeWeapon[currentRangeWeaponNum].AttackType)
        {
            case RangeWeapon.RangeAttackType.Hitscan:
                // firingLineBlink = firingLineBlink ? false : true;
                //    aimSight.material.SetColor("_BaseColor", firingLineBlink ? Color.yellow : Color.white);
                RaycastHit hit;

                if (Physics.Raycast(trajectory, out hit, rangeWeapon[currentRangeWeaponNum].MaximumAttackRange, aimLayerMask))
                {
                    //inFiringLine = true;
                    ActionBaseComponent tgt = hit.transform.GetComponent<ActionBaseComponent>();
                    if (tgt!=null)
                    {
                        tgt.Damage(rangeWeapon[currentRangeWeaponNum].AttackDamage * rangeWeapon[currentRangeWeaponNum].AttackEffectByDistance.Evaluate(hit.distance / rangeWeapon[currentRangeWeaponNum].MaximumAttackRange), transform.position);

                        PunchHole(hit.point, hit.normal, hit.transform);
                    }
                    else           // terrain scale makes hole rchitecture
                    {
                        PunchHole(hit.point, hit.normal, null);
                    }
                    rangeAttackEffect[currentRangeWeaponNum].PlayShootAnimation(trajectory.origin, hit.point, rangeWeapon[currentRangeWeaponNum].AttackType);
                }
                else
                {
                    rangeAttackEffect[currentRangeWeaponNum].PlayShootAnimation(trajectory.origin, trajectory.origin + trajectory.direction * rangeWeapon[currentRangeWeaponNum].MaximumAttackRange, rangeWeapon[currentRangeWeaponNum].AttackType);
                }
                break;
            case RangeWeapon.RangeAttackType.Projectile:
                bullets[firedBullet].gameObject.SetActive(true);
                bullets[firedBullet].Fire(trajectory.origin, trajectory.direction * rangeWeapon[currentRangeWeaponNum].ProjectileSpeed, this);
                break;
            case RangeWeapon.RangeAttackType.DirectionalRanged:

                int count = Physics.OverlapSphereNonAlloc(directionalRangeAttackArea[currentRangeWeaponNum].bounds.center, directionalRangeAttackArea[currentRangeWeaponNum].bounds.extents.magnitude, neighbours, enemyLayerMask);
                // List<ActionBaseComponent> dealedEnemies = new List<ActionBaseComponent>();

                Physics.autoSyncTransforms = true;

                for (int i = 0; i < count; ++i)
                {
                    var collider = neighbours[i];

                    //if (thisColliderList.Exists(x=>(collider||directionalRangeAttackArea))){
                    //    continue;
                    //}

                    Vector3 otherPosition = collider.transform.position;
                    Quaternion otherRotation = collider.transform.rotation;

                    Vector3 direction;
                    float distance;

                    if (Physics.ComputePenetration(directionalRangeAttackArea[currentRangeWeaponNum], directionalRangeAttackArea[currentRangeWeaponNum].transform.position, directionalRangeAttackArea[currentRangeWeaponNum].transform.rotation,
                        collider, otherPosition, otherRotation, out direction, out distance))
                    {
                        ActionBaseComponent tgt= collider.transform.GetComponent<ActionBaseComponent>();
                        if (tgt!=null)
                        {
                            //tgt = collider.transform.GetComponent<ActionBaseComponent>();
                            //  if (!dealedEnemies.Exists(x => tgt))
                            {
                                if (Physics.Raycast(trajectory.origin, (tgt.transform.position - trajectory.origin).normalized, out hit, rangeWeapon[currentRangeWeaponNum].MaximumAttackRange, aimLayerMask))
                                {
                                    if (hit.transform.GetComponent<ActionBaseComponent>())
                                    {
                                        /* float damage = rangeWeapon[currentRangeWeaponNum].AttackDamage * rangeWeapon[currentRangeWeaponNum].AttackEffectByDistance.Evaluate(hit.distance/ rangeWeapon[currentRangeWeaponNum].MaximumAttackRange)
                                         * rangeWeapon[currentRangeWeaponNum].AttackEffectByCenterDistance.Evaluate(Vector3.Dot(trajectory.direction, (hit.point - trajectory.origin).normalized));
                                         tgt.Damage(damage, transform.position);*/
                                        SpreadFire(hit.point, hit.distance, tgt);
                                        //  dealedEnemies.Add(tgt);
                                    }
                                }
                            }
                        }
                        else if (collider.transform.parent != null)
                        {
                            tgt = collider.transform.parent.GetComponent<ActionBaseComponent>();
                            if (tgt!=null)
                            {
                                // if (!dealedEnemies.Exists(x => tgt))
                                {
                                    if (Physics.Raycast(trajectory.origin, (tgt.transform.position - trajectory.origin).normalized, out hit, rangeWeapon[currentRangeWeaponNum].MaximumAttackRange, aimLayerMask))
                                    {
                                        if (hit.transform.GetComponent<ActionBaseComponent>())
                                        {
                                            SpreadFire(hit.point, hit.distance, tgt);
                                            //       dealedEnemies.Add(tgt);
                                        }
                                    }
                                }

                            }
                        }

                    };

                }

                Physics.autoSyncTransforms = false;

                // directionalRangeAttackRenderer.enabled = true;

                rangeAttackEffect[currentRangeWeaponNum].PlayShootAnimation(trajectory.origin, trajectory.origin + trajectory.direction, rangeWeapon[currentRangeWeaponNum].AttackType);
                break;
        }

        firedBullet = firedBullet >= bullets.Length - 1 ? 0 : firedBullet + 1;

        // rangeWeapon[currentRangeWeaponNum].PlayShootAnimation(trajectory.origin, trajectory.origin + trajectory.direction * rangeWeapon[currentRangeWeaponNum].MaximumAttackRange);

        if (OnSound != null)
            ActionBaseComponent.OnSound(transform.position, gunShotSoundRange);

        chamberLoadTime = 0;
        // return true;
    }

    public virtual void SpreadFire(Vector3 hitPoint, float hitDistance, ActionBaseComponent tgt)
    {

        float damage = rangeWeapon[currentRangeWeaponNum].AttackDamage * rangeWeapon[currentRangeWeaponNum].AttackEffectByDistance.Evaluate(hitDistance/ rangeWeapon[currentRangeWeaponNum].MaximumAttackRange)
     * rangeWeapon[currentRangeWeaponNum].AttackEffectByCenterDistance.Evaluate(Vector3.Dot(trajectory.direction, (hitPoint - trajectory.origin).normalized));
        //tgt.complexDamaged[myIndexNumInGMActorList].Damaged(damage, transform.position);
        tgt.DealComplexDamage(myIndexNumInGMActorList, damage, transform.position);
    }

    //private void OnDrawGizmos()
    //{
    //    if (!directionalRangeAttackArea) return;

    //    int count = Physics.OverlapSphereNonAlloc(directionalRangeAttackArea.bounds.center, directionalRangeAttackArea.bounds.size.magnitude, neighbours);

    //    for (int i = 0; i < count; ++i)
    //    {
    //        var collider = neighbours[i];

    //        //if (thisColliderList.Exists(x=>(collider||directionalRangeAttackArea))){
    //        //    continue;
    //        //}

    //        Vector3 otherPosition = collider.gameObject.transform.position;
    //        Quaternion otherRotation = collider.gameObject.transform.rotation;

    //        Vector3 direction;
    //        float distance;

    //        if (Physics.ComputePenetration(directionalRangeAttackArea, directionalRangeAttackArea.transform.position, directionalRangeAttackArea.transform.rotation,
    //            collider, otherPosition, otherRotation, out direction, out distance))
    //        {

    //            Gizmos.color = Color.red;
    //            Gizmos.DrawRay(otherPosition, direction * distance);
    //            /*hit.transform.getcomponent<actionbasecomponent>()*/
    //            // tgt.damage(1f, transform.position);

    //        };

    //    }
    //}


    public virtual void UpdateRotation()
    {
        if (rotationSpeed <= 0) return;

       // Debug.Log("Rotating");
            Vector3 turnAngles = Vector3.zero;
            
            const float e = 0.0001f;
            if (cameraInput.x < -e || cameraInput.x > e || cameraInput.y < -e || cameraInput.y > e)
            {
            
                {
                    turnAngles = rotationSpeed * Time.deltaTime *
                        cameraInput
                        * (desiredAim ? rotationSpeedMultiplyWhenZoom : 1);

                }
                upperOrbitAngles += turnAngles;
                //else
            }


            upperOrbitAngles = ExtendedMathmatics.ConstrainAngles(upperOrbitAngles, minVerticalAngle, maxVerticalAngle);
            /*gunTf*/
            upperBody.localRotation = Quaternion.Euler(new Vector3(upperOrbitAngles.x, 0, 0));
            body.MoveRotation(Quaternion.Euler(new Vector3(0, upperOrbitAngles.y, 0)));
        
    }

    protected ComplexDamage[] complexDamaged;
    public void DealComplexDamage(int index, float damage, Vector3 attackPos) { complexDamaged[index].Damaged(damage,attackPos); }

    [System.Serializable]
    public struct ComplexDamage
    {
        public int index;
        public Vector3 attackerPosition;
        public float[] damaged;

        public void Init(int myColliderCount)
        {
            damaged = new float[myColliderCount];
            for(int i =0; i < myColliderCount; ++i)
            {
                damaged[i] = 0;
            }
        }

        public void Damaged(float damage, Vector3 attackPos) {
            if (index >= damaged.Length)
                return;

          //  Debug.Log("Damage On Complex Collider");
            damaged[index] = damage;
            if (index == 0) attackerPosition = attackPos;
            index++;
        }

        public float FeedbackMaxDamage(out Vector3 attackerPos)
        {
            float result = 0;
            for(int i = 0; i < index; i++)
            {
                if(result < damaged[i])
                {
                    result = damaged[i];
                }
            }
            index = 0;
         //   Debug.Log("max damage is" + result);

            attackerPos = attackerPosition;

            return result;
        }
    }

    public virtual void UpdateHP()
    {
        for(int i = 0; i < complexDamaged.Length; ++i)
        {
            if (complexDamaged[i].index > 0)
            {
                Vector3 pos;
                float damage = complexDamaged[i].FeedbackMaxDamage(out pos);
                Damage(damage, pos);
            }
        }

        if(hp < 0)
        {
            Death();
            return;
        }

        if (lastDamageTakenTime > recoveryDelay)
        {
            hp = Mathf.Min(hp + recoverHealthPerSec * Time.deltaTime, 1);
        }
        else
        {
            lastDamageTakenTime += Time.deltaTime;
        }
    }

    public virtual void OnLateUpdate()
    {
       // nearByWeapon = null;

       // desiredPickupWeapon = false;
    }

    [SerializeField]private AudioSource audioSource;

    public virtual void Sound(AudioClip sfx)
    {
        audioSource.PlayOneShot(sfx);
    }

    public virtual void Talk(AudioClip sound)
    {
        if (audioSource.isPlaying)
        { audioSource.Stop(); }
        audioSource.PlayOneShot(sound);
    }

}
