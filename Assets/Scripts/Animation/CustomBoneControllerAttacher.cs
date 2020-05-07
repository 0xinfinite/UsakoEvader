using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Animations;


[ExecuteInEditMode]
public class CustomBoneControllerAttacher : MonoBehaviour {

    public StreamReader reader;

    public string fileName;

   // public List<Transform> boneList;

        private Transform[] FindTransformOfChild(Transform target)
    {
        List<Transform> list = new List<Transform>();
        //Debug.Log(target.name+"'s child Count : "+target.childCount);
        for (int i = 0; i < target.childCount; i++)
        {
                Transform[] temp= FindTransformOfChild(target.GetChild(i));
            list.AddRange(temp);
        }
        list.Add(target);

        return list.ToArray();
    }

    public void AttachBoneConstraint()
    {
        try
        {
            reader = new StreamReader(Path.Combine(Application.streamingAssetsPath, fileName));

            string allRead = reader.ReadToEnd();

            Dictionary<int, List<string>> data = new Dictionary<int, List<string>>();

            string[] line = allRead.Split('\n');

Debug.Log(line.Length);


            for (int i = 0; i < line.Length; i++)
            {
                string[] tab = line[i].Split('\t');

                List<string> contents = new List<string>();
                for (int j = 0; j < tab.Length; j++)
                {
                    contents.Add(tab[j]);
                }
                data.Add(i, contents);
            }

            Debug.Log(data.Count);

            Dictionary<string, Transform> bone = new Dictionary<string, Transform>();
            //for (int m = 0; m < boneList.Count; m++)
            //{
            //    bone.Add(boneList[m].name, boneList[m]);
            //}
            Transform[] allTransform = FindTransformOfChild(this.transform);//FindObjectsOfType<Transform>();
            Debug.Log(allTransform.Length);
            for (int m = 0; m < allTransform.Length; m++)
            {
                bone.Add(allTransform[m].name, allTransform[m]);
            }

            Debug.Log(bone.Count);

            for (int k = 0; k < data.Count; k++)
            {
                Debug.Log(data[k][0]);
                switch (data[k][1])
                    {
                        case "LookAt":
                        //Transform temp = Instantiate(bone[data[k][0]], bone[data[k][0]].position, Quaternion.identity, bone[data[k][0]]);
                        //LookAtConstraint look = temp.gameObject.AddComponent<LookAtConstraint>() as LookAtConstraint;
                        //if (!data[k][2].Equals("World"))
                        //{
                        //    look.useUpObject = true;
                        //    look.worldUpObject = bone[data[k][2]];
                        //}
                        //for (int l = 3; l < data[k].Count; l += 2)
                        //{
                        //    ConstraintSource source = new ConstraintSource();
                        //    source.weight = float.Parse(data[k][l + 1]);
                        //    look.AddSource(source);
                        //}
                        //look.constraintActive=true;
                        //temp.parent = bone[data[k][0]].parent;
                        //bone[data[k][0]].parent = temp;
                        //temp.name = bone[data[k][0]].name + "'s Controller";
                        //
                        //

                        //More transform and components but Stable Way
                        Transform temp = Instantiate(bone[data[k][0]], bone[data[k][0]].position, Quaternion.identity, bone[data[k][0]].parent);
                        LookAtConstraint look = temp.gameObject.AddComponent<LookAtConstraint>() as LookAtConstraint;
                        if (!data[k][2].Equals("World"))
                        {
                            look.useUpObject = true;
                            Debug.Log(bone[data[k][2]]);
                            look.worldUpObject = bone[data[k][2]];
                        }
                        for (int l = 3; l < data[k].Count; l += 2)
                        {
                            ConstraintSource source = new ConstraintSource();
                            source.sourceTransform = bone[data[k][l]];
                            source.weight = float.Parse(data[k][l + 1]);
                            look.AddSource(source);
                        }
                        look.constraintActive = true;
                        RotationConstraint lookRot = bone[data[k][0]].gameObject.AddComponent<RotationConstraint>() as RotationConstraint;
                        ConstraintSource lookSource = new ConstraintSource();
                        lookRot.weight = 0f;
                        lookSource.sourceTransform = temp;
                        lookSource.weight = 1f;
                        lookRot.AddSource(lookSource);
                        temp.name = bone[data[k][0]].name + "'s Controller";


                        //AimConstraint look = bone[data[k][0]].gameObject.AddComponent<AimConstraint>() as AimConstraint;
                        //if (!data[k][4].Equals("World"))
                        //{
                        //    look.worldUpType = AimConstraint.WorldUpType.ObjectUp;
                        //    look.worldUpObject = bone[data[k][4]];
                        //}
                        //else
                        //{
                        //    look.worldUpType = AimConstraint.WorldUpType.SceneUp;
                        //}
                        //switch (data[k][2])
                        //{
                        //    case "X":
                        //        look.aimVector = new Vector3(1, 0, 0);
                        //        break;
                        //    case "Y":
                        //        look.aimVector = new Vector3(0, 1, 0);
                        //        break;
                        //    case "Z":
                        //        look.aimVector = new Vector3(0, 0, 1);
                        //        break;
                        //    case "-X":
                        //        look.aimVector = new Vector3(-1, 0, 0);
                        //        break;
                        //    case "-Y":
                        //        look.aimVector = new Vector3(0, -1, 0);
                        //        break;
                        //    case "-Z":
                        //        look.aimVector = new Vector3(0, 0, -1);
                        //        break;
                        //}
                        //switch (data[k][3])
                        //{
                        //    case "X":
                        //        look.upVector = new Vector3(1, 0, 0);
                        //        break;
                        //    case "Y":
                        //        look.upVector = new Vector3(0, 1, 0);
                        //        break;
                        //    case "Z":
                        //        look.upVector = new Vector3(0, 0, 1);
                        //        break;
                        //    case "-X":
                        //        look.upVector = new Vector3(-1, 0, 0);
                        //        break;
                        //    case "-Y":
                        //        look.upVector = new Vector3(0, -1, 0);
                        //        break;
                        //    case "-Z":
                        //        look.upVector = new Vector3(0, 0, -1);
                        //        break;
                        //}
                        //for (int l = 5; l < data[k].Count; l += 2)
                        //{
                        //    ConstraintSource source = new ConstraintSource();
                        //    source.sourceTransform = bone[data[k][l]];
                        //    source.weight = float.Parse(data[k][l + 1]);
                        //    look.AddSource(source);
                        //}

                        //
                        //LookAtConstraint look = bone[data[k][0]].gameObject.AddComponent<LookAtConstraint>() as LookAtConstraint;
                        //if (!data[k][2].Equals("World"))
                        //{
                        //    look.useUpObject = true;
                        //    look.worldUpObject = bone[data[k][2]];
                        //}
                        //for (int l = 3; l < data[k].Count; l += 2)
                        //{
                        //    ConstraintSource source = new ConstraintSource();
                        //    source.sourceTransform = bone[data[k][l]];
                        //    source.weight = float.Parse(data[k][l + 1]);
                        //    look.AddSource(source);
                        //}
                        break;
                        case "Rotate":
                        //RotationConstraint rot = transform.Find(data[k][0]).gameObject.AddComponent<RotationConstraint>() as RotationConstraint;
                        RotationConstraint rot = bone[data[k][0]].gameObject.AddComponent<RotationConstraint>() as RotationConstraint;
                        for (int l = 2; l < data[k].Count; l += 2)
                        {
                            ConstraintSource source = new ConstraintSource();
                            source.sourceTransform = bone[data[k][l]];
                            source.weight = float.Parse(data[k][l + 1]);
                            rot.AddSource(source);
                        }
                        break;
                    }
            }

            reader.Close();
        }
        catch {
            Debug.LogError("에러발생, StreamReader 닫기");
            reader.Close(); }
    }
    public void AttachBoneRotationConstraint()
    {
        //try
        //{
        //    reader = new StreamReader(Path.Combine(Application.streamingAssetsPath, fileName));

        //    string allRead = reader.ReadToEnd();

        //    Dictionary<int, List<string>> data = new Dictionary<int, List<string>>();

        //    string[] line = allRead.Split('\n');


        //    for (int i = 0; i < line.Length - 1; i++)
        //    {
        //        string[] tab = line[i].Split('\t');

        //        List<string> contents = new List<string>();
        //        for (int j = 0; j < tab.Length; j++)
        //        {
        //            contents.Add(tab[j]);
        //        }
        //        data.Add(i, contents);
        //    }

        //    Dictionary<string, Transform> bone = new Dictionary<string, Transform>();
        //    //for (int m = 0; m < boneList.Count; m++)
        //    //{
        //    //    bone.Add(boneList[m].name, boneList[m]);
        //    //}
        //    Transform[] allTransform = FindObjectsOfType<Transform>();
        //    for (int m = 0; m < allTransform.Length; m++)
        //    {
        //        bone.Add(allTransform[m].name, allTransform[m]);
        //    }


        //    for (int k = 0; k < data.Count; k++)
        //    {
        //        switch (data[k][1])
        //        {
        //            case "LookAt":
        //                //LookAtConstraint look = transform.Find(data[k][0]).gameObject.AddComponent<LookAtConstraint>() as LookAtConstraint;
        //                //Transform temp = Instantiate(bone[data[k][0]], bone[data[k][0]].position, Quaternion.identity, bone[data[k][0]]);
        //                //LookAtConstraint look = temp.gameObject.AddComponent<LookAtConstraint>() as LookAtConstraint;
        //                //if (!data[k][2].Equals("World"))
        //                //{
        //                //    look.useUpObject = true;
        //                //    look.worldUpObject = bone[data[k][2]];
        //                //}
        //                //for (int l = 3; l < data[k].Count; l += 2)
        //                //{
        //                //    ConstraintSource source = new ConstraintSource();
        //                //    source.weight = float.Parse(data[k][l + 1]);
        //                //    look.AddSource(source);
        //                //}
        //                //look.constraintActive = true;
        //                //temp.parent = bone[data[k][0]].parent;
        //                //bone[data[k][0]].parent = temp;
        //                //temp.name = bone[data[k][0]].name + "'s Controller";
        //                //LookAtConstraint look = bone[data[k][0]].gameObject.AddComponent<LookAtConstraint>() as LookAtConstraint;
        //                //if (!data[k][2].Equals( "World"))
        //                //{
        //                //    look.useUpObject = true;
        //                //    look.worldUpObject = bone[data[k][2]];
        //                //}
        //                //for (int l = 3; l < data[k].Count; l += 2)
        //                //{
        //                //    ConstraintSource source = new ConstraintSource();
        //                //    source.sourceTransform = bone[data[k][l]];
        //                //    source.weight = float.Parse(data[k][l + 1]);
        //                //    look.AddSource(source);
        //                //}
        //                break;
        //            case "Rotate":
        //                //RotationConstraint rot = transform.Find(data[k][0]).gameObject.AddComponent<RotationConstraint>() as RotationConstraint;
        //                RotationConstraint rot = bone[data[k][0]].gameObject.AddComponent<RotationConstraint>() as RotationConstraint;
        //                for (int l = 2; l < data[k].Count; l += 2)
        //                {
        //                    ConstraintSource source = new ConstraintSource();
        //                    if ((bone[data[k][l]].name+"'s Controller").Equals(bone[data[k][l]].parent.name))
        //                    { source.sourceTransform = bone[data[k][l]].parent;
        //                        Debug.Log("parent");
        //                    }
        //                    else
        //                        source.sourceTransform = bone[data[k][l]];
        //                    source.sourceTransform = bone[data[k][l]];
        //                    source.weight = float.Parse(data[k][l + 1]);
        //                    rot.AddSource(source);
        //                }
        //                break;
        //        }
        //    }

        //    reader.Close();
        //}
        //catch
        //{
        //    Debug.LogError("에러발생, StreamReader 닫기");
        //    reader.Close();
        //}
    }

    public void AllConstraintActive()
    {
        LookAtConstraint[] look = FindObjectsOfType<LookAtConstraint>();

        RotationConstraint[] rot = FindObjectsOfType<RotationConstraint>();

        // AimConstraint[] aim = FindObjectsOfType<AimConstraint>();

        for(int i = 0; i<look.Length;i++)
        {
            // l.locked = true;
            look[i].constraintActive = true;
            look[i].locked = true;
        }
        for(int i = 0; i<rot.Length;i++)
        {
            // r.locked = true;
            rot[i].constraintActive = true;
            rot[i].weight = 1f;
            rot[i].locked = true;
        }


        //foreach(AimConstraint a in aim)
        //{
        //    a.locked = true;
        //    a.constraintActive = true;
        //}

        //  StartCoroutine(ActiveConstraint());

    }
    public void LookAtConstraintActive()
    {
        LookAtConstraint[] look = FindObjectsOfType<LookAtConstraint>();


        // AimConstraint[] aim = FindObjectsOfType<AimConstraint>();

        for (int i = 0; i < look.Length; i++)
        {
            // l.locked = true;
            look[i].constraintActive = true;
            look[i].locked = true;
        }



    }
    public void RotationConstraintActive()
    {
        RotationConstraint[] rot = FindObjectsOfType<RotationConstraint>();

        // AimConstraint[] aim = FindObjectsOfType<AimConstraint>();

        for (int i = 0; i < rot.Length; i++)
        {
            // r.locked = true;
            rot[i].constraintActive = true;
        }


    }
    public void RotationConstraintLock()
    {
        RotationConstraint[] rot = FindObjectsOfType<RotationConstraint>();

        // AimConstraint[] aim = FindObjectsOfType<AimConstraint>();

        for (int i = 0; i < rot.Length; i++)
        {
            // r.locked = true;
            rot[i].weight = 1f;
            rot[i].locked = true;
        }


    }
    IEnumerator ActiveConstraint()
    {
        LookAtConstraint[] look = FindObjectsOfType<LookAtConstraint>();

        RotationConstraint[] rot = FindObjectsOfType<RotationConstraint>();

        // AimConstraint[] aim = FindObjectsOfType<AimConstraint>();

        foreach (LookAtConstraint l in look)
        {
            // l.locked = true;
            l.constraintActive = true;
            l.locked = true;
        }
        foreach (RotationConstraint r in rot)
        {
            // r.locked = true;
            r.constraintActive = true;
            yield return null;
            r.weight = 0;
            yield return null;
            r.locked = false;
            yield return null;
            r.weight = 1;
            yield return null;
            r.locked = true;
            yield return null;
        }
    }

    public void AllConstraintRemove()
    {
        LookAtConstraint[] look = FindObjectsOfType<LookAtConstraint>();

        RotationConstraint[] rot = FindObjectsOfType<RotationConstraint>();

       // AimConstraint[] aim = FindObjectsOfType<AimConstraint>();

        for (int i = look.Length - 1; i >= 0; i--)
        {
            DestroyImmediate(look[i]);
        }
        for (int j = rot.Length - 1; j >= 0; j--)
        {
            DestroyImmediate(rot[j]);
        }
        //for(int k = aim.Length - 1; k >= 0; k--)
        //{
        //    DestroyImmediate(aim[k]);
        //}

    }

    public List<LookAtConstraint> lookAtConstraint;
    public List<RotationConstraint> rotationConstraint;
    //public List<AimConstraint> aimConstraint;

    public void AllConstraintCountCheck()
    {
        LookAtConstraint[] look = FindObjectsOfType<LookAtConstraint>();

        for (int i = 0; i < look.Length; i++)
        {
            lookAtConstraint.Add(look[i]);
            Debug.Log(look[i].transform.name);
                }
        Debug.Log("LookAtConstraint 갯수 : " + look.Length);

        RotationConstraint[] rot = FindObjectsOfType<RotationConstraint>();

        for (int i = 0; i < rot.Length; i++)
        {
            rotationConstraint.Add(rot[i]);
            Debug.Log(rot[i].transform.name);
        }
        Debug.Log("RotationConstraint 갯수 : " + rot.Length);

        //AimConstraint[] aim = FindObjectsOfType<AimConstraint>();

        //for (int i = 0; i < aim.Length; i++)
        //{
        //    aimConstraint.Add(aim[i]);
        //    Debug.Log(aim[i].transform.name);
        //}
        //Debug.Log("AimConstraint 갯수 : " + aim.Length);

    }
}
