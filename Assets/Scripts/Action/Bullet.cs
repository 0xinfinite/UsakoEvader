using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //Rigidbody body;

    //private Transform shooter;

    private ActionBaseComponent shooter;
    private int shootersWeaponIndex;
    
    //public MovingSphere sphere;
    private Vector3 thisVelocity;
    public float pushPower=0.2f;
    private Vector3 firstFirePosition;
    private Vector3 direction;
    private float speed;
    public LayerMask hitMask = -1;

    public Transform bulletHole;

    [SerializeField] float bulletRadius;

    // Start is called before the first frame update
    //void Awake()
    //{
    //  body = GetComponent<Rigidbody>();
    //}

    RaycastHit hit;

    private void FixedUpdate()
    {
        float passedDistance = Vector3.Distance(transform.position, firstFirePosition);

        if (passedDistance > shooter.CarriedRangeWeapon(shootersWeaponIndex).MaximumAttackRange)
        {
            gameObject.SetActive(false);
        }

        if (bulletRadius <= 0)
        {
            if (Physics.Raycast(transform.position, direction, out hit, thisVelocity.magnitude * Time.deltaTime, hitMask))
            {
                if (hit.transform.GetComponent<ActionBaseComponent>())
                {
                    ActionBaseComponent target = hit.transform.GetComponent<ActionBaseComponent>();

                    target.Damage(shooter.CarriedRangeWeapon(shootersWeaponIndex).AttackDamage * shooter.CarriedRangeWeapon(shootersWeaponIndex).AttackEffectByDistance.Evaluate(passedDistance / shooter.CarriedRangeWeapon(shootersWeaponIndex).MaximumAttackRange), shooter.transform.position);
                    target.Body.velocity += thisVelocity * pushPower;

                    if (bulletHole != null)
                    {
                        Transform hole = Instantiate(bulletHole, hit.point + hit.normal * 0.0001f, Quaternion.LookRotation(hit.normal, Vector3.up), target.transform);
                    }
                }
                else if (bulletHole != null)
                {
                    Transform hole = Instantiate(bulletHole, hit.point + hit.normal * 0.0001f, Quaternion.LookRotation(hit.normal, Vector3.up), null);
                }
                gameObject.SetActive(false);
            }
        }
        else
        {
            if (Physics.SphereCast(transform.position, bulletRadius, direction, out hit, thisVelocity.magnitude * Time.deltaTime, hitMask))
            {
                if (hit.transform.GetComponent<ActionBaseComponent>())
                {
                    ActionBaseComponent target = hit.transform.GetComponent<ActionBaseComponent>();

                    target.Damage(shooter.CarriedRangeWeapon(shootersWeaponIndex).AttackDamage * shooter.CarriedRangeWeapon(shootersWeaponIndex).AttackEffectByDistance.Evaluate(passedDistance / shooter.CarriedRangeWeapon(shootersWeaponIndex).MaximumAttackRange), shooter.transform.position);
                    target.Body.velocity += thisVelocity * pushPower;

                    if (bulletHole != null)
                    {
                        Transform hole = Instantiate(bulletHole, hit.point + hit.normal * 0.0001f, Quaternion.LookRotation(hit.normal, Vector3.up), target.transform);
                    }
                }
                else if (bulletHole != null)
                {
                    Transform hole = Instantiate(bulletHole, hit.point + hit.normal * 0.0001f, Quaternion.LookRotation(hit.normal, Vector3.up), null);
                }
                gameObject.SetActive(false);
            }
        }


        transform.Translate(thisVelocity*Time.deltaTime, Space.World);
    }

    //private void OnCollisionStay(Collision collision)
   // {
        //if (collision.transform == sphere.transform)
        //    sphere.Damage(damage);
        //gameObject.SetActive(false);
   // }

    public void Fire(Vector3 firePos, Vector3 velocity, ActionBaseComponent owner)
    {
        shooter = owner;
        shootersWeaponIndex = owner.CurrentRangeWeaponNum;
        firstFirePosition = firePos;
        transform.position = firePos;
        float velocityMutliplier = 1;
        if (SceneLoader.instance)
        {
            if (SceneLoader.currentDifficulty() == Difficulty.Easy)
            {
                velocityMutliplier *= 0.3f;
            }
        }
        thisVelocity = velocity*velocityMutliplier;
      //  velocityPerDT = velocity*Time.deltaTime;//body.velocity = velocity;
        direction = velocity.normalized;
        //speed = velocity.magnitude*Time.deltaTime;
    }
}
