using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Proximity : MonoBehaviour
{


    //public float force=100f;
    //public float AirTargetMutiplier = 10f;

    //private void OnTriggerEnter(Collider other)
    //{
     
    //    Rigidbody target = other.GetComponent<Rigidbody>();

    //    if (other.GetComponent<MovingSphere>().StepsSinceLastGrounded < 10)
    //    {
    //        var sphere = other.GetComponent<MovingSphere>();
    //        //Debug.Log("Active Proximity To Gorund");
    //        //  other.GetComponent<MovingSphere>().Jump(CustomGravity.GetGravity(Vector3.zero));
    //        sphere.disableProbeTime = 30f;
    //        sphere.Damage(0.3f);
    //        MovingSphere.onPointGain(-200000);
    //        //target.AddForce(((other.transform.position - transform.position).normalized+CustomGravity.GetGravity(Vector3.zero)).normalized * force*groundTargetMutiplier);
    //        target.AddForce(((other.transform.position - transform.position).normalized * force + CustomGravity.GetGravity(Vector3.zero)) * AirTargetMutiplier * 2);
    //    }
    //    else
    //    {
    //        target.AddForce((other.transform.position - transform.position).normalized * force * AirTargetMutiplier);
    //    }

    //    gameObject.SetActive(false);
    //}
}
