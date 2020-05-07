using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragGrenade : Throwable
{
   // public ActionBaseComponent Owner { set { owner = value; } }

    [SerializeField] float cookingTime = 3f;
    private float remainTimeToExplosion;

    Collider[] targetsBuffer = new Collider[16];
    [SerializeField] LayerMask targetLayerMask = -1;
    [SerializeField] LayerMask hitCheckLayerMask = -1;

    [SerializeField] float range=3f;
    [SerializeField] float damage = 1.5f;
    [SerializeField] AnimationCurve damageCurveByDistance = AnimationCurve.Linear(0, 1, 1, 0);

    [SerializeField] float soundRange = 40f;

    [SerializeField] ParticleSystem explosionEffect;

     void Awake()
    {
        base.OnAwake();

        targetsBuffer = new Collider[16];

        if(owner==null)
        owner = transform.parent.GetComponent<ActionBaseComponent>();
    }

    // Start is called before the first frame update
     void OnEnable()
    {
        isExplosion = false;
        explosionEffect.transform.parent = this.transform;
        explosionEffect.transform.localPosition = Vector3.zero;
        explosionEffect.transform.localRotation = Quaternion.identity;
        remainTimeToExplosion = cookingTime;
    }

    //private void Update()
    //{
      //  base.OnUpdate();
   // }

    bool isExplosion;

    // Update is called once per frame
    void FixedUpdate()
    {
        base.OnFixedUpdate();

        remainTimeToExplosion -= Time.deltaTime;

        if (remainTimeToExplosion < 0&&!isExplosion)
        {
            isExplosion = true;
            Explosion();
               //temp
        }
    }
    
    private void Explosion()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, range, targetsBuffer, targetLayerMask);
        //Debug.Log(count);

        for(int i =0; i < count; ++i)
        {
            //Debug.Log(targetsBuffer[i].name);
            if (targetsBuffer[i].attachedRigidbody.GetComponent<ActionBaseComponent>()) {
                

                RaycastHit hit;
                if (Physics.Raycast(transform.position, (targetsBuffer[i].attachedRigidbody.position - transform.position).normalized, out hit, Mathf.Infinity, hitCheckLayerMask))
                {
                    Debug.Log(hit.transform + " / " + hit + " / " + targetsBuffer[i] + " / " + targetsBuffer[i].attachedRigidbody + " / " + targetsBuffer[i].attachedRigidbody.transform);
                    if (hit.transform == targetsBuffer[i].attachedRigidbody.transform)//Important! hit.transform /= targetBuffer[i] /= targetBuffer[i].transform /= targetBuffer[i].attachedRigidbody
                    {
                        targetsBuffer[i].attachedRigidbody.GetComponent<ActionBaseComponent>().DealComplexDamage(owner.GetIndexNumInGMActorList,
                            damage * damageCurveByDistance.Evaluate(Vector3.Distance(transform.position, hit.transform.position) / range), owner.transform.position);
                    }
                }
               
            }
           
        }
        owner.InvokeSound(owner.transform.position, soundRange);

        explosionEffect.transform.parent = null;
        explosionEffect.Play();

        gameObject.SetActive(false);//Destroy(this.gameObject);
    }
}
