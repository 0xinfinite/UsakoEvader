using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class UIGradientColor : MonoBehaviour
{
    public Color c1;
    public Color c2;

    public Image img;

    public MeshFilter mesh;

    // Start is called before the first frame update
    //void Awake()
    //{
    //    if (render == null)
    //    {
    //        render = GetComponent<Renderer>();
    //    }
    //}

    private void OnValidate()
    {
        if (mesh == null)
            return;

        for(int i =0; i< mesh.sharedMesh.colors.Length; ++i)
        {
            mesh.sharedMesh.colors[i] = i>1? c2:c1;
        }
        
    }

}
