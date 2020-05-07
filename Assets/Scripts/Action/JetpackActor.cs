using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR||UNITY_STANDALONE
using XInputDotNetPure;
#endif
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.UI;

public class JetpackActor : ActionBaseComponent
{
    public OrbitCamera orbitCam;


    [SerializeField, Range(0f, 100f)]
    protected float /*maxSteepSpeed = 8f,*/ maxFallSpeedWhenSteep = 1f;

    public Transform[] hitDetector;
    private int hitNum;

    public struct HitInfo
    {
        public Transform icon;
        public Vector3 attackerPos;
        public float remainTime;
    }
    private HitInfo[] hitList;
    private int currentHitNum;

    //  public bool IsAiming { get { return /*!aimStop*/desiredAim; } }

    bool noObstacleBetweenCamNTgt, noObstacleBetweenCharNTgt;
    RaycastHit camRayHit, characterRayHit;

    [SerializeField]
    LayerMask aimAssistLayerMask = -1;


    //[SerializeField, Range(0, 1)]
    //float aimAssistStrengthMin = 0.0f, aimAssistStrengthMax = 0.3f, aimAssistRangeMin = 0.15f, aimAssistRangeMax = 0.3f, aimAssistClamp = 0.98f;
    //[SerializeField]
    //float aimAssistRangeRadiusIdle = 0.5f, aimAssistRangeRadiusExpanded = 1f, aimAssistRangeHeightIdle = 0.5f, aimAssistRangeHeightExpanded = 1.2f;

    //[SerializeField]
    //CapsuleCollider[] assistCapsules;
    [SerializeField]
    Slider[] aimAssistDebugers;


    private float rightVibration;
    private float leftVibration;

    public Image[] lifeUI;

    public void TurnOffLifeUI()
    {
        lifeUI[0].transform.parent.gameObject.SetActive(false);
        lifeUI[1].transform.parent.gameObject.SetActive(false);
        lifeUI[2].transform.parent.gameObject.SetActive(false);
    }

    //public UnityEngine.UI.Slider lifeUI;

    Renderer debugger;
    int colorId = Shader.PropertyToID("_BaseColor");

    public AudioClip jumpSound;

    //private void ShowDebugger()
    //{
    //    debugger.material.SetColor(colorId, OnGround ? new Color(0, 0, 0, 0.5f) : OnSteep ? (lastWallBounceNormal == firstWallBounceNormal ? new Color(1, 0.92f, 0.016f, 0.5f) : new Color(0, 0, 1, 0.5f))
    //        : jumpPhase < 2 ? new Color(1, 1, 1, 0.5f) : new Color(0.5f, 0.5f, 0.5f, 0.5f));
    //}

    public void PlaySound(AudioSource source)
    {
        source.Stop();
        source.Play();
    }
    public void ControlVolume(AudioSource source, float value)
    {
        source.volume = value;
    }

    public float Hp { get { return hp; } set { hp = value; } }

    private float prevHp;
    public Animator canvasAnim;

    [SerializeField] AudioClip[] damagedSound;
    public override void Damage(float value, Vector3 attackerPos)
    {
        if (!isLive)
            return;

        float damageMultiplier = 1;

        if (SceneLoader.instance)
        {
            if (SceneLoader.currentDifficulty() == Difficulty.Easy)
            {
                damageMultiplier *= 0.5f;
            }
        }

        prevHp = hp;

        base.Damage(value* damageMultiplier, attackerPos);
        if(canvasAnim)
        if(!canvasAnim.GetCurrentAnimatorStateInfo(0).IsName("Damaged Loop"))
                canvasAnim.SetTrigger("Damage");

        Sound(damagedSound[value>0.2f?1:0]);

        leftVibration = Mathf.Min(1, leftVibration + value * 3f);
        rightVibration = Mathf.Min(1, rightVibration + value * 3f);
        lifeUI[0].transform.parent.gameObject.SetActive(true);
        lifeUI[1].transform.parent.gameObject.SetActive(true);
        lifeUI[2].transform.parent.gameObject.SetActive(true);

        lastDamageTakenTime = 0;

        if (hitList.Length > 0)
        {
            hitList[currentHitNum].attackerPos = attackerPos; hitList[currentHitNum].remainTime = 3f;
            hitList[currentHitNum].icon.gameObject.SetActive(true);
            currentHitNum = currentHitNum >= hitList.Length - 1 ? 0 : currentHitNum + 1;
        }
    }

    void HitInfoUpdate()
    {
        for (int i = 0; i < hitList.Length; ++i)
        {
            if (hitList[i].remainTime <= 0)
            {
                hitList[i].icon.gameObject.SetActive(false);
                continue;
            }
            hitList[i].remainTime -= Time.deltaTime;

            hitList[i].icon.rotation = Quaternion.LookRotation((hitList[i].attackerPos - transform.position).normalized, Vector3.up);


        }
    }

   // public delegate void GameOver();
   // public static GameOver GameOverCall;

    public override void Death()
    {
        body.constraints = RigidbodyConstraints.None;
        body.useGravity = true;
        body.AddForce(Vector3.forward);

#if UNITY_EDITOR || UNITY_STANDALONE
        GamePad.SetVibration(0, 0, 0);
#endif
        if (lifeUI != null)
        {
            lifeUI[0].fillAmount = 0;
            lifeUI[1].fillAmount = 0;
            lifeUI[2].fillAmount = 0;
            //lifeUI.value = 0;
        } 
        GetComponent<CapsuleCollider>().material = null;
        if(debugger)
        debugger.material.SetColor(colorId, Color.red);

        isLive = false;
        OnJump -= JumpVibration;
        if (GameOverCall != null)
            GameOverCall();

    }

    void JumpVibration()
    {
        leftVibration += 0.2f;
    }

    private void Awake()
    {
       
        OnValidate();
    }

    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    public override void OnAwake()
    {
        base.OnAwake();
        isLive = true;

        //lifeUI[0].transform.parent.gameObject.SetActive(false);
        //lifeUI[1].transform.parent.gameObject.SetActive(false);
        //lifeUI[2].transform.parent.gameObject.SetActive(false);

        OnJump += JumpVibration;
       // remainJet = jetMaxDuration;
        if (bullets.Length > 0)
        {
            bullets[0].transform.parent.transform.parent = null;
        }

        // aimStop = true;
        hitList = new HitInfo[hitDetector.Length];//new List<HitInfo>();
        for (int i = 0; i < hitList.Length; ++i)
        {
            hitList[i].icon = hitDetector[i];
        }

        OnValidate();

        debugger = GetComponent<Renderer>();

#if UNITY_ANDROID
        orbitCam.transform.parent.parent.rotation = Quaternion.Euler(90, 90, 0);
        
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = true;
        }
#endif



    }
    [SerializeField] PhysicMaterial physicMat;
    private void OnEnable()
    {

        body.useGravity = false;

        if (physicMat)
            movementCollider.sharedMaterial = physicMat;
        //body.isKinematic = true;
    }

    private void OnDisable()
    {
        body.useGravity = true;

        movementCollider.sharedMaterial = null;

        //body.isKinematic = false;
    }

    private void OnDestroy()
    {
        if (isLive)
        { OnJump -= JumpVibration; }
#if UNITY_EDITOR||UNITY_STANDALONE
        GamePad.SetVibration(0, 0, 0);
#endif
    }

    //bool desireToHook;

    public override void UpdateInput()
    {
        playerInput.x = CrossPlatformInputManager.GetAxis("Horizontal");//Input.GetAxis("Horizontal");
        playerInput.y = CrossPlatformInputManager.GetAxis("Vertical");//Input.GetAxis("Vertical");
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

      /*  cameraInput = new Vector3(
             CrossPlatformInputManager.GetAxis("Vertical Camera"),
           CrossPlatformInputManager.GetAxis("Horizontal Camera")
             , 0
         );*/

#if UNITY_ANDROID
        gyroRotation = Input.gyro.attitude;
        //cameraInput = new Vector3(
        //    Input.gyro.attitude.eulerAngles.x  ,
        //    Input.gyro.attitude.eulerAngles.y  ,
        //    Input.gyro.attitude.eulerAngles.z
        // );
#endif

        desiredJump |= CrossPlatformInputManager.GetButtonDown("Jump");//Input.GetButtonDown("Jump");
        //desiredJet = CrossPlatformInputManager.GetButton("Jump");

       // aimAxis = CrossPlatformInputManager.GetAxis("Aim");
       // desiredAim = aimAxis >= 0.3f ? true : false;

       // isTriggerPull = (CrossPlatformInputManager.GetAxis("Fire") >= 0.3f ? true : false) || CrossPlatformInputManager.GetButton("FireButton");

       // desiredThrowReady |= CrossPlatformInputManager.GetButtonDown("Throw");
      //  desiredThrow |= CrossPlatformInputManager.GetButtonUp("Throw");

       // desiredPickupWeapon |= CrossPlatformInputManager.GetButtonDown("Interaction");
       // desiredChangeWeapon |= CrossPlatformInputManager.GetButtonDown("ChangeWeapon");

       // desiredSwap |= CrossPlatformInputManager.GetButtonDown("Swap");

       
        //desiredEvade |= CrossPlatformInputManager.GetButtonDown("Evade");

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            desiredChangeWeaponTo = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            desiredChangeWeaponTo = 2;
        }

    }

    public override void OnUpdate()
    {
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    aimAssistDebugers[0].transform.parent.gameObject.SetActive(aimAssistDebugers[0].transform.parent.gameObject.activeInHierarchy ? false : true);
        //}

        if (hp <= 0)
        {
            return;
        }

        //UpdateInput();

        //HitInfoUpdate();

        //ShowDebugger();

        disableProbeTime = Mathf.Max(disableProbeTime - Time.deltaTime, 0);

        //if (jetpackUI)
        //{
        //    float remain = remainJet / jetMaxDuration;
        //    if (remain > 0.99)
        //    {
        //        jetpackUI.enabled = false;
        //    }
        //    else
        //    {
        //        jetpackUI.enabled = true;
        //        jetpackUI.value = remain;
        //    }
            
        //}

        base.OnUpdate();
    }

    //void FocusCurrentTargetAssist()
    //{
    //    foreach (CapsuleCollider c in assistCapsules)
    //    {
    //        if (c.transform == target)
    //        {
    //            c.radius = aimAssistRangeRadiusExpanded;
    //            c.height = aimAssistRangeHeightExpanded;
    //        }
    //        else
    //        { c.enabled = false; }
    //    }
    //}

    //void ResetAssistColliders()
    //{
    //    if (assistCapsules.Length <= 0)
    //    {
    //        return;
    //    }

    //    foreach (CapsuleCollider c in assistCapsules)
    //    {
    //        c.enabled = true;
    //        c.radius = aimAssistRangeRadiusIdle;
    //        c.height = aimAssistRangeHeightIdle;
    //    }
    ////}

    //void AimAssist(Ray camRay)
    //{
    //    RaycastHit assistHit;


    //    if (Physics.Raycast(camRay, out assistHit, 100f, aimAssistLayerMask))
    //    {
    //        if (assistHit.transform.CompareTag("Assist"))
    //        {
    //            if (target != assistHit.transform)
    //            {
    //                isAquiredNow = false;
    //                lastTargetPosition = Vector3.zero;
    //            }
    //            target = assistHit.transform;
    //            FocusCurrentTargetAssist();
    //            if (!isAquiredNow && lastTargetPosition == Vector3.zero)
    //            {
    //                TargetAcquired();
    //            }
    //        }
    //        else
    //        {
    //            ResetAssistColliders();
    //            target = null;
    //            lastTargetPosition = Vector3.zero;
    //        }
    //    }
    //    else
    //    {
    //        ResetAssistColliders();
    //        target = null;
    //        lastTargetPosition = Vector3.zero;
    //    }

    //}

    public override void UpdateTrajectory()
    {
        if (!noObstacleBetweenCharNTgt)
        {
            trajectory = new Ray(gunpoint.position, (characterRayHit.point - gunpoint.position).normalized);
            for (int i = 0; i < directionalRangeAttackArea.Length; ++i)
            {
                if (directionalRangeAttackArea[i] == null)
                    continue;
                directionalRangeAttackArea[i].transform.rotation = Quaternion.LookRotation((characterRayHit.point - gunpoint.position).normalized, Vector3.up);
            }
            return;
        }
        if (noObstacleBetweenCamNTgt)
        {
            trajectory = new Ray(gunpoint.position, (camRayHit.point - gunpoint.position).normalized);
            for (int i = 0; i < directionalRangeAttackArea.Length; ++i)
            {
                if (directionalRangeAttackArea[i] == null)
                    continue;
                directionalRangeAttackArea[i].transform.rotation = Quaternion.LookRotation((camRayHit.point - gunpoint.position).normalized, Vector3.up);
            }
            return;
        }
        for (int i = 0; i < directionalRangeAttackArea.Length; ++i)
        {
            if (directionalRangeAttackArea[i] == null)
                continue;
            directionalRangeAttackArea[i].transform.rotation = Quaternion.LookRotation(gunpoint.forward, Vector3.up);
        }
        base.UpdateTrajectory();
    }

    public UnityEngine.UI.Text speedmeter;


    Vector3 lastEdgePoint = Vector3.zero;
    Vector3 lastEdgeNormal = Vector3.up;

    public Transform box;

    public override void FirstFixedUpdate()
    {

        DesireVelocity();

        Vector3 gravity = CustomGravity.GetGravity(body.position, out upAxis);

        base.UpdateState();
        ShoulderSwapBySteepNormal(steepNormal);
        AdjustVelocity();


        if (desiredJump)
        {
            desiredJump = false;
            Jump(gravity);
        }
        if (desiredEvade)
        {
            SuperJump();
            desiredEvade = false;
        }

        ApplyGravity(gravity);

        base.ApplyTotalVelocity();//        ApplyTotalVelocity();
    }

    public override void OnFixedUpdate()
    {
        if (hp <= 0)
            return;

        

        FirstFixedUpdate();
        if (speedmeter)
        {
            Vector3 checkSpeed = body.velocity;
            checkSpeed.y = 0;
            speedmeter.text = checkSpeed.magnitude.ToString("N3");
        }

       

        //Ray camRay = new Ray(orbitCam.transform.position + -orbitCam.transform.forward * orbitCam.transform.localPosition.z, orbitCam.transform.forward);
        //noObstacleBetweenCamNTgt = Physics.Raycast(camRay, out camRayHit, 100f, aimLayerMask);
        //if (noObstacleBetweenCamNTgt)
        //{
        //    noObstacleBetweenCharNTgt = Physics.Raycast(gunpoint.position, (camRayHit.point - gunpoint.position).normalized, out characterRayHit, 100f, aimLayerMask);

        //}



        //UpdateTrajectory();

        //if (desiredSwap)
        //{
        //    SwapShoulder();
        //}

     //   AimAssist(camRay);

        //if (!desiredAim)
        //{
        //    aimTime = 0;
        //}
        //else
        //{
        //    aimTime += Time.deltaTime;
        //}

        UpdateRotation();

        if (desiredChangeWeapon)
            SwapWeapon();

        if (desiredChangeWeaponTo > 0)
        {
            SwapWeapon(desiredChangeWeaponTo - 1);
        }

        if (desiredPickupWeapon)
        {
            desiredPickupWeapon = false;
            if (nearByWeapon)
                PickupWeapon(nearByWeapon);
        }

       

        Attack();

        leftVibration = Mathf.Max(leftVibration - Time.deltaTime, 0);
        rightVibration = Mathf.Max(rightVibration - Time.deltaTime, 0);


#if UNITY_EDITOR || UNITY_STANDALONE
        GamePad.SetVibration(0, leftVibration, rightVibration);
#endif
        // UpdateHP();

       


        //base.LastFixedUpdate();    
    }

    public override void UpdateHP()
    {
        for (int i = 0; i < complexDamaged.Length; ++i)
        {
            if (complexDamaged[i].index > 0)
            {
                Vector3 pos;
                float damage = complexDamaged[i].FeedbackMaxDamage(out pos);
                Damage(damage, pos);
            }
        }

        if (hp < 0)
        {
            Death();
            return;
        }
        if (hp < 0.99f)
        {
            if (lastDamageTakenTime > recoveryDelay)
            {
               
                hp = Mathf.Min(hp + recoverHealthPerSec * Time.deltaTime, 1);
                if (lifeUI.Length > 0)
                {
                    //lifeUI.value = hp;

                    lifeUI[0].fillAmount = ExtendedMathmatics.Map(hp, 0.667f, 1, 0, 1);
                    lifeUI[1].fillAmount = ExtendedMathmatics.Map(hp, 0.333f, 0.667f, 0, 1);
                    lifeUI[2].fillAmount = ExtendedMathmatics.Map(hp, 0, 0.333f, 0, 1);
                    lifeUI[3].fillAmount = ExtendedMathmatics.Map(hp, 0.667f, 1, 0, 1);
                    lifeUI[4].fillAmount = ExtendedMathmatics.Map(hp, 0.333f, 0.667f, 0, 1);
                    lifeUI[5].fillAmount = ExtendedMathmatics.Map(hp, 0, 0.333f, 0, 1);

                    //lifeUI.gameObject.SetActive(hp > 0.99f ? false : true);
                    if (hp > 0.99f)
                    {
                        lifeUI[0].transform.parent.gameObject.SetActive(false);
                        lifeUI[1].transform.parent.gameObject.SetActive(false);
                        lifeUI[2].transform.parent.gameObject.SetActive(false);
                        
                    }

                }
            }
            else
            {
                lastDamageTakenTime += Time.deltaTime;

                float displayValue = Mathf.Lerp(prevHp, hp, lastDamageTakenTime);
                lifeUI[0].fillAmount = ExtendedMathmatics.Map(hp, 0.667f, 1, 0, 1);
                lifeUI[1].fillAmount = ExtendedMathmatics.Map(hp, 0.333f, 0.667f, 0, 1);
                lifeUI[2].fillAmount = ExtendedMathmatics.Map(hp, 0, 0.333f, 0, 1);
                lifeUI[3].fillAmount = ExtendedMathmatics.Map(displayValue, 0.667f, 1, 0, 1);
                lifeUI[4].fillAmount = ExtendedMathmatics.Map(displayValue, 0.333f, 0.667f, 0, 1);
                lifeUI[5].fillAmount = ExtendedMathmatics.Map(displayValue, 0, 0.333f, 0, 1);
            }
        }
        
    }

    Vector3 syncCrosshairAnglesWhenSwapShoulder = Vector3.zero;
    [SerializeField] float shoulderSwapTurnAngleRate = 0.12f;
    [SerializeField] float shoulderSwapTurnConstraitDistance = 2f;

    private void FocusCenterWhileSwapShoulder()
    {
        if (noObstacleBetweenCharNTgt)
        {
            if ((transform.position - characterRayHit.point).sqrMagnitude > shoulderSwapTurnConstraitDistance)
            {
                syncCrosshairAnglesWhenSwapShoulder = ExtendedMathmatics.XYZAngleBetweenTwoDirection((orbitCam.transform.position - characterRayHit.point).normalized,
                    (upperBody.TransformPoint(new Vector3(-orbitCam.transform.localPosition.x,
                    orbitCam.transform.localPosition.y, orbitCam.transform.localPosition.z))
                    - characterRayHit.point).normalized);

                if (Mathf.Abs(syncCrosshairAnglesWhenSwapShoulder.y) <= 45f)
                    syncCrosshairAnglesWhenSwapShoulder.z = shoulderSwapTurnAngleRate;
            }
            return;
        }
        if (noObstacleBetweenCamNTgt)
        {
            if ((transform.position - orbitCam.transform.position).sqrMagnitude > shoulderSwapTurnConstraitDistance)
            {
                syncCrosshairAnglesWhenSwapShoulder = ExtendedMathmatics.XYZAngleBetweenTwoDirection((orbitCam.transform.position - camRayHit.point).normalized,
                     (upperBody.TransformPoint(new Vector3(-orbitCam.transform.localPosition.x,
                     orbitCam.transform.localPosition.y, orbitCam.transform.localPosition.z))
                     - characterRayHit.point).normalized);

                if (Mathf.Abs(syncCrosshairAnglesWhenSwapShoulder.y) <= 45f)
                    syncCrosshairAnglesWhenSwapShoulder.z = shoulderSwapTurnAngleRate;
            }
        }
    }

    public override void SwapShoulder()
    {
        base.SwapShoulder();

        FocusCenterWhileSwapShoulder();
    }

    public override void ShoulderSwapBySteepNormal(Vector3 normal)
    {
        float dot = Vector3.Dot(normal, transform.right);
        float frontDot = Vector3.Dot(normal, transform.forward);
        if (dot > 0.5)
        {
            if (CurrentShoulderStatus == ShoulderStatus.Left)
            {
                SwapShoulder(ShoulderStatus.Right);
                if (frontDot > 0.1f)
                {
                    FocusCenterWhileSwapShoulder();
                }
            }
        }
        else if (dot < -0.5)
        {
            if (CurrentShoulderStatus == ShoulderStatus.Right)
            {
                SwapShoulder(ShoulderStatus.Left);
                if (frontDot > 0.1f)
                {
                    FocusCenterWhileSwapShoulder();
                }
            }
        }




    }

    public override void SwapShoulder(ShoulderStatus changeShoulder)
    {
        base.SwapShoulder(changeShoulder);

        //if (noObstacleBetweenCharNTgt)
        //{
        //    syncCrosshairAnglesWhenSwapShoulder = ExtendedMathmatics.XYZAngleBetweenTwoDirection((orbitCam.transform.position - characterRayHit.point).normalized,
        //        (upperBody.TransformPoint(new Vector3(Mathf.Abs(orbitCam.transform.localPosition.x)*(changeShoulder== ShoulderStatus.Left?-1:1),
        //        orbitCam.transform.localPosition.y, orbitCam.transform.localPosition.z))
        //        - characterRayHit.point).normalized);

        //    syncCrosshairAnglesWhenSwapShoulder.z = 1f;
        //    return;
        //}
        //if (noObstacleBetweenCamNTgt)
        //{
        //    syncCrosshairAnglesWhenSwapShoulder = ExtendedMathmatics.XYZAngleBetweenTwoDirection((orbitCam.transform.position - camRayHit.point).normalized,
        //           (upperBody.TransformPoint(new Vector3(Mathf.Abs(orbitCam.transform.localPosition.x) * (changeShoulder == ShoulderStatus.Left ? -1 : 1),
        //           orbitCam.transform.localPosition.y, orbitCam.transform.localPosition.z))
        //           - characterRayHit.point).normalized);

        //    syncCrosshairAnglesWhenSwapShoulder.z = 1f;
        //}
    }

    public override void FireRangeWeapon()
    {
        base.FireRangeWeapon();
        rightVibration = Mathf.Max(rightVibration, 0.5f);


    }

    public override void SpreadFire(Vector3 hitPoint, float hitDistance, ActionBaseComponent tgt)
    {
        float damage = rangeWeapon[currentRangeWeaponNum].AttackDamage * rangeWeapon[currentRangeWeaponNum].AttackEffectByDistance.Evaluate(hitDistance / rangeWeapon[currentRangeWeaponNum].MaximumAttackRange)
     * rangeWeapon[currentRangeWeaponNum].AttackEffectByCenterDistance.Evaluate(Vector3.Dot(orbitCam.transform.forward, (hitPoint - orbitCam.transform.position).normalized));
        tgt.DealComplexDamage(myIndexNumInGMActorList, damage, transform.position);
    }


    public static bool RangeCheck(float inputValue, float max, float min)
    {
        if (inputValue < max && inputValue > min)
        {
            return true;
        }
        return false;
    }

    //public Transform gyroscope;
    private Quaternion gyroRotation;

    public RectTransform swapButton;

    float rotatedAngle;

    public override void UpdateRotation()
    {
       // base.UpdateRotation();

#if UNITY_ANDROID

        orbitCam.transform.parent.parent.position = transform.position;
        orbitCam.transform.parent.localRotation = gyroRotation* new Quaternion(0, 0, 1, 0);
        orbitCam.transform.parent.parent.eulerAngles += new Vector3(0,cameraInput.y*rotationSpeed*Time.deltaTime,0)
            + Vector3.Lerp(Vector3.zero, new Vector3(0, syncCrosshairAnglesWhenSwapShoulder.y, 0), syncCrosshairAnglesWhenSwapShoulder.z);//: Vector3.zero);
        syncCrosshairAnglesWhenSwapShoulder.z = Mathf.Lerp(syncCrosshairAnglesWhenSwapShoulder.z, 0, 0.1f);

        swapButton.position = orbitCam.RegularCamera.WorldToScreenPoint(transform.position);

        transform.rotation = Quaternion.LookRotation(Vector3.Cross(Vector3.up, Vector3.Cross(Vector3.up, orbitCam.transform.parent.forward)));
        //        transform.rotation. += Quaternion.Euler(0, cameraInput.y * rotationSpeed, 0);
        upperBody.rotation = Quaternion.LookRotation(orbitCam.transform.forward,Vector3.up);//Quaternion.LookRotation(Vector3.Cross(Vector3.up, -orbitCam.transform.parent.right), orbitCam.transform.up);
        //transform.localRotation = Quaternion.Euler(new Vector3(0, cameraInput.x, 0));
        //orbitCam.transform.localRotation = Quaternion.Euler(new Vector3(cameraInput.y, 0, cameraInput.z+90f));
        return;
#endif
        //if (orbitCam)
        //{
        //    Vector3 turnAngles = Vector3.zero;
        //    Vector3 correctAngle = Vector3.zero;
        //    if (target != null)
        //    {
        //        correctAngle = TargetTrackingAngles();
        //    }
        //    const float e = 0.0001f;
        //    if (cameraInput.x < -e || cameraInput.x > e || cameraInput.y < -e || cameraInput.y > e)
        //    {
        //        if (aimAssistDebugers.Length > 0)
        //        {
        //            aimAssistDebugers[8].value = cameraInput.y;
        //            aimAssistDebugers[9].value = -cameraInput.x;
        //        }
        //        if (target != null && !isAquiredNow)
        //        {
        //            Vector2 correctAngle01 = new Vector2(correctAngle.x / Time.deltaTime / orbitCam.RotationSpeed / (IsAiming ? rotationSpeedMultiplyWhenZoom : 1), -correctAngle.y / Time.deltaTime / orbitCam.RotationSpeed / (IsAiming ? rotationSpeedMultiplyWhenZoom : 1));

        //            float accurate = ExtendedMathmatics.Map(correctAngle01.magnitude, aimAssistStrengthMin, aimAssistStrengthMax, aimAssistRangeMin, aimAssistRangeMax);//Mathf.Lerp(aimAssistRangeMin, aimAssistRangeMax, ExtendedMathmatics.Map(correctAngle01.magnitude, aimAssistStrengthMin, aimAssistStrengthMax, 0, 1));

        //            Vector2 correctRangeX = new Vector2(correctAngle01.x - accurate, correctAngle01.x + accurate);
        //            Vector2 correctRangeY = new Vector2(correctAngle01.y - accurate, correctAngle01.y + accurate);
        //            if (aimAssistDebugers.Length > 0)
        //            {
        //                aimAssistDebugers[0].value = Mathf.Max(0, correctRangeY.y);
        //                aimAssistDebugers[1].value = Mathf.Max(0, correctRangeY.x);
        //                aimAssistDebugers[3].value = Mathf.Min(0, correctRangeY.y) * -1;
        //                aimAssistDebugers[2].value = Mathf.Min(0, correctRangeY.x) * -1;
        //                aimAssistDebugers[4].value = Mathf.Max(0, correctRangeX.y);
        //                aimAssistDebugers[5].value = Mathf.Max(0, correctRangeX.x);
        //                aimAssistDebugers[7].value = Mathf.Min(0, correctRangeX.y) * -1;
        //                aimAssistDebugers[6].value = Mathf.Min(0, correctRangeX.x) * -1;
        //            }
        //            float xValue;

        //            if (cameraInput.x > correctAngle01.x)
        //                xValue = ExtendedMathmatics.Map(cameraInput.x, correctRangeX.y, 1, 0, 1);
        //            else
        //                xValue = ExtendedMathmatics.Map(cameraInput.x, correctRangeX.x, -1, 0, 1);

        //            float yValue;

        //            if (cameraInput.y > correctAngle01.y)
        //                yValue = ExtendedMathmatics.Map(cameraInput.y, correctRangeY.y, 1, 0, 1);
        //            else
        //                yValue = ExtendedMathmatics.Map(cameraInput.y, correctRangeY.x, -1, 0, 1);

        //            xValue = Mathf.Clamp01(xValue);
        //            yValue = Mathf.Clamp01(yValue);
        //            //  Debug.Log("X is " + xValue + " / Y is " + yValue);
        //            float xInput = Mathf.Lerp(-correctAngle01.x, cameraInput.x, xValue);
        //            float yInput = Mathf.Lerp(correctAngle01.y, cameraInput.y, yValue);

        //            turnAngles = orbitCam.RotationSpeed * Time.deltaTime *
        //              new Vector2(xInput, yInput)
        //                * (IsAiming ? rotationSpeedMultiplyWhenZoom : 1);
        //            // syncCrosshairAnglesWhenSwapShoulder.z = Mathf.Max(0, syncCrosshairAnglesWhenSwapShoulder.z - Time.deltaTime);
        //        }
        //        else
        //        {
        //            if (aimAssistDebugers.Length > 0)
        //            {
        //                aimAssistDebugers[1].value = 0;
        //                aimAssistDebugers[3].value = 0;
        //                aimAssistDebugers[5].value = 0;
        //                aimAssistDebugers[7].value = 0;
        //                aimAssistDebugers[0].value = 0;
        //                aimAssistDebugers[2].value = 0;
        //                aimAssistDebugers[4].value = 0;
        //                aimAssistDebugers[6].value = 0;
        //            }

        //            turnAngles = orbitCam.RotationSpeed * Time.deltaTime *
        //                cameraInput
        //                * (IsAiming ? rotationSpeedMultiplyWhenZoom : 1); //(syncCrosshairAnglesWhenSwapShoulder.z > 1f ?

        //            // syncCrosshairAnglesWhenSwapShoulder.z = Mathf.Max(0, syncCrosshairAnglesWhenSwapShoulder.z- Time.deltaTime);

        //        }
        //        upperOrbitAngles += turnAngles;
        //        isAquiredNow = false;
        //    }

        //    upperOrbitAngles += Vector3.Lerp(Vector3.zero, new Vector3(0, syncCrosshairAnglesWhenSwapShoulder.y, 0), syncCrosshairAnglesWhenSwapShoulder.z);//: Vector3.zero);
        //    syncCrosshairAnglesWhenSwapShoulder.z = Mathf.Lerp(syncCrosshairAnglesWhenSwapShoulder.z, 0, 0.1f);

        //    upperOrbitAngles = ExtendedMathmatics.ConstrainAngles(upperOrbitAngles, orbitCam.MinVerticalAngle, orbitCam.MaxVerticalAngle);
        //    upperBody.localRotation = Quaternion.Euler(new Vector3(upperOrbitAngles.x, 0, 0));
        //    body.MoveRotation(Quaternion.Euler(new Vector3(0, upperOrbitAngles.y, 0)));
        //}


    }


    Transform target;
    Vector3 lastPlayerPosition;
    Vector3 lastTargetPosition;
    Vector3 acquireAngles;
    bool isAquiredNow;

    void TargetAcquired()
    {
        isAquiredNow = true;
        lastTargetPosition = target.position;
        lastPlayerPosition = transform.position;
    }

    Vector3 TargetTrackingAngles()
    {
        Vector3 correction = ExtendedMathmatics.XYZAngleBetweenTwoDirection((target.position - transform.position).normalized,
            (lastTargetPosition - lastPlayerPosition).normalized);

        lastTargetPosition = target.position;
        lastPlayerPosition = transform.position;

        return correction;
    }


    //Special Moves for JatpackActor

    //[SerializeField] float jetUpMaxSpeed = 4f;
    //[SerializeField] float jetUpAcceleration = 10f;
    //[SerializeField] float jetMaxDuration = 3f;
    //float remainJet;
    //[SerializeField] float jetRechargeRatePerSec = 1f;
    //[SerializeField] float jetRechargeDelay = 0.5f;
    //[SerializeField] float requireMinimumCharge = 1f;
    //[SerializeField] Slider jetpackUI;
    //float lastJetTime=0;
    //bool desiredJet;
    //bool isJetDrain;

    // private float reachedMaximumSpeed;

    public override void DesireVelocity()
    {
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

        //float speed;//=maxSpeed;
        //Vector3 horizontalVelocity = new Vector3(body.velocity.x, 0, body.velocity.z);
        //float horizontalMagnitude = horizontalVelocity.magnitude;
        //if ((OnGround && stepsSinceLastAir > bunnyHopAllowFrame)/*||!hook.IsSteep*/)
        //{
        //    speed =  maxSpeed;
        //   // reachedMaximumSpeed = 0;
        //}
        //else
        //{
        //    speed = Mathf.Max(reachedMaximumSpeed, maxSpeed);
        //    //reachedMaximumSpeed = Mathf.Min(reachedMaximumSpeed, horizontalMagnitude);
        //}
        //reachedMaximumSpeed = horizontalMagnitude;
        // speed = Mathf.Max(horizontalMagnitude, maxSpeed);

        desiredVelocity =
              new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        // maximumDesiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * speed;
    }



    public override void AdjustVelocity()
    {
        Vector3 xAxis = ProjectDirectionOnPlane(rightAxis, contactNormal);
        Vector3 zAxis = ProjectDirectionOnPlane(forwardAxis, contactNormal);

        Vector3 relativeVelocity = velocity - connectionVelocity;

        float currentX = Vector3.Dot(/*velocity*/relativeVelocity, xAxis);
        float currentZ = Vector3.Dot(/*velocity*/relativeVelocity, zAxis);

        float acceleration;
        if (OnGround && stepsSinceLastAir > bunnyHopAllowFrame)
        {
            acceleration = maxAcceleration;

            //  reachedMaximumSpeed = 0;
        }
        else
        {
            acceleration = maxAirAcceleration;// * playerInput.magnitude;
        }


        float maxSpeedChange = acceleration * Time.deltaTime;




        float newX =
            Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);// *sleepMultiler;
        float newZ =
            Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);// * sleepMultiler;


        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);


    }
    
    public override void Jump(Vector3 gravity)
    {
        Vector3 jumpDirection;

        if (OnGround)
        {
            jumpDirection = contactNormal;
        }
        else if (OnSteep)   //Wall Kick
        {
            if (firstWallBounceNormal == lastWallBounceNormal && maxAirJumps <= jumpPhase)
                return;

            jumpDirection =  steepNormal;
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

        Sound(jumpSound);

        stepsSinceLastJump = 0;
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

    [SerializeField] float desentSpeed = 2f;
    public override void ApplyGravity(Vector3 gravity)
    {
        //if (desiredJet&&!isJetDrain){
        //    float maxSpeedChange = jetUpAcceleration * Time.deltaTime;

        //    float newY =
        //        Mathf.MoveTowards(velocity.y, jetUpMaxSpeed, maxSpeedChange);// *sleepMultiler;

        //    velocity.y += (newY - velocity.y);

        //    lastJetTime = 0;
        //    remainJet -= Time.deltaTime;

        //    if (remainJet <= 0)
        //    {
        //        isJetDrain = true;
        //    }
        //}


        //if (OnSteep && !OnGround && onSteepTime < SteepDuration)
        //{

        //    onSteepTime += Time.deltaTime;

        //    Vector3 localVelocity = playerInputSpace.InverseTransformVector(velocity);
        //    Vector3 deltaGravity = playerInputSpace.InverseTransformVector(gravity) * Time.deltaTime;

        //    localVelocity = new Vector3(localVelocity.x + deltaGravity.x, localVelocity.y + deltaGravity.y/*Mathf.Max(localVelocity.y+deltaGravity.y,-maxFallSpeedWhenSteep)*/, localVelocity.z + deltaGravity.z);

        //    Vector3 output = playerInputSpace.TransformVector(localVelocity);

        //    velocity = output;


        //    velocity.y = Mathf.Max(velocity.y, -maxFallSpeedWhenSteep);
        //}

               // else
        {
            //lastJetTime += Time.deltaTime;
            //if (lastJetTime > jetRechargeDelay)
            //{
            //    remainJet = Mathf.Min(remainJet + jetRechargeRatePerSec * Time.deltaTime,jetMaxDuration);

            //    if (remainJet > requireMinimumCharge)
            //    {
            //        isJetDrain = false;
            //    }
            //}

            velocity += gravity * Time.deltaTime;
        }
    }

    //private float grappleTime;
    [SerializeField] float moveForceWhenGrappled = 2f;

    //public override void ApplyTotalVelocity()
    //{
        
    //    { body.velocity = velocity; }// +grappledVelocity;
    //}

    public override void ClearState()
    {

        if (CheckWallDetach())
        {
            onSteepTime = 0;
            //jumpPhase = 0;
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
   // bool manualThrowHook;
    public override void ThrowReady()
    {
     
        base.ThrowReady();
    }

    public override void ThrowObject()
    {
       

        base.ThrowObject();
    }
    //[SerializeField] float requireJetToSuperjump=1.2f;
    [SerializeField] float superjumpPower = 12f;
    void SuperJump()
    {
       
            velocity.y = superjumpPower;
        
    }
    [SerializeField] AudioClip springSound;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            GamePad.SetVibration(0, 0, 0);
#endif
            GameManager.instance.Win();
        }
        if (other.CompareTag("Trampoline"))
        {
            Sound(springSound);
            desiredEvade=true;
           // SuperJump();
        }
    }

}
