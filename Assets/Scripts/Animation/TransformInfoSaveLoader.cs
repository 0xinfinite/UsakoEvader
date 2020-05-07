using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformInfoSaveLoader : MonoBehaviour {

    [System.Serializable]
    public struct TransformInfo
    {
        public Transform transform;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }

    public Transform[] targetTransform;

    public void ConvertToStructValues()
    {
        transforms = new TransformInfo[targetTransform.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].transform = targetTransform[i];
        }


    }

    public TransformInfo[] transforms;

    public void SaveTransformValues()
    {
        for(int i =0; i < transforms.Length; i++)
        {
            transforms[i].position = transforms[i].transform.localPosition;
            transforms[i].rotation = transforms[i].transform.localRotation;
            transforms[i].scale = transforms[i].transform.localScale;
        }
    }

    public void LoadTransformValues()
    {
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i].transform.localPosition = transforms[i].position;
            transforms[i].transform.localRotation = transforms[i].rotation;
            transforms[i].transform.localScale=transforms[i].scale;
        }
    }
}
