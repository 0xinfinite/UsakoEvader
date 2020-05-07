using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleSceneManager : MonoBehaviour
{

    public Animator animator;

    private int currentButton;

    [SerializeField] int maxButtonCount;

    private void Awake()
    {

#if UNITY_EDITOR
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
#else
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
#endif
    }

    // Start is called before the first frame update
    void Start()
    {

        if (!animator)
            animator = GetComponent<Animator>();

        ResumeGame();
    }

    private float axisPushedTime=0;
    [SerializeField] float keepPushDuration = 0.5f;

    [SerializeField] bool isGameScene;
    bool isPause;

    bool pushedInvoke;
    bool pushedLeft;
    bool pushedRight;

    private void LateUpdate()
    {
        animator.ResetTrigger("Pause");
        animator.ResetTrigger("Right");
        animator.ResetTrigger("Left");
        animator.ResetTrigger("InvokeWindow");
    }

    // Update is called once per frame
    void Update()
    {
        //animator.ResetTrigger("Right");
        //animator.ResetTrigger("Left");
        //animator.ResetTrigger("InvokeWindow");

        if (SceneLoader.instance.ProceedNextScene)
            return;


        //Time.timeScale = isPause ? 0 : 1;
        //if (isGameScene)
        //{
        //    if (!isPause)
        //    {
        //        if (Input.GetButtonDown("Submit"))
        //        {
        //            Debug.Log("Pausing");
        //            animator.SetTrigger("Pause");
        //            isPause = isPause ? false : true;

        //        }

        //        return;
        //    }
        //}
        if (!isGameScene || (isGameScene && isPause))
        {
            if (Input.GetAxis("Vertical") < -0.05f || Input.GetKeyDown(KeyCode.DownArrow)||Input.GetKeyDown(KeyCode.S))
            {
                if (currentButton + 1 < maxButtonCount && axisPushedTime <= 0)
                {
                    currentButton++;
                    animator.SetInteger("ButtonState", currentButton);
                }
                axisPushedTime += Time.unscaledDeltaTime;//Time.deltaTime;

                if (axisPushedTime > keepPushDuration)
                {
                    Debug.Log("Keep Pushed");
                    axisPushedTime = 0;
                }
            }
            else if (Input.GetAxis("Vertical") > 0.05f || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                if (currentButton - 1 >= 0 && axisPushedTime <= 0)
                {
                    currentButton--;
                    animator.SetInteger("ButtonState", currentButton);
                }
                axisPushedTime += Time.unscaledDeltaTime;//+= Time.deltaTime;

                if (axisPushedTime > keepPushDuration)
                {
                    Debug.Log("Keep Pushed");
                    axisPushedTime = 0;
                }
            }
            else
            {
                axisPushedTime = 0;
            }
        }

     //   if (!isWinScene)
       // {
            if (Input.GetAxis("Horizontal") > 0.05f || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
            animator.SetTrigger("Right");//animator.SetBool("IsRight", true);
            }
            else if (Input.GetAxis("Horizontal") < -0.05f || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
            animator.SetTrigger("Left");//animator.SetBool("IsRight", false);
            }
        //  }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isButtonReady)
            {
                animator.SetTrigger("Skip");
                isButtonReady = true;
                return;
            }

            if (isPause)
                animator.SetTrigger("Escape");
            else
                animator.SetTrigger("Pause");
            return;
        }

        if (Input.GetButtonDown("Submit"))
        {
            if (!isButtonReady)
            {
                animator.SetTrigger("Skip");
                isButtonReady = true;
             //   return;
            }

            if (!isPause)
            {
                animator.SetTrigger("Pause");
            }
            else
            {
                animator.SetTrigger("Escape");
            }
        }

        if (/*Input.GetButtonDown("Jump")|| */Input.GetButtonDown("Enter"))
        {
            

            if (!isButtonReady)
            {
                animator.SetTrigger("Skip");
                isButtonReady = true;
             //   return;
            }

            if (!isPause)
            {
                animator.SetTrigger("Pause");
            }

            //  if (!pushedInvoke)
            //    {
            animator.SetTrigger("InvokeWindow");
           //     pushedInvoke = true;
       //     }
            //if (!isWinScene)
            //{
            //    if (animator.GetBool("IsRight"))
            //    {
            //        animator.SetTrigger("InvokeWindow");
            //        return;
            //    }
            //}

            //switch (currentButton)
            //{
            //    case 0:
            //        if (!isWinScene)
            //        {
            //            BGMManager.instance.ControlVolume(0, 0.5f);
            //            SceneLoader.SetDifficulty(Difficulty.Normal);
            //            SceneLoader.instance.LoadNextScene();
            //        }
            //        else
            //        {
            //            BGMManager.instance.ControlVolume(0, 0.5f);
            //            SceneLoader.instance.LoadTargetScene(0);
            //        }
            //        break;
            //    case 1:
            //        if (!isWinScene)
            //        {
            //            BGMManager.instance.ControlVolume(0, 0.5f);
            //            SceneLoader.SetDifficulty(Difficulty.Easy);
            //            SceneLoader.instance.LoadNextScene();   //temp
            //        }
            //        else
            //        {
            //            animator.SetTrigger("InvokeWindow");
            //        }
            //        break;
            //    case 2:
            //        Application.Quit(); //Temp
            //        break;
            //}
        }

         if ( Input.GetButtonDown("Jump"))
        {
            if(isGameScene&& !isPause )
            {
                return;
            }
            //if (!pushedInvoke)
            //{
                animator.SetTrigger("InvokeWindow");
           //     pushedInvoke = true;
           // }

        }
        //else
        //{
        //    pushedInvoke = false;
        //}
    }

    bool isButtonReady;
    public void ButtonsReady()
    {
        isButtonReady = true;
    }

    public void PauseGame()
    {
        isPause = true;
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        isPause = false;
        Time.timeScale = 1;
    }

    public void StartNormalMode()
    {
        BGMManager.instance.ControlVolume(0, 0.5f);
                    SceneLoader.SetDifficulty(Difficulty.Normal);
                    SceneLoader.instance.LoadNextScene();
    }

    public void StartEasyMode()
    {
        BGMManager.instance.ControlVolume(0, 0.5f);
        SceneLoader.SetDifficulty(Difficulty.Easy);
        SceneLoader.instance.LoadNextScene();
    }

    public void RestartStageMoreEasier()
    {
        SceneLoader.SetDifficulty(Difficulty.Easy);
        SceneLoader.instance.ReloadCurrentScene();
    }

    public void ReturnToTitle()
    {
        BGMManager.instance.ControlVolume(0, 0.5f);
        SceneLoader.instance.LoadTargetScene(0);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
