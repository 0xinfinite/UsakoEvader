using UnityEngine;

public class RotationClass : MonoBehaviour
{
    public Vector3 tempAngles;

    public Transform target;

    private Vector3 orbitAngles;

    // Start is called before the first frame update
    void Start()
    {
        transform.localRotation = Quaternion.Euler(orbitAngles);
    }

    // Update is called once per frame
    void Update()
    {
      if(target!=null)
            RotationToTarget();    
        
    }

    public void RotationToTarget()
    {
        orbitAngles = ExtendedMathmatics.XYZAngleForwardToTargetPosition(Vector3.forward, target.position, transform.position);//angleToTarget;

        orbitAngles = ExtendedMathmatics.ConstrainAngles(orbitAngles, -90f, 90f);

        transform.rotation = Quaternion.Euler(orbitAngles);
    }

  
}
