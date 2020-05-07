using UnityEngine;

//[DisallowMultipleComponent]
[ExecuteInEditMode()]
public class PerObjectMaterialProperties : MonoBehaviour
{

    static int baseColorId = Shader.PropertyToID("_BaseColor");
    static int cutoffId = Shader.PropertyToID("_Cutoff"),
        metallicId = Shader.PropertyToID("_Metallic"),
        smoothnessId = Shader.PropertyToID("_Smoothness"),
        emissionColorId = Shader.PropertyToID("_EmissionColor"),
        wpId = Shader.PropertyToID("_ObjectWorldPosition"),
        baseMapId = Shader.PropertyToID("_BaseMap"),
    baseMapSTId = Shader.PropertyToID("_BaseMap_ST");//,
    //coreShadowsId = Shader.PropertyToID("_CORE_SHADOWS");

    static MaterialPropertyBlock block;

    [SerializeField]
    Color baseColor = Color.white;
    public Color BaseColor { get { return baseColor; } }

    [SerializeField]
    Texture2D baseMap;
    [SerializeField]
    bool setBaseMap;
    public void SetColor(Color c)
    {
        baseColor = c;
        OnValidate();
    }
    public Color GetColor()
    {
        return baseColor;
    }

    [SerializeField, ColorUsage(false, true)]
    Color emissionColor = Color.black;


    [SerializeField, Range(0f, 1f)]
    float cutoff = 0.5f, metallic = 0f, smoothness = 0.5f;

    [SerializeField]
    Vector4 baseMapOffset = new Vector4(1,1,0,0);
    public Vector4 BaseMapOffset { get { return baseMapOffset; } set { baseMapOffset = value; } }

    //[SerializeField] bool isCoreShadow;

    private Renderer render;

    [SerializeField] int targetMaterialIndex=-1;
    [SerializeField] bool isIndexExclusive;

    void Awake()
    {
        //baseColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        OnValidate();
    }
    [SerializeField] bool updatePropertiesOnTick;
    public void UpdatePropertiesOnTick(bool set) {  updatePropertiesOnTick = set;  }
    private void Update()
    {
        if (updatePropertiesOnTick)
        {
            OnValidate();
        }
    }

    void OnValidate()
    {
        if(render == null)
        {
            render = GetComponent<Renderer>();
        }

        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }
        if (setBaseMap)
        {
            block.SetTexture(baseMapId, baseMap);
        }
        else if(block.GetTexture(baseMapId)!=null)
        {
            block = null;
            block = new MaterialPropertyBlock();
        }
        block.SetColor(baseColorId, baseColor);
        block.SetFloat(cutoffId, cutoff);
        block.SetFloat(metallicId, metallic);
        block.SetFloat(smoothnessId, smoothness);
        block.SetColor(emissionColorId, emissionColor);
        block.SetVector(wpId, transform.position);
        block.SetVector(baseMapSTId, baseMapOffset);
       
        //render.SetPropertyBlock(block);
        //block.SetFloat(coreShadowsId, isCoreShadow ? 1 : 0);
        for(int i = 0; i<render.sharedMaterials.Length; ++i)
        {
            render.SetPropertyBlock(isIndexExclusive? (targetMaterialIndex == -1 ? null : (targetMaterialIndex != i ? block : null)) :
                (targetMaterialIndex ==-1?block: (targetMaterialIndex==i?block:null)), i);
        }
        //if (targetMaterialIndex ==-1 )
        //{
        //    render.SetPropertyBlock(block);
        //}
        //else
        //{
        //    render.SetPropertyBlock(block, targetMaterialIndex);
        //}
    }

    private void OnDestroy()
    {
        render.SetPropertyBlock(null);
     //   block.Clear();
    }
}