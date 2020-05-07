using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public ActionBaseComponent[] spheres;

    public TurretAim[] turrets;

    public ActionBaseComponent[] actors;
    [SerializeField] bool manualActorSelect;

    public OrbitCamera orbitCamera;
    public Camera arisaCam;

    public Throwable[] throwableArray;

    public List<Throwable> throwableList;

    public Animator arisaAnim;
    public /*SkinnedMeshRenderer*/PerObjectMaterialProperties arisaMat;
    public TypingAnimation explainer;
    public Transform outroCameraGimbal;

    public int GetMyIndexInActors(ActionBaseComponent myActor)
    {
        for(int i = 0; i < actors.Length; ++i)
        {
            if (actors[i] == myActor)
                return i;
        }
        return -1;
    }
    private void Awake()
    {
        if (instance)
            Destroy(this.gameObject);
        else
        {
            instance = this;
        }
        
        
        if(!manualActorSelect)
        actors = FindObjectsOfType(typeof(ActionBaseComponent)) as ActionBaseComponent[];

        if (!manualActorSelect)
            turrets = FindObjectsOfType(typeof(TurretAim)) as TurretAim[];

        throwableList = new List<Throwable>();
        //foreach (Throwable t in throwableList)
        //{
        //    t.OnAwake();
        //}

        foreach(ActionBaseComponent a in actors)
        {
            a.OnAwake();
        }

        //   foreach (ActionBaseComponent s in spheres)
        //   {
        //       s.OnAwake();
        //   }
        //foreach(TurretAim t in turrets)
        //   {
        //       t.OnAwake();
        //   }
        if (orbitCamera != null)
            orbitCamera.enabled = true;//OnAwake();

        //if (!intro)
        //{
        //    director.playOnAwake = true;
        //}

    }

    // Start is called before the first frame update
    void Start()
    {
        //if (intro)
        //{
        //    director.playableAsset = intro;
        //    director.Play();
        //}

        arisaMat.BaseMapOffset = new Vector4(0.5f, 0.5f, 0, 0.5f); //sharedMaterials[0].SetTextureOffset("_BaseMap", new Vector2(1, 0));

        foreach (TurretAim t in turrets)
        {
            t.OnStart();
        }
       // StartCoroutine("AfterFixedUpdate");

    }

    // Update is called once per frame
    void Update()
    {
        //arisaAnim.transform.position = new Vector3(0, 0, 41.52171f);
        //if (Input.GetKeyDown(KeyCode.Escape)) SceneLoader.instance.ReloadCurrentScene();//UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        /* if (Input.GetKeyDown(KeyCode.Alpha2)) UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) UnityEngine.SceneManagement.SceneManager.LoadScene(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) UnityEngine.SceneManagement.SceneManager.LoadScene(3);*/

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Cursor.visible = Cursor.visible ? false : true;
            Cursor.lockState = Cursor.lockState == CursorLockMode.None ? CursorLockMode.Locked : CursorLockMode.None;
        }

        if (!orbitCamera.enabled) return;

        foreach (Throwable t in throwableList)
        {
            t.OnUpdate();
        }
        foreach(ActionBaseComponent a in actors)
        {
            a.OnUpdate();
        }
        //        foreach (ActionBaseComponent s in spheres)
        //        {
        //            s.OnUpdate();
        //        }
        //        foreach (TurretAim t in turrets)
        //        {
        //            t.OnUpdate();
        ////            t.OnLateUpdate();
        //            //s.UpdateRotation();
        //        }


         //   if (orbitCamera != null)
        //       orbitCamera.OnUpdate();

        ArisaAnimationUpdate();
    }

    [SerializeField]
    float closeDistance = 100f;
    [SerializeField]
    float handAnimDistance = 100f;
    [SerializeField]
    float farDistance = 120f;

    private void ArisaAnimationUpdate()
    {
        float currentDistance = (spheres[0].transform.position - arisaAnim.transform.position).sqrMagnitude;

        if (currentDistance < handAnimDistance)
        {
            arisaAnim.SetBool("isClosed", true);
        }
        else if(currentDistance>farDistance)
        {
            arisaAnim.SetBool("isClosed", false);
        }

        //if (currentDistance < closeDistance&& !isPhase2Exist)
        //{
        //    float width = Mathf.Lerp(.7f, 1, ExtendedMathmatics.Map(currentDistance, 30, closeDistance,1, 0));
        //    orbitCamera.RegularCamera.rect = new Rect(0, 0, width, 1);
        //    arisaCam.rect = new Rect(width, 0, 1 - width, 1);

        //}
        
       
    }

    private void OnEnable()
    {

        arisaMat.BaseMapOffset = new Vector4(0.5f, 0.5f, 0, 0.5f); //sharedMaterials[0].SetTextureOffset("_BaseMap", new Vector2(1, 0));
        ActionBaseComponent.GameOverCall += ArisaWorrying;

        StartCoroutine("TurnOffLifeUI");
    }

  //  public GameObject LifeObj;

    IEnumerator TurnOffLifeUI()
    {
        yield return new WaitForSeconds(0.1f);

        Debug.Log("TurnOff");
         spheres[0].GetComponent<JetpackActor>().TurnOffLifeUI();
       // player.lifeUI[3].gameObject.SetActive(false);
        //player.lifeUI[4].gameObject.SetActive(false);
      //  player.lifeUI[5].gameObject.SetActive(false);

        //if(LifeObj!=null)
        //LifeObj.SetActive(false);
    }

    private void OnDisable()
    {

       // arisaMat.sharedMaterials[0].SetTextureOffset("_BaseMap", new Vector2(1, 0));
        ActionBaseComponent.GameOverCall -= ArisaWorrying;
    }
    private bool isLose;
    private void ArisaWorrying()
    {
        if(!isLose)
        {
            isLose = true;
            ac.TalkSound(loseSound);
            explainer.MoveRectPosition(new Vector2(48, -194), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
            explainer.context = "しまった…\nバックスペースキーで\nやり直し";
            explainer.PanelOpen(true);
            arisaAnim.SetTrigger("Lose");
        }
        
    }

    private void FixedUpdate()
    {
        if (!orbitCamera.enabled) return;

            foreach (Throwable t in throwableList)
        {
            t.OnFixedUpdate();
        }
        foreach(ActionBaseComponent a in actors)
        {
            a.OnFixedUpdate();
        }
        foreach (ActionBaseComponent a in actors)
        {
            a.LastFixedUpdate();
        }

        //foreach (ActionBaseComponent t in turrets)
        //    {
        //        t.OnFixedUpdate();
        //    }

        //foreach (ActionBaseComponent s in spheres)
        //{
        //    s.OnFixedUpdate();
        //}

        //foreach (ActionBaseComponent t in turrets)
        //{
        //    t.LastFixedUpdate();
        //}

        //foreach (ActionBaseComponent s in spheres)
        //{
        //    s.LastFixedUpdate();
        //}

        foreach (Throwable t in throwableList)
        {
            t.LastFixedUpdate();
        }
      //  if (orbitCamera != null)
      //      orbitCamera.OnLateUpdate();

    }


    IEnumerator OutroCameraTransit()
    {
        float t = 0;
        while (t<1)
        {
            orbitCamera.transform.position = Vector3.Lerp(orbitCamera.transform.position, outroCameraGimbal.position, t);
            orbitCamera.transform.rotation = Quaternion.Lerp(orbitCamera.transform.rotation, outroCameraGimbal.rotation, t);

            yield return null;
            t += Time.deltaTime;
        }

        orbitCamera.transform.parent = outroCameraGimbal;
        orbitCamera.transform.localPosition = Vector3.zero;
        orbitCamera.transform.localRotation = Quaternion.identity;
    }

    Vector3 gameCameraPos;
    Quaternion gameCameraRot;

    public void CallReturningCameraTransit()
    {
        StartCoroutine("ReturningGameCameraTransit");
    }

    IEnumerator ReturningGameCameraTransit()
    {
        float t = 1;
        while (t > 0)
        {
            orbitCamera.transform.position = Vector3.Lerp(gameCameraPos, outroCameraGimbal.position, t);
            orbitCamera.transform.rotation = Quaternion.Lerp(gameCameraRot, outroCameraGimbal.rotation, t);

            yield return null;
            t -= Time.deltaTime;
        }

        orbitCamera.transform.parent = outroCameraGimbal.parent;
        orbitCamera.enabled = true;
        spheres[0].enabled = true;
        isPhase2Exist = false;
        //orbitCamera.transform.localPosition = Vector3.zero;
        //orbitCamera.transform.localRotation = Quaternion.identity;
    }

    public PlayableDirector director;
    public PlayableAsset intro;
    public PlayableAsset phase2Anim;
    public PlayableAsset outro;

   // public GameObject[] phase1Objs;
   // public GameObject[] phase2Objs;

    public AudioController ac;
    public string loseSound;
    [SerializeField] bool isPhase2Exist;
    public void Win()
    {
        if (!isPhase2Exist)
        { 
            //ac.PlaySound(winSound);
            spheres[0].GetComponent<JetpackActor>().TurnOffLifeUI();
            spheres[0].gameObject.SetActive(false);
            orbitCamera.enabled = false;
            orbitCamera.transform.LookAt(arisaAnim.transform.position + new Vector3(0, 1.2f, 0));
            arisaAnim.SetTrigger("Win");
            arisaAnim.GetComponent<ArisaAnimator>().target = orbitCamera.transform;
            arisaMat.UpdatePropertiesOnTick(true);
            arisaMat.BaseMapOffset = new Vector4(0.5f, 0.5f, 0.5f, 0.5f); //sharedMaterials[0].SetTextureOffset("_BaseMap", new Vector2(0,0));
            director.playableAsset = outro;
            director.Play();

            SceneLoader.instance.ProceedNextScene = true;
            StartCoroutine("OutroCameraTransit");
            //this.enabled = false;
        }
        else
        {
            //for(int i =0; i < phase1Objs.Length; ++i)
            //{
            //    phase1Objs[i].SetActive(false);
            //}
            //for (int i = 0; i < phase2Objs.Length; ++i)
            //{
            //    phase1Objs[i].SetActive(true);
            //}
            director.playableAsset = phase2Anim;
            director.Play();

            spheres[0].enabled = false;
            gameCameraPos = orbitCamera.transform.position;
            gameCameraRot = orbitCamera.transform.rotation;

            StartCoroutine("OutroCameraTransit");
        }
    }

    //private void LateUpdate()
    //{
      
    //    //foreach (TurretAim t in turrets)
    //    //{
    //    //    t.OnLateUpdate();
    //    //}

    //    foreach(MovingSphere s in spheres)
    //    {
    //        s.OnLateUpdate();
    //    }

    ////    orbitCamera.OnLateUpdate();
    //}
    
	
}
