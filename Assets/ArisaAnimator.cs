using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArisaAnimator : MonoBehaviour
{
    private Animator animator;

    [SerializeField] private float lookLimit = 0.3f;

    public MagicaCloth.MagicaBoneCloth cloth;

    public Renderer bodyRender;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        bodyRender.sharedMaterials[0].SetTextureOffset("_BaseMap", new Vector2(1, 0));
    }
    [Range(0,1)]
    public float armWeight=0;

    [Range(0, 1)]
    public float lookWeight = 1;

    public Transform target;

    private IEnumerator Start()
    {
        while (true)
        {
            animator.SetInteger("Idle", Random.Range(0, 3));
            if (exprassionCount>0)
            {
                animator.SetInteger("Expression", Random.Range(0, exprassionCount));
            }
            yield return new WaitForSeconds(2f);
        }
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!GameManager.instance&&!target) return;

        if (target == null)
            target = GameManager.instance.spheres[0].transform;

        Vector3 localTgtPos = transform.InverseTransformPoint(target.position);
        localTgtPos.z = localTgtPos.z<lookLimit? lookLimit: localTgtPos.z;

        animator.SetLookAtWeight(lookWeight);
        animator.SetLookAtPosition(transform.TransformPoint(localTgtPos));//target.position);//GameManager.instance.spheres[0].transform.position);

        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, armWeight);
        animator.SetIKPosition(AvatarIKGoal.RightHand, transform.TransformPoint(localTgtPos) /*target.position*/);// GameManager.instance.spheres[0].transform.position);

    }


    public void ResetCloth()
    {
        if (cloth == null) return;

        cloth.ResetCloth();
    }

    public int exprassionCount;
}
