﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAim : ActionBaseComponent
{
    public enum EnemyStatus { Eliminated = -1 ,Idle = 0, Chasing, Attack, Alert}
    EnemyStatus enemyStatus = EnemyStatus.Idle;

    public enum EnemyAIType { Tracker = 0, StaticSpread =1, Grenadier, VeteranGrenadier, Follower, Ambusher }
    [SerializeField] EnemyAIType AIType;

    [SerializeField] Color idleColor = Color.cyan;//new Color(1,0.5f,0.5f);
    [SerializeField] Color alertColor = Color.yellow;
    [SerializeField] Color chasingColor = new Color(1, 0.7f, 0.5f);
    [SerializeField] Color attackColor = Color.red;
    [SerializeField] Color eliminatedColor = Color.black;
    [SerializeField] PerObjectMaterialProperties turretLook;

   // [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform destination;
    [SerializeField] float movingStopDistance = 1f;

    public ActionBaseComponent target;

    // private Queue<Vector3> targetPosHistory;
    [SerializeField] float trackDelayDuration=0.3f;
    private int trackDelayTick;
    //public float correctAimDuration=0.3f, incorrectAimDuration = 0.5f;
    private float aimSettingTime;

    public float attackDelay = 0.15f;
    private float targetInsightTime;

    //public float damagePerSec = 3f;

    //public Vector3 upperOrbitAngles;
    public float trackingTurnAnglePerSec = 30f;

    [Range(0,1)]
    public float leadShotPersentage = 1;
    
    public LayerMask terrainLayer;

   // public Transform bulletHole;

    public /*Line*/Renderer aimSight;
   // public LayerMask aimLayerMask = -1;
    public LayerMask obsticalLayerMask = -1;

    public CapsuleCollider assistCollider;

    bool inSight;
    float howCloseToCenter;
    //bool inFiringLine;
    // bool nearBy;
    bool closeContact;
    bool noObstacleBetweenDistance;
    RaycastHit firingLineHit;


    private Vector3 turnAngles;
    private Vector3 aimPos;
    Vector3 lastTurnAngles;
    Vector3 lastTargetLocation;
    Vector3 anglesToLastTgtLoc;
    //public Transform box;
    public float alertDuration = 10f;
    float alertTime;

    bool firingLineBlink;

    public float sentryTurnSpeed = 10f;
    [Range(0, 1)]
    public float targetFowardDetectRadius = 0.2f;
    public float myAroundDetectRadius = 5f;
    public float targetDetectRange = 100f;
    public AnimationCurve seekingCurveHorizontal = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(1, 0f));
    public AnimationCurve seekingCurveVertical = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(1, 0f));
    public Vector2 seekRange = new Vector2(20f, 50f);

    Vector3 seekingPosition;
    float remainTimeToSeekStart;
    float remainTimeToSeekAngle;
    Vector3 lockAnglesToSeek;
    [SerializeField]
    float seekDelay = 0.8f;

    bool turnedToSeekPosition;

    [SerializeField] bool updateMovementByCode = true;
    [SerializeField] bool ignoreAICulling;
    //public Rigidbody aimSpot;

    public void AlertOnSound(Vector3 sourcePos, float distance)
    {
        if(Vector3.Distance(sourcePos, transform.position) > distance||enemyStatus== EnemyStatus.Attack||enemyStatus== EnemyStatus.Chasing)
        {
            return;
        }

        seekingPosition = sourcePos;
     //   alertTime = 0;
        ChangeStatus(inSight ? EnemyStatus.Chasing:EnemyStatus.Alert);
    }

    //bool isLive;
    private void Awake()
    {
        OnValidate();
        

        if (bullets.Length > 0)
        {
            bullets[0].transform.parent.transform.parent = null;
        }
    }

   

    public override void Death()
    {

        isLive = false;
        if (aimSight)
        { aimSight.enabled = false; }
        if (assistCollider)
        {
            assistCollider.gameObject.SetActive(false);
            assistCollider.enabled = false;
        }
        if(ActionBaseComponent.OnSound!=null)
            ActionBaseComponent.OnSound -= AlertOnSound;
        
        ChangeStatus(EnemyStatus.Eliminated);

        base.Death();
    }

    private void OnDestroy()
    {
        if(isLive)
        ActionBaseComponent.OnSound -= AlertOnSound;
    }

    // Start is called before the first frame update
    public void OnStart()
    {
        //base.OnAwake();


        // isLive = true;
        ActionBaseComponent.OnSound += AlertOnSound;
        if (assistCollider)
            assistCollider.transform.parent = null;

        if (!target)
        {
            target = GameObject.FindWithTag("Player").GetComponent<ActionBaseComponent>(); //ActionBaseComponent.FindObjectOfType(typeof(ActionBaseComponent)) as ActionBaseComponent;
        }

        trackDelayTick = (int)(trackDelayDuration * 60f);

        if(aimSight) aimSight.material.SetColor("_BaseColor", Color.cyan);
        if(turretLook) turretLook.SetColor(idleColor);

        if(AIType== EnemyAIType.StaticSpread)
        {
            enemyStatus = EnemyStatus.Attack;
            lockAnglesToSeek = upperOrbitAngles;
            
            //enemyStatus = EnemyStatus.Attack;
        }

        throwDelay = throwDuration+1f;
    }

    //Quaternion prevRotation;
    //private void Update()
    //{
    //    float lerpFactor = ((Time.time - Time.fixedTime) / Time.fixedDeltaTime);

    //    transform.rotation = Quaternion.Slerp(prevRotation,body.rotation, lerpFactor);
    //}
    bool attackable;

    public override void OnFixedUpdate()
    {
        if (hp <= 0) return;

        playerInput = Vector2.zero;

        if (GameManager.instance.orbitCamera.transform.InverseTransformPoint(target.transform.position).z - 1.65f
             > GameManager.instance.orbitCamera.transform.InverseTransformPoint(transform.position).z&&!ignoreAICulling ||
             (target.transform.position.y < activationHeight.x||target.transform.position.y > activationHeight.y))
        {
            attackable = false;
        }
        else
        {
            attackable = true;
        }

        

        if (updateMovementByCode)
        {
            base.FirstFixedUpdate();
            base.UpdateRotation();

            UpdateAimAI();
        }

        //UpdateMovingAI(); Moving To OnUpdate


        base.UpdateTrajectory();

        //if (shootLine)
        //    shootLine.enabled = false;

        /*base.*/Attack();


        //base.UpdateHP();

       // base.LastFixedUpdate();
    }

    public override void Attack()
    {
        if (!attackable)
            return;
       
        base.Attack();
    }

    public void UpdateAimAI()
    {
        if (!attackable)
        {
            cameraInput = Vector3.zero;
            return;
        }

        switch (AIType)
        {
            case EnemyAIType.Tracker:
                if (Vector3.Distance(transform.position, target.GetPrevPosition(0).position) < triggingDistance)
                {
                    TrackerAIUpdate();
                }
                break;
            case EnemyAIType.StaticSpread:
         //       Trigging();
                RotateByPreset();
                break;
            case EnemyAIType.Grenadier:
                
                ThrowEquipment();
                RotateByPreset();
                break;
            case EnemyAIType.VeteranGrenadier:
                CalculateThrowAngle();
                if (Vector3.Distance(transform.position, target.GetPrevPosition(0).position) < triggingDistance)
                { ThrowEquipment(); }
                break;
            case EnemyAIType.Follower:
                if (following.IsTriggerPull)
                {
                    RotateByPreset();
                    enemyStatus = EnemyStatus.Attack;

                    if (throwableEquipment.Length > 0)
                    {
                        ThrowEquipment();
                    }
                    //     Trigging();
                }
                else
                {
                    enemyStatus = EnemyStatus.Idle;
                }
                break;
            case EnemyAIType.Ambusher:
                if (//GameManager.instance.orbitCamera.transform.InverseTransformPoint(target.transform.position).z 
                   target.transform.position.z > triggingDistance)
                {
                    RotateByPreset();
                    enemyStatus = EnemyStatus.Attack;
                    //if (rangeWeapon.Length > 0)
                    //{

                    //        Trigging();

                    //}
                    if (throwableEquipment.Length > 0)
                    {
                        ThrowEquipment();
                    }
                }
                else
                {
                    enemyStatus = EnemyStatus.Idle;
                }
                break;
        }

        cameraInput = Vector3.ClampMagnitude(cameraInput, 1f);
    }

    int currentCornerIndex;
    private float startUpperAngle=-9999f;

    private void RotateByPreset()
    {
        if (startUpperAngle == -9999f)
        {

            startUpperAngle = ExtendedMathmatics.ConstrainAngles(upperOrbitAngles, -90f, 90).y;
        }

        float time = alertTime / alertDuration;
        Vector3 progressAngle = new Vector3(seekingCurveVertical.Evaluate(time) * seekRange.x, seekingCurveHorizontal.Evaluate(time) * seekRange.y, 0)/* + lockAnglesToSeek*/ /*+ upperOrbitAngles*/;//+ anglesToLastTgtLoc;
                                                                                                                                                                                                     // Debug.Log(progressAngle);
        cameraInput.x = (progressAngle.x-upperOrbitAngles.x) / Time.deltaTime / rotationSpeed;
        cameraInput.y = (progressAngle.y) / Time.deltaTime / rotationSpeed;
       
        alertTime += Time.deltaTime;

        if (alertTime > alertDuration)
        {
            alertTime = 0;
        }
    }

   private void TrackerAIUpdate()
    {
        noObstacleBetweenDistance = CheckNoObstacleInRay(target.transform, upperBody.position, (target.Body.position - upperBody.position).normalized, obsticalLayerMask);
        howCloseToCenter = CheckTargetHowCloseToCenter(target.Body.position);

        inSight = (howCloseToCenter > 1 - targetFowardDetectRadius) && noObstacleBetweenDistance;

        if(rangeWeapon.Length>0)
        if (rangeWeapon[currentRangeWeaponNum].AttackType == RangeWeapon.RangeAttackType.Hitscan)
        {
            if (targetInsightTime > attackDelay)
            {
                if (howCloseToCenter < 0.999f || !noObstacleBetweenDistance)
                    targetInsightTime = 0;

            }

            if (howCloseToCenter > 0.99f && noObstacleBetweenDistance)
            {
                targetInsightTime += Time.deltaTime;
            }
            else
            {
                targetInsightTime = 0;
            }
        }

        switch (enemyStatus)
        {
            case EnemyStatus.Chasing:
                TrackingTargetPosition();
                lastTargetLocation = aimPos;
                
                    if (rangeWeapon[currentRangeWeaponNum].AttackType == RangeWeapon.RangeAttackType.Projectile)
                {
                    targetInsightTime += Time.deltaTime;
                }
                break;
            case EnemyStatus.Attack:
                TrackingTargetPosition();
                lastTargetLocation = aimPos;
                break;
            case EnemyStatus.Alert:
                break;
        }


        Vector3 impactPoint = /*transform*/gunpoint.position + /*transform*/gunpoint.forward * 100f;
        float laserDistance = 100f;

        RaycastHit hit;
        if (Physics.Raycast(gunpoint.position, /*transform*/gunpoint.forward, out hit, 100, aimLayerMask))
        {
            impactPoint = hit.point;

            laserDistance = hit.distance;
        }


        if (aimSight)
        {
            aimSight.transform.localPosition = new Vector3(0, 0, laserDistance + gunpoint.localPosition.z) * 0.5f;
            aimSight.transform.localScale = new Vector3(aimSight.transform.localScale.x, aimSight.transform.localScale.y, laserDistance + gunpoint.localPosition.z);

            //aimSight.SetPosition(0, transform.position);
            // aimSight.SetPosition(1, impactPoint);
        }

        switch (enemyStatus)
        {
            case EnemyStatus.Idle:
                SentryMode();

                if (inSight || closeContact)
                {
                    seekingPosition = target.GetPrevPosition(trackDelayTick).position;
                    remainTimeToSeekStart = seekDelay;
                    ChangeStatus(EnemyStatus.Chasing);
                }

                break;
            case EnemyStatus.Chasing:
                TrackingTarget();

                if (targetInsightTime >= attackDelay)
                {
                    ChangeStatus(EnemyStatus.Attack);
                    break;
                }

                if (!noObstacleBetweenDistance)
                {
                    seekingPosition = aimPos;
                    ChangeStatus(EnemyStatus.Alert);
                }
                break;
            case EnemyStatus.Attack:


               
                    TrackingTarget();
                    switch (rangeWeapon[currentRangeWeaponNum].AttackType)
                    {
                        case RangeWeapon.RangeAttackType.Hitscan:
                            if (targetInsightTime < attackDelay)
                            {
                                //isTriggerPull = false;
                                ChangeStatus(EnemyStatus.Chasing);
                                break;
                            }
                            break;
                        case RangeWeapon.RangeAttackType.Projectile:
                            if (!noObstacleBetweenDistance)
                            {
                                //isTriggerPull = false;
                                seekingPosition = aimPos;
                                ChangeStatus(EnemyStatus.Alert);
                            }
                            break;
                    }
               

                // fireTime += Time.deltaTime;

                //isTriggerPull = true;
                //AutomaticFire(/*firingLineHit*/);


                break;
            case EnemyStatus.Alert:
                SeekingTarget();
                //TrackingTarget();


                if (inSight || closeContact)
                {
                    alertTime = 0;
                    aimPos = target.GetPrevPosition(trackDelayTick).position;
                    ChangeStatus(EnemyStatus.Chasing);
                    break;
                }

                alertTime += Time.deltaTime;
                if (alertTime > alertDuration)
                {
                    alertTime = 0;
                    ChangeStatus(EnemyStatus.Idle);
                }

                break;
            case EnemyStatus.Eliminated:


                break;
        }
        if (assistCollider)
            assistCollider.transform.position = transform.position;
        closeContact = false;

       // cameraInput = Vector3.ClampMagnitude(cameraInput, 1f);
    }

   

    public override void OnUpdate()
    {
        if (hp <= 0) return;

        if (enemyStatus == EnemyStatus.Attack)
        {
            if(rangeWeapon.Length>0)
                Trigging();
            
            
        }
        else
        {
            isTriggerPull = false;
        }


        base.OnUpdate();
    }

    private void Trigging()
    {
        switch (rangeWeapon[currentRangeWeaponNum].CurrentTriggerType)
        {
            case RangeWeapon.TriggerType.FullAuto:
                isTriggerPull = true;
                break;
            case RangeWeapon.TriggerType.SemiAuto:
                if (chamberLoadTime < rangeWeapon[currentRangeWeaponNum].FireRate)
                {
                    isTriggerPull = false;
                }
                else
                {
                    isTriggerPull = true;
                }
                break;
            case RangeWeapon.TriggerType.Burst:
                if(chamberLoadTime < rangeWeapon[currentRangeWeaponNum].BurstRate)
                {
                    isTriggerPull = false;
                }
                else
                {
                    isTriggerPull = true;
                }
                break;
            case RangeWeapon.TriggerType.ManualAction:
                break;
        }
        
    }
    float throwDelay;
    [SerializeField] float throwDuration=5f;
    private void ThrowEquipment()
    {
        if (!attackable) return;

        if (throwingObject != null)
        {
            desiredThrow = true;
           // throwDelay = 0;
        }
        if (attackDelay > throwDelay)
        {
            throwDelay += Time.deltaTime;
            return;
        }
        else
        {
            attackDelay = throwDuration + 1f;
        }

        if (throwDelay > throwDuration)
        {
            
            
             if (!desiredThrowReady)
            {
                desiredThrowReady = true;//            desiredThrow = true;
                throwDelay = 0;
                return;
            }

            //
        }
        throwDelay += Time.deltaTime;
    }

    public override void ThrowObject()
    {
        desiredThrow = false;

        if (!throwingObject) return;

        throwingObject.transform.parent = null;

        //throwingObject.Body.useGravity = false;
        //throwingObject.Body.isKinematic = true;

        throwingObject.Throw(AIType== EnemyAIType.VeteranGrenadier? aimPos:transform.TransformPoint(Vector3.forward*throwPower*0.33f));

        throwingObject = null;
    }

    private void ChangeStatus(EnemyStatus nextStatus)
    {
        switch (nextStatus)
        {
            case EnemyStatus.Idle:

                if(aimSight) aimSight.material.SetColor("_BaseColor", Color.cyan);
                if(turretLook) turretLook.SetColor(idleColor);
                break;
            case EnemyStatus.Chasing:
                if(aimSight) aimSight.material.SetColor("_BaseColor", Color.red);
                if(turretLook) turretLook.SetColor(chasingColor);
                break;
            case EnemyStatus.Attack:
                if(turretLook) turretLook.SetColor(attackColor);
                break;
            case EnemyStatus.Alert:
                if(aimSight) aimSight.material.SetColor("_BaseColor", new Color(1, 0.7f, 0.5f));
                aimPos = seekingPosition;
                Vector3 seekAngles = ExtendedMathmatics.XYZAngleForwardToTargetPosition(upperBody.forward, aimPos, upperBody.position);
                lockAnglesToSeek = ExtendedMathmatics.ConstrainAngles(seekAngles + upperOrbitAngles, minVerticalAngle, maxVerticalAngle);
                remainTimeToSeekAngle = Mathf.Max(Mathf.Abs(seekAngles.x / trackingTurnAnglePerSec), Mathf.Abs(seekAngles.y / trackingTurnAnglePerSec));
                if(turretLook) turretLook.SetColor(alertColor);
                break;
            case EnemyStatus.Eliminated:
                if(turretLook) turretLook.SetColor(eliminatedColor);
                break;
        }

        enemyStatus = nextStatus;
    }
    enum PredictType { Linear, JumpCurve}
    [SerializeField] PredictType targetPredictType;

    [SerializeField] AnimationCurve predictMultiplierByDistance = AnimationCurve.EaseInOut(0, 1, 40, 1);

        void TrackingTargetPosition()
    {
        
            switch (rangeWeapon[currentRangeWeaponNum].AttackType)
            {
                case RangeWeapon.RangeAttackType.Hitscan:
                    aimPos = Vector3.Lerp(target.transform.position, target.GetPrevPosition(trackDelayTick).position, ExtendedMathmatics.Map(Mathf.Max(target.Body.velocity.magnitude, target.GetPrevPosition(trackDelayTick).velocity.magnitude), 3f, 6, 0, 1));
                    break;
                case RangeWeapon.RangeAttackType.Projectile:
                    float distance = Vector3.Distance(gunpoint.position, target.transform.position) / rangeWeapon[currentRangeWeaponNum].ProjectileSpeed;
                    Vector3 predictPos = aimPos;
                    switch (targetPredictType)
                    {
                        case PredictType.Linear:
                            predictPos = ExtendedMathmatics.PredictPositionLinearly(target.GetPrevPosition(trackDelayTick).position, target.GetPrevPosition(trackDelayTick).velocity, distance);
                            break;
                        case PredictType.JumpCurve:
                            predictPos = ExtendedMathmatics.PredictPositionSimpleGroundCheck(target.GetPrevPosition(trackDelayTick).position, target.GetPrevPosition(trackDelayTick).velocity,
                            CustomGravity.GetGravity(Vector3.zero), distance
                            , terrainLayer, target.GetComponent<CapsuleCollider>(), 0.75f);
                            break;
                    }
                    ExtendedMathmatics.PredictPositionSimpleGroundCheck(target.GetPrevPosition(trackDelayTick).position, target.GetPrevPosition(trackDelayTick).velocity,
                            CustomGravity.GetGravity(Vector3.zero), distance
                            , terrainLayer, target.GetComponent<CapsuleCollider>(), 0.75f);
                    aimPos = Vector3.Lerp(target.GetPrevPosition(trackDelayTick).position, predictPos, leadShotPersentage * predictMultiplierByDistance.Evaluate(Vector3.Distance(upperBody.position, target.GetPrevPosition(trackDelayTick).position)));
                    break;
            }
       
       



    }

    void OnValidate()
    {
    }


    public void SentryMode()
    {
        upperOrbitAngles.x = Mathf.MoveTowards(upperOrbitAngles.x, 0, sentryTurnSpeed * Time.deltaTime);
        upperOrbitAngles.y += sentryTurnSpeed * Time.deltaTime;

        upperOrbitAngles = ExtendedMathmatics.ConstrainAngles(upperOrbitAngles, minVerticalAngle, maxVerticalAngle);
        

    }

    public void TrackingTarget()
    {
        

        turnAngles = ExtendedMathmatics.XYZAngleForwardToTargetPosition(/*transform*/upperBody.forward,aimPos+new Vector3(0,0.2f,0) /*aimSpot.transform.position*/, /*transform*/upperBody.position);
        //turnAngles += upperOrbitAngles;
        // ExtendedMathmatics.ConstrainAngles(turnAngles, minVerticalAngle, maxVerticalAngle);

        //upperOrbitAngles = Vector3.RotateTowards(upperOrbitAngles, turnAngles, trackingTurnAnglePerSec * Time.deltaTime, 1);
        //upperOrbitAngles.x = Mathf.MoveTowards(upperOrbitAngles.x, turnAngles.x, trackingTurnAnglePerSec * Time.deltaTime);
        //upperOrbitAngles.y = Mathf.MoveTowards(upperOrbitAngles.y, turnAngles.y, trackingTurnAnglePerSec * Time.deltaTime);

        //upperOrbitAngles = ExtendedMathmatics.ConstrainAngles(upperOrbitAngles, minVerticalAngle, maxVerticalAngle);

        cameraInput.x = turnAngles.x / Time.deltaTime / rotationSpeed;
        cameraInput.y = turnAngles.y / Time.deltaTime / rotationSpeed;
        //if (isUpperRotationSeperated)
        //{
        //    body.MoveRotation(Quaternion.Euler(new Vector3(0, pevisOrbitAngles.y, 0)));
        //    upperBody.rotation = Quaternion.Euler(upperOrbitAngles);//new Vector3(upperOrbitAngles.x, 0, 0));
        //}
        //else
        //{
        //    body.MoveRotation(Quaternion.Euler(new Vector3(0, upperOrbitAngles.y, 0)));
        //    upperBody.rotation = Quaternion.Euler(upperOrbitAngles);//new Vector3(upperOrbitAngles.x, 0, 0));

        //}


    }

    //private void OnDrawGizmos()
    //{
    //    Ray ray = new Ray(throwPoint.position, transform.forward);
    //    Gizmos.DrawLine(throwPoint.position, ray.GetPoint(dist));
    //}

    float dist;

    public void CalculateThrowAngle()
    {
        aimPos = ExtendedMathmatics.PredictPositionLinearly ( target.GetPrevPosition(0).position, target.GetPrevPosition(0).velocity*1.2f, 2f);

        turnAngles.y = ExtendedMathmatics.XYZAngleForwardToTargetPosition(/*transform*/transform.forward, target.GetPrevPosition(0).position /*aimSpot.transform.position*/, /*transform*/upperBody.position).y;
        turnAngles.y = Mathf.Abs(turnAngles.y) > 2f ? turnAngles.y : 0;
        float horizontalDistance = Vector3.Distance(Vector3.zero, transform.InverseTransformPoint(new Vector3(aimPos.x, 0, aimPos.z)));
        dist = horizontalDistance;
        float throwAngle = 0;//ExtendedMathmatics.AngleOfLaunch(horizontalDistance, aimPos.y - throwPoint.position.y, throwPower);
       // Debug.Log(throwAngle);
        turnAngles.x = throwAngle - upperOrbitAngles.x;
        
        cameraInput.x = turnAngles.x / Time.deltaTime / rotationSpeed;
        cameraInput.y = turnAngles.y / Time.deltaTime / rotationSpeed;

       

    }

    [SerializeField] LineRenderer[] lines;
    public override void InitWeapon(RangeWeapon pickup)
    {
        base.InitWeapon(pickup);

        if (rangeAttackEffect.Length > 0)
        {
                rangeAttackEffect[0].SetLineRenderers(lines);
            
        }
    }

    [SerializeField] TurretAim following;
    [SerializeField] float triggingDistance;

    [SerializeField] Vector2 activationHeight = new Vector2(-0.5f, 2f);

    public override void FireRangeWeapon()
    {
        for (int i = 0; i < rangeWeapon[currentRangeWeaponNum].shells.Length; ++i)
        {
            Ray shellTrajectory = trajectory;
            if (rangeWeapon[currentRangeWeaponNum].ShellDirection(i) != Vector3.zero)
            {
                shellTrajectory.direction = (shellTrajectory.direction + gunpoint.TransformDirection(rangeWeapon[currentRangeWeaponNum].ShellDirection(i))).normalized;
            }

            switch (rangeWeapon[currentRangeWeaponNum].AttackType)
            {
                case RangeWeapon.RangeAttackType.Hitscan:
                    // firingLineBlink = firingLineBlink ? false : true;
                    //    aimSight.material.SetColor("_BaseColor", firingLineBlink ? Color.yellow : Color.white);
                    RaycastHit hit;

                    if (Physics.Raycast(shellTrajectory, out hit, rangeWeapon[currentRangeWeaponNum].MaximumAttackRange, aimLayerMask))
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
                        rangeAttackEffect[currentRangeWeaponNum].PlayShootOneOfLine(shellTrajectory.origin, hit.point, i);
                    }
                    else
                    {
                        rangeAttackEffect[currentRangeWeaponNum].PlayShootOneOfLine(shellTrajectory.origin, shellTrajectory.origin + shellTrajectory.direction * rangeWeapon[currentRangeWeaponNum].MaximumAttackRange,i);
                    }



                    break;
                case RangeWeapon.RangeAttackType.Projectile:
                    bullets[firedBullet].gameObject.SetActive(true);
                    bullets[firedBullet].Fire(shellTrajectory.origin, shellTrajectory.direction * rangeWeapon[currentRangeWeaponNum].ProjectileSpeed, this);
                    break;
                case RangeWeapon.RangeAttackType.DirectionalRanged:

                    int count = Physics.OverlapSphereNonAlloc(directionalRangeAttackArea[currentRangeWeaponNum].bounds.center, directionalRangeAttackArea[currentRangeWeaponNum].bounds.extents.magnitude, neighbours, enemyLayerMask);
                    // List<ActionBaseComponent> dealedEnemies = new List<ActionBaseComponent>();

                    Physics.autoSyncTransforms = true;

                    for (int j = 0; j < count; ++j)
                    {
                        var collider = neighbours[j];

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
                                tgt = collider.transform.GetComponent<ActionBaseComponent>();
                                //  if (!dealedEnemies.Exists(x => tgt))
                                {
                                    if (Physics.Raycast(shellTrajectory.origin, (tgt.transform.position - shellTrajectory.origin).normalized, out hit, rangeWeapon[currentRangeWeaponNum].MaximumAttackRange, aimLayerMask))
                                    {
                                        if (hit.transform.GetComponent<ActionBaseComponent>())
                                        {
                                            /* float damage = rangeWeapon[currentRangeWeaponNum].AttackDamage * rangeWeapon[currentRangeWeaponNum].AttackEffectByDistance.Evaluate(hit.distance/ rangeWeapon[currentRangeWeaponNum].MaximumAttackRange)
                                             * rangeWeapon[currentRangeWeaponNum].AttackEffectByCenterDistance.Evaluate(Vector3.Dot(shellTrajectory.direction, (hit.point - shellTrajectory.origin).normalized));
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
                                        if (Physics.Raycast(shellTrajectory.origin, (tgt.transform.position - shellTrajectory.origin).normalized, out hit, rangeWeapon[currentRangeWeaponNum].MaximumAttackRange, aimLayerMask))
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

                    rangeAttackEffect[currentRangeWeaponNum].PlayShootAnimation(shellTrajectory.origin, shellTrajectory.origin + shellTrajectory.direction, rangeWeapon[currentRangeWeaponNum].AttackType);
                    break;
            }
            firedBullet = firedBullet >= bullets.Length - 1 ? 0 : firedBullet + 1;
        }

        // rangeWeapon[currentRangeWeaponNum].PlayShootAnimation(trajectory.origin, trajectory.origin + trajectory.direction * rangeWeapon[currentRangeWeaponNum].MaximumAttackRange);

        if (OnSound != null)
            ActionBaseComponent.OnSound(transform.position, gunShotSoundRange);

        chamberLoadTime = 0;
        // return true;
    }

    public void SeekingTarget()
    {
        if (remainTimeToSeekStart > 0)
        {
            remainTimeToSeekStart -= Time.deltaTime;
            return;
        }

        if (remainTimeToSeekAngle > 0)
        {
            turnAngles = ExtendedMathmatics.XYZAngleForwardToTargetPosition(upperBody.forward, aimPos, upperBody.position);

            cameraInput.x = turnAngles.x / Time.deltaTime / rotationSpeed;
            cameraInput.y = turnAngles.y / Time.deltaTime / rotationSpeed;
            //turnAngles += upperOrbitAngles;
            //upperOrbitAngles.x = Mathf.MoveTowards(upperOrbitAngles.x, turnAngles.x, trackingTurnAnglePerSec * Time.deltaTime);
            //upperOrbitAngles.y = Mathf.MoveTowards(upperOrbitAngles.y, turnAngles.y, trackingTurnAnglePerSec * Time.deltaTime);

            //upperOrbitAngles = ExtendedMathmatics.ConstrainAngles(upperOrbitAngles, minVerticalAngle, maxVerticalAngle);
            //if (isUpperRotationSeperated)
            //{
            //    body.MoveRotation(Quaternion.Euler(new Vector3(0, pevisOrbitAngles.y, 0)));
            //    upperBody.rotation = Quaternion.Euler(upperOrbitAngles);//new Vector3(upperOrbitAngles.x, 0, 0));
            //}
            //else
            //{
            //    body.MoveRotation(Quaternion.Euler(new Vector3(0, upperOrbitAngles.y, 0)));
            //    upperBody.localRotation = Quaternion.Euler(new Vector3(upperOrbitAngles.x, 0, 0));//new Vector3(upperOrbitAngles.x, 0, 0));

            //}

            remainTimeToSeekAngle -= Time.deltaTime;
        }
        else
        {
            float time = alertTime / alertDuration;

            Vector3 progressAngle =  new Vector3(seekingCurveVertical.Evaluate(time) * seekRange.x, seekingCurveHorizontal.Evaluate(time) * seekRange.y, 0) + lockAnglesToSeek /*+ upperOrbitAngles*/;//+ anglesToLastTgtLoc;

            cameraInput.x = (progressAngle.x - upperOrbitAngles.x) / Time.deltaTime / rotationSpeed;
            cameraInput.y = (progressAngle.y - upperOrbitAngles.y) / Time.deltaTime / rotationSpeed;
            alertTime += Time.deltaTime;
            //upperOrbitAngles = ExtendedMathmatics.ConstrainAngles(progressAngle, minVerticalAngle, maxVerticalAngle);
            //if (isUpperRotationSeperated)
            //{
            //    body.MoveRotation(Quaternion.Euler(new Vector3(0, pevisOrbitAngles.y, 0)));
            //    upperBody.rotation = Quaternion.Euler(upperOrbitAngles);
            //}
            //else
            //{
            //    body.MoveRotation(Quaternion.Euler(new Vector3(0, upperOrbitAngles.y, 0)));
            //    upperBody.localRotation = Quaternion.Euler(new Vector3(upperOrbitAngles.x, 0, 0));

            //}
        }

    }

    //public void AutomaticFire()
    //{
    //    //        if (!base.AutomaticFire()) { return false; };

    //    chamberLoadTime += Time.deltaTime;
    //    if(chamberLoadTime < rangeWeapon[currentRangeWeaponNum].FireRate)
    //    {
    //        return;
    //    }

    //    switch (rangeWeapon[currentRangeWeaponNum].AttackType)
    //    {
    //        case RangeWeapon.RangeAttackType.Hitscan:
    //            firingLineBlink = firingLineBlink ? false : true;
    //            if(aimSight) aimSight.material.SetColor("_BaseColor", firingLineBlink ? Color.yellow : Color.white);

    //            if (Physics.Raycast(gunpoint.position,/*transform*/upperBody.forward,out firingLineHit,100f,aimLayerMask))
    //            {
    //                //inFiringLine = true;
    //                if ( firingLineHit.transform == target.transform)
    //                {
    //                    if (firingLineHit.transform == target.transform)
    //                    {
    //                    //    DoSlowmotion();
    //                        target.Damage(/*damagePerSec*/rangeWeapon[currentRangeWeaponNum].AttackDamage, transform.position);
                        
    //                    }
    //                }
    //                base.PunchHole(firingLineHit.point , firingLineHit.normal, firingLineHit.transform == target.transform?target.transform:null);
    //                /*if (bulletHole != null)
    //                {
    //                    Transform hole = Instantiate(bulletHole, firingLineHit.point + firingLineHit.normal * 0.0001f, Quaternion.LookRotation(hit.normal, Vector3.up), firingLineHit.transform==target.transform? target.transform:null);
    //                 }*/

    //            }
    //            break;
    //        case RangeWeapon.RangeAttackType.Projectile:
    //            bullets[firedBullet].gameObject.SetActive(true);
    //            bullets[firedBullet].Fire(gunpoint.position,/*transform*/upperBody.forward * rangeWeapon[currentRangeWeaponNum].ProjectileSpeed, this);
    //            break;
    //    }

    //    firedBullet = firedBullet>=bullets.Length-1?0:firedBullet+1;

    //    chamberLoadTime = 0;
    ////    return true;
    //}

    public bool CheckTargetInSight(Vector3 targetPosition)
    {
        Vector3 targetDir = (targetPosition - upperBody.position).normalized;
        float dot = Vector3.Dot(upperBody.forward, targetDir);
        if (dot> 1 - targetFowardDetectRadius)
        {
            return true;
        }
        return false;
    }


    public float CheckTargetHowCloseToCenter(Vector3 targetPosition)
    {
        Vector3 targetDir = (targetPosition - /*transform*/upperBody.position).normalized;
        return Vector3.Dot(/*transform*/upperBody.forward, targetDir);
        
    }

    public bool CheckNoObstacleInRay(Transform target, Vector3 startPos,Vector3 rayDir, LayerMask mask)
    {
        RaycastHit rayHit;
        if (Physics.Raycast(startPos, /*targetDir*/rayDir, out rayHit, targetDetectRange, mask))
        {
            if (target == rayHit.transform)//rayHit.transform.GetComponent<MovingSphere>())
            { return true; }
        }
        return false;
    }

    public bool CheckNoObstacleInRay(Transform target, Vector3 startPos, Vector3 rayDir, out RaycastHit hit, LayerMask mask)
    {
        RaycastHit rayHit;
        if (Physics.Raycast(startPos, /*targetDir*/rayDir, out rayHit, targetDetectRange, aimLayerMask))
        {
            hit = rayHit;
            if (target == rayHit.transform)//rayHit.transform.GetComponent<MovingSphere>())
            { return true; }
        }
        hit = rayHit;
        return false;
    }
   

    private void OnCollisionStay(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        { closeContact = true; }
    }



}