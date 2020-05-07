using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowParentMaterialProperties : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor");

    static MaterialPropertyBlock block;

    public PerObjectMaterialProperties parentP;

    public Renderer render;


    void Start()
    {
        //baseColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        parentP = transform.parent.GetComponent<PerObjectMaterialProperties>();

         if (parentP == null)
            return;

        if (render == null)
        {
            render = GetComponent<Renderer>();
        }

        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }

        block.SetColor(baseColorId, parentP.GetColor());

        render.SetPropertyBlock(block);
    }
   


  
}
