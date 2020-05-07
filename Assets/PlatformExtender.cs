using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformExtender : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()//IEnumerator Awake()
    {
        //while (SceneLoader.instance == null)
        //{
        //    yield return null;
        //}
        float scaleMultiplier = 1;
        //if (SceneLoader.instance)
        {
          if(SceneLoader.difficulty== Difficulty.Easy)//  if (SceneLoader.currentDifficulty() == Difficulty.Easy)
            {
                scaleMultiplier = 1.5f;
            }
        }


        transform.localScale = new Vector3(transform.localScale.x * scaleMultiplier, transform.localScale.y,  transform.localScale.z * (GetComponent<Animator>()!=null ? 1.1f:scaleMultiplier));
    }

}
