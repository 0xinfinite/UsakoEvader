using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField]
    float damage = 0.1f;

    [SerializeField]
    bool damagePerSec;

    [SerializeField]
    float damageTime = 0.3f;

    [SerializeField]
    float allowMaintainDuration = 3f;

    [SerializeField]
    AnimationCurve sizeAnim = new AnimationCurve() { keys = new Keyframe[3] { new Keyframe(0,0), new Keyframe(0.5f,1), new Keyframe(1,0) } };

    private void Awake()
    {
        multiplier = 1 / allowMaintainDuration;
    }

    

    float time;
    float multiplier;
    [SerializeField]
    Transform origParent;
    private void Update()
    {
        if (allowMaintainDuration == Mathf.Infinity)
        {
            return;
        }

        if (time > allowMaintainDuration)
        {
            transform.parent = origParent;
            time = 0;
            gameObject.SetActive(false);
        }

        float sizeValue = sizeAnim.Evaluate(time);
        transform.localScale = new Vector3(sizeValue, sizeValue, sizeValue);

        time += Time.deltaTime*multiplier;
    }

    private void OnTriggerStay(Collider other)
    {
        if (damagePerSec)
        {
            ActionBaseComponent actor = other.GetComponent<ActionBaseComponent>();

            if (actor != null)
            {
                actor.Damage(damage*Time.deltaTime, transform.position);
                actor.ForcingJumpPhase = 2;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!damagePerSec)
        {
            ActionBaseComponent actor = other.GetComponent<ActionBaseComponent>();

            if (actor!=null){
                actor.Damage(damage, transform.position);
                actor.ForcingJumpPhase = 2;
            }
        }
    }
}
