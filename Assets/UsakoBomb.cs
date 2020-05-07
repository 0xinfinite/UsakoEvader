using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsakoBomb : Throwable
{
    float aeroProgress;
    [SerializeField] float flyingDuration = 2f;
    [SerializeField] GameObject blastObj;
    [SerializeField] AnimationCurve aeroCurve = new AnimationCurve() { keys = new Keyframe[3] { new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0) } };

    private Vector3 initPos;
    private Vector3 targetPos;

    [SerializeField]float aeroHeight;

    public override void OnAwake()
    {
        base.OnAwake();

        if (SceneLoader.instance)
        {
            if (SceneLoader.currentDifficulty() == Difficulty.Normal)
            {
                flyingDuration = 1;
            }
        }

        multiplier = 1 / flyingDuration;
    }

    public override void Throw(Vector3 targetPosition)
    {
        time = 0;
        blastObj.SetActive(false);
        blastObj.transform.parent = this.transform;
        blastObj.transform.localPosition = Vector3.zero;

        initPos = owner.ThrowPoint;

        targetPos = targetPosition;
        transform.position = owner.ThrowPoint;
        Body.isKinematic = false;
        Body.useGravity = false;
        //base.Throw(velocity);
    }
    float time;
    float multiplier;
    public override void OnFixedUpdate()
    {
        if (time > 1)
        {
            //Body.isKinematic = false;
            Body.useGravity = true;
            base.OnFixedUpdate();
            return;
        }

        Vector3 pos = Vector3.Lerp(initPos, targetPos, time);
        pos.y += aeroCurve.Evaluate(time);
        transform.position = pos;
        time += multiplier * Time.deltaTime;

        

    }

    [SerializeField] float saftyTime = 0.5f;

    private void OnCollisionStay(Collision collision)
    {
        if (time < saftyTime)
        {
            return;
        }

        blastObj.transform.parent = null;
        blastObj.SetActive(true);
        transform.parent = owner.transform;
        transform.position = owner.ThrowPoint;
        gameObject.SetActive(false);
        
    }
}
