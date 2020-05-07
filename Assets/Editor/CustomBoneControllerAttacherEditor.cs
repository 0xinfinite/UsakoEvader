using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomBoneControllerAttacher))]
public class CustomBoneControllerAttacherEditor : Editor {


    public override void OnInspectorGUI()
    {
        CustomBoneControllerAttacher attacher = target as CustomBoneControllerAttacher;

        base.OnInspectorGUI();


        if (GUILayout.Button("본 컨트롤러 적용"))
        {
            if (attacher)
            {
                attacher.AttachBoneConstraint();
                //MultiplyColor(com);
            }
        }
        //if (GUILayout.Button("본 RotationConstrait 적용"))
        //{
        //    if (attacher)
        //    {
        //        attacher.AttachBoneRotationConstraint();
        //        //MultiplyColor(com);
        //    }
        //}
       
        if (GUILayout.Button("LookAtConstraint 활성화"))
        {
            if (attacher)
            {
                attacher.LookAtConstraintActive();
                //MultiplyColor(com);
            }
        }
        if (GUILayout.Button("RotationConstraint 활성화"))
        {
            if (attacher)
            {
                attacher.RotationConstraintActive();
                //MultiplyColor(com);
            }
        }
        if (GUILayout.Button("RotationConstraint Lock"))
        {
            if (attacher)
            {
                attacher.RotationConstraintLock();
                //MultiplyColor(com);
            }
        }
        if (GUILayout.Button("모든 Constraint 활성화(불안정)"))
        {
            if (attacher)
            {
                attacher.AllConstraintActive();
                //MultiplyColor(com);
            }
        }
        if (GUILayout.Button("Constraint 모두 삭제"))
        {
            if (attacher)
            {
                attacher.AllConstraintRemove();
                //MultiplyColor(com);
            }
        }
        if (GUILayout.Button("Constraint 갯수 체크"))
        {
            if (attacher)
            {
                attacher.AllConstraintCountCheck();
                //MultiplyColor(com);
            }
        }

    }



}
