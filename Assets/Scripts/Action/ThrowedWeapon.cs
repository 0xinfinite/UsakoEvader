using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ThrowedWeapon : Throwable
{
    public RangeWeapon thisWeapon;

    public void Grabable()
    {
        SphereCollider triggerCol = gameObject.AddComponent<SphereCollider>();
        triggerCol.radius = 2f;
        triggerCol.isTrigger = true;
        gameObject.AddComponent<BoxCollider>();

        var mat = GetComponent<PerObjectMaterialProperties>();
        mat.SetColor(new Color(mat.BaseColor.r, mat.BaseColor.g, mat.BaseColor.b, 1));

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<ActionBaseComponent>().NearByWeapon = this;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<ActionBaseComponent>().NearByWeapon = null;
        }
    }
}
