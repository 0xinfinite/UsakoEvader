using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TransformInfoSaveLoader))]
public class TransformInfoSaveLoaderEditor : Editor {

    public override void OnInspectorGUI()
    {
        TransformInfoSaveLoader loader = (TransformInfoSaveLoader)target;

        base.OnInspectorGUI();

        if(GUILayout.Button("Convert Transform to TransformInfo"))
        {
            loader.ConvertToStructValues();
        }

        if (GUILayout.Button("Save Transform values"))
        {
            loader.SaveTransformValues();
        }
        if(GUILayout.Button("Load Transform values"))
        {
            loader.LoadTransformValues();
        }
    }

}
