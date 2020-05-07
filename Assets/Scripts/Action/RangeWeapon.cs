using UnityEngine;
//using UnityEditor;

    [System.Serializable]
    public struct RangeAttackEffect
{
    public GameObject obj;

    public LineRenderer lineRenderer;
    public LineRenderer[] lineRenderers;
    public Animator shootAnim;
    public ParticleSystem particle;

    public void PlayShootAnimation(Vector3 firePos, Vector3 impactPos, RangeWeapon.RangeAttackType attackType)
    {
        switch (attackType)
        {
            case RangeWeapon.RangeAttackType.Hitscan:
                shootAnim.Play("HitscanFireStoped");
                lineRenderer.SetPosition(0, firePos);
                lineRenderer.SetPosition(1, impactPos);
                shootAnim.Play("HitscanFire");
                break;
            case RangeWeapon.RangeAttackType.DirectionalRanged:
                if (obj.transform.parent == null)
                {
                    particle.transform.position = firePos;
                    particle.transform.rotation = Quaternion.LookRotation((impactPos - firePos).normalized);
                }

                particle.Stop();
                particle.Play();
                break;
        }
    }

    public void SetLineRenderers(LineRenderer[] renderers)
    {
        lineRenderers = new LineRenderer[renderers.Length];

        for(int i =0; i < renderers.Length; ++i)
        {
            lineRenderers[i] = renderers[i];
        }
    }

    public void PlayShootOneOfLine(Vector3 firePos, Vector3 impactPos, int lineRendererIndex)
    {
                shootAnim.Play("HitscanFireStoped");
                lineRenderers[lineRendererIndex].SetPosition(0, firePos);
                lineRenderers[lineRendererIndex].SetPosition(1, impactPos);
                shootAnim.Play("HitscanFire");
        
    }
}

[CreateAssetMenu(menuName = "Action Game Template/Range Weapon")]
public class RangeWeapon : ScriptableObject
{
    //[MenuItem("Assets/Create/Range Weapon")]
    //public static void CreateAsset()
    //{
    //    ScriptableObjectUtility.CreateAsset<RangeWeapon>();
    //}

    public enum RangeAttackType { Hitscan = 0, Projectile, DirectionalRanged }
    public enum TriggerType { FullAuto = 0, SemiAuto, Burst, ManualAction }

    [SerializeField] protected GameObject weaponModel;
    public GameObject WeaponModel { get { return weaponModel; } }
    [SerializeField] protected float attackDamage = 0.2f;
    public float AttackDamage { get { return attackDamage; } }
    [SerializeField] protected RangeAttackType attackType = RangeAttackType.Hitscan;
    public RangeAttackType AttackType { get { return attackType; } }
    [SerializeField] protected TriggerType triggerType = TriggerType.FullAuto;
    public TriggerType CurrentTriggerType { get { return triggerType; } }
    [SerializeField] protected float maximumAttackRange = 50f;
    public float MaximumAttackRange { get { return maximumAttackRange; } }
    [SerializeField] protected AnimationCurve attackEffectByDistance = AnimationCurve.Linear(0, 1, 1, 0);
    public AnimationCurve AttackEffectByDistance { get { return attackEffectByDistance; } }
    [SerializeField] protected AnimationCurve attackEffectByCenterDistance;
    public AnimationCurve AttackEffectByCenterDistance { get { return attackEffectByCenterDistance; } }
    [SerializeField] protected /*ColliderExtention*/MeshCollider directionalRangeAttackArea;
    public MeshCollider DirectionalRangeAttackArea { get { return directionalRangeAttackArea; } }
    [SerializeField] protected float fireRate = 0.1f;
    public float FireRate { get { return fireRate; } }
    [SerializeField] protected float burstRate = 0.3f;
    public float BurstRate { get { return burstRate; } }
    [SerializeField] protected float forceReleaseTriggerDuration = 0.3f;
    public float ForceReleaseTriggerDuration { get { return forceReleaseTriggerDuration; } }
    [SerializeField] protected int burstShotCountWhenTriggerPulled = 3;
    public int BurstShotCountWhenTriggerPulled { get { return burstShotCountWhenTriggerPulled; } }
    [SerializeField] protected float projectileSpeed = 10f;
    public float ProjectileSpeed { get { return projectileSpeed; } }
    [SerializeField]
    public Vector4[] shells = new Vector4[1] { new Vector4(0,0,0,0) };
    public Vector3 ShellDirection(int index) { return new Vector3(shells[index].x, shells[index].y, shells[index].z); }

    [SerializeField] GameObject shootAnimationObject;
    [SerializeField]public bool isShootAnimParentNull;


    public virtual RangeAttackEffect InitAnimation(Transform owner)
    {
        if (!shootAnimationObject) return new RangeAttackEffect();

        GameObject newObj = Instantiate(shootAnimationObject, owner.position, owner.rotation, isShootAnimParentNull ? null : owner);

        RangeAttackEffect effect = new RangeAttackEffect();
        effect.obj = newObj;

        switch (attackType)
        {
            case RangeAttackType.Hitscan:
                effect.lineRenderer = newObj.GetComponent<LineRenderer>();
                effect.shootAnim = newObj.GetComponent<Animator>();
                break;
            case RangeAttackType.DirectionalRanged:
                effect.particle = newObj.GetComponent<ParticleSystem>();
                break;
        }

        return effect;
    }

    

}
