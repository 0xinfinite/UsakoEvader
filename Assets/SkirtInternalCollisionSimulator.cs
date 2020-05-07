using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkirtInternalCollisionSimulator : MonoBehaviour
{
    public Transform[] skirtParentBones;
    private Vector3[] skirtParentBonesInitPos;
    private Vector3[] skirtParentBonesInitEuler;
    public Transform[] skirtBones;
    private Vector3[] skirtBonesInitPos;
    private Vector3[] skirtBonesInitEuler;
    public Transform[] skirtBones2nd;
    private Vector3[] skirtBones2ndInitPos;

    [SerializeField] Transform[] upperJoints;
    [SerializeField] Transform[] knees;
    [SerializeField] private float jointRadius = 0.1f;

    // Start is called before the first frame update
    void Awake()
    {
        skirtParentBonesInitPos = new Vector3[skirtParentBones.Length];
        skirtParentBonesInitEuler = new Vector3[skirtParentBones.Length];

        skirtBonesInitPos = new Vector3[skirtBones.Length];
        skirtBonesInitEuler = new Vector3[skirtBones.Length];

        skirtBones2ndInitPos = new Vector3[skirtBones2nd.Length];
        
        for(int i =0; i < skirtBones.Length; ++i)
        {
            skirtParentBonesInitPos[i] = skirtParentBones[i].position;
            skirtParentBonesInitEuler[i] = skirtParentBones[i].eulerAngles;
            skirtBonesInitPos[i] = skirtBones[i].position;
            skirtBonesInitEuler[i] = skirtBones[i].eulerAngles;
            skirtBones2ndInitPos[i] = skirtBones2nd[i].position;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
