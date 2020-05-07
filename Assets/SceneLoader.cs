using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Difficulty { Easy = 0, Normal}

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance;

    private Animator anim;

    bool canProceedNext;
    public bool ProceedNextScene { get { return canProceedNext; } set { canProceedNext = value; } }

    public bool willChangeBGMOnNextScene;

    public static Difficulty difficulty;
    public static void SetDifficulty(Difficulty dff)
    {
        difficulty = dff;
    }

    public static Difficulty currentDifficulty()
    {
        return difficulty;
    }

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        anim = GetComponent<Animator>();
    }

    IEnumerator Start()
    {
        while (!BGMManager.instance)
        {
            yield return null;
        }
        BGMManager.instance.ControlVolume(0.5f, 0.4f);

        if (lowerDiffExplain != null)
        {
            if(difficulty == Difficulty.Easy)
            {
                lowerDiffExplain.text = "既に易い難易度になっています。\n「はい」ボタンを押せばステージをやり直します。";
            }
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Restart")) SceneLoader.instance.ReloadCurrentScene();

        if (canProceedNext)if(Input.GetButtonDown("Enter")||Input.GetButtonDown("Submit")) SceneLoader.instance.LoadNextScene();
    }

    AsyncOperation async;

    bool isLoading;

    public void ReloadCurrentScene()
    {
        if (isLoading) return;

        isLoading = true;

        anim.Play("NextScene");
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex));
    }

    public void LoadNextScene()
    {
        if (isLoading) return;

        isLoading = true;

        anim.Play("NextScene");
        if (willChangeBGMOnNextScene)
        {
            BGMManager.instance.ControlVolume(0, 0.4f);
        }
        StartCoroutine(LoadScene(SceneManager.GetActiveScene().buildIndex+1>=SceneManager.sceneCountInBuildSettings?0: SceneManager.GetActiveScene().buildIndex + 1));
    }

    public void LoadTargetScene(int sceneIndex)
    {
        if (isLoading) return;

        isLoading = true;

        anim.Play("NextScene");
        StartCoroutine(LoadScene(sceneIndex));
    }

    public void ReadyQueueFromSlide()
    {
        async.allowSceneActivation = true;
    }

    IEnumerator LoadScene(int sceneNum)
    {
        async = SceneManager.LoadSceneAsync(sceneNum);
        async.allowSceneActivation = false;
        while (!async.isDone)
        {
            yield return null;
        }
    }

    public UnityEngine.UI.Text lowerDiffExplain;
}
