using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Throwable : MonoBehaviour
{
    public ActionBaseComponent owner;

    public enum AddVelocityType { None=0, AllDirection, OnlyFrontal}

    Rigidbody body;
    public Rigidbody Body { get { return body; } }

    int bounceCount;

    [SerializeField]
    int distractAllowBounceCount = 2;

    public float bounceSoundRange = 8f;

    public bool enableCustomGravity;

    private void OnValidate()
    {
        if (distractAllowBounceCount <= 0)
            distractAllowBounceCount = 1;
    }

    public virtual void Throw(Vector3 velocity)
    {
        Body.velocity = velocity;
    }
    
    private void Awake()
    {
        if(GameManager.instance)
        GameManager.instance.throwableList.Add(this);
        OnAwake();
    }

    public virtual void OnAwake()
    {
        OnValidate();
        if (enableCustomGravity)
        {
            body.useGravity = false;
        }
        body = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        OnEnabled();
    }

    public virtual void OnEnabled()
    {

    }

    void OnDisable()
    {
        OnDisabled();   
    }

    public virtual void OnDisabled()
    {

    }

    private void OnDestroy()
    {
        GameManager.instance.throwableList.Remove(this);
        OnDestroyed();
    }

    public virtual void OnDestroyed()
    {
        
    }


    public virtual void OnFixedUpdate()
    {
        if (!enableCustomGravity)
            return;

        body.velocity += CustomGravity.GetGravity(Vector3.zero) * Time.deltaTime;
    }

    public virtual void LastFixedUpdate()
    {

    }

    float autoDestoryAltitude=-100f;
   

    public virtual void OnUpdate()
    {
        if (transform.position.y < autoDestoryAltitude)
        { Destroy(gameObject); }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ActionBaseComponent.OnSound != null&&bounceCount<distractAllowBounceCount)
        {
            ActionBaseComponent.OnSound(transform.position, bounceSoundRange);
        }
        bounceCount++;
    }
}
