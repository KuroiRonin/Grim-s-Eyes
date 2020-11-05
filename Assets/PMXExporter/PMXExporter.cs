using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MMDataIO.Pmx;
using System.IO;
using VRM;
using MeshUtility;

#if UNITY_EDITOR
using UnityEditor;
#endif
 
[ExecuteInEditMode]
public class PMXExporter : MonoBehaviour
{
    public bool ReplaceBoneName = true;
    public bool ConvertArmatuar_NeedReplaceBoneName = true;
    public bool ReplaceMorphName = true;

#if UNITY_EDITOR

    [ContextMenu("Export")]
    void Init()
    {
        Debug.Log("build 170");


        bool ConvertArmatuar = ReplaceBoneName && ConvertArmatuar_NeedReplaceBoneName;

        Quaternion? tmpRootRot = null;
        Quaternion? tmpLUARot = null;
        Quaternion? tmpRUARot = null;

        float caScaling = 10.0f;

        try
        {
            if (ConvertArmatuar)
            {
                var anim = GetComponent<Animator>();
                var lua = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                tmpLUARot = lua.rotation;
                lua.Rotate(new Vector3(0, 0, 30));

                var rua = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
                tmpRUARot = rua.rotation;
                rua.Rotate(new Vector3(0, 0, -30));

                tmpRootRot = transform.rotation;
                transform.Rotate(new Vector3(0, 180, 0));
                transform.localScale *= caScaling;
            }


            var filterdFileName = System.IO.Path.GetInvalidFileNameChars().Aggregate(
                name, (s, c) => s.Replace(c.ToString(), ""));
            string savepath = EditorUtility.SaveFilePanel("Export PMX", "", filterdFileName, "pmx");

            if (string.IsNullOrEmpty(savepath))
            {
                Debug.Log("SavePath not exists.");
                return;
            }

            var pmx = new PmxModelData();

            using (FileStream fs = new FileStream("Assets/PMXExporter/vanilla.pmx", FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
            {
                pmx.Read(br);
            }
            Debug.Log("Load vanilla.pmx");


            var ikTemplate = new PmxModelData();

            using (FileStream fs = new FileStream("Assets/PMXExporter/iktemplate.pmx", FileMode.Open))
            using (BinaryReader br = new BinaryReader(fs))
            {
                ikTemplate.Read(br);
            }
            Debug.Log("Load iktemplate.pmx");


            var slots = new List<PmxSlotData>();

            #region build_bone
            {//bone

                var humanoid2mmd = new SortedDictionary<HumanBodyBones, string>();
                {
                    humanoid2mmd.Add(HumanBodyBones.Hips, "下半身");
                    humanoid2mmd.Add(HumanBodyBones.Spine, "上半身");
                    humanoid2mmd.Add(HumanBodyBones.Chest, "上半身2");
                    humanoid2mmd.Add(HumanBodyBones.LeftUpperLeg, "左足");
                    humanoid2mmd.Add(HumanBodyBones.LeftLowerLeg, "左ひざ");
                    humanoid2mmd.Add(HumanBodyBones.LeftFoot, "左足首");
                    humanoid2mmd.Add(HumanBodyBones.LeftToes, "左つま先");
                    humanoid2mmd.Add(HumanBodyBones.RightUpperLeg, "右足");
                    humanoid2mmd.Add(HumanBodyBones.RightLowerLeg, "右ひざ");
                    humanoid2mmd.Add(HumanBodyBones.RightFoot, "右足首");
                    humanoid2mmd.Add(HumanBodyBones.RightToes, "右つま先");
                    humanoid2mmd.Add(HumanBodyBones.Neck, "首");
                    humanoid2mmd.Add(HumanBodyBones.Head, "頭");
                    humanoid2mmd.Add(HumanBodyBones.LeftEye, "左目");
                    humanoid2mmd.Add(HumanBodyBones.RightEye, "右目");
                    humanoid2mmd.Add(HumanBodyBones.LeftShoulder, "左肩");
                    humanoid2mmd.Add(HumanBodyBones.LeftUpperArm, "左腕");
                    humanoid2mmd.Add(HumanBodyBones.LeftLowerArm, "左ひじ");
                    humanoid2mmd.Add(HumanBodyBones.LeftHand, "左手首");
                    humanoid2mmd.Add(HumanBodyBones.RightShoulder, "右肩");
                    humanoid2mmd.Add(HumanBodyBones.RightUpperArm, "右腕");
                    humanoid2mmd.Add(HumanBodyBones.RightLowerArm, "右ひじ");
                    humanoid2mmd.Add(HumanBodyBones.RightHand, "右手首");
                    humanoid2mmd.Add(HumanBodyBones.LeftRingProximal, "左薬指１");
                    humanoid2mmd.Add(HumanBodyBones.LeftRingIntermediate, "左薬指２");
                    humanoid2mmd.Add(HumanBodyBones.LeftRingDistal, "左薬指３");
                    humanoid2mmd.Add(HumanBodyBones.LeftThumbProximal, "左親指１");
                    humanoid2mmd.Add(HumanBodyBones.LeftThumbIntermediate, "左親指２");
                    humanoid2mmd.Add(HumanBodyBones.LeftIndexProximal, "左人指１");
                    humanoid2mmd.Add(HumanBodyBones.LeftIndexIntermediate, "左人指２");
                    humanoid2mmd.Add(HumanBodyBones.LeftIndexDistal, "左人指３");
                    humanoid2mmd.Add(HumanBodyBones.LeftMiddleProximal, "左中指１");
                    humanoid2mmd.Add(HumanBodyBones.LeftMiddleIntermediate, "左中指２");
                    humanoid2mmd.Add(HumanBodyBones.LeftMiddleDistal, "左中指３");
                    humanoid2mmd.Add(HumanBodyBones.LeftLittleProximal, "左小指１");
                    humanoid2mmd.Add(HumanBodyBones.LeftLittleIntermediate, "左小指２");
                    humanoid2mmd.Add(HumanBodyBones.LeftLittleDistal, "左小指３");
                    humanoid2mmd.Add(HumanBodyBones.RightRingProximal, "右薬指１");
                    humanoid2mmd.Add(HumanBodyBones.RightRingIntermediate, "右薬指２");
                    humanoid2mmd.Add(HumanBodyBones.RightRingDistal, "右薬指３");
                    humanoid2mmd.Add(HumanBodyBones.RightThumbProximal, "右親指１");
                    humanoid2mmd.Add(HumanBodyBones.RightThumbIntermediate, "右親指２");
                    humanoid2mmd.Add(HumanBodyBones.RightIndexProximal, "右人指１");
                    humanoid2mmd.Add(HumanBodyBones.RightIndexIntermediate, "右人指２");
                    humanoid2mmd.Add(HumanBodyBones.RightIndexDistal, "右人指３");
                    humanoid2mmd.Add(HumanBodyBones.RightMiddleProximal, "右中指１");
                    humanoid2mmd.Add(HumanBodyBones.RightMiddleIntermediate, "右中指２");
                    humanoid2mmd.Add(HumanBodyBones.RightMiddleDistal, "右中指３");
                    humanoid2mmd.Add(HumanBodyBones.RightLittleProximal, "右小指１");
                    humanoid2mmd.Add(HumanBodyBones.RightLittleIntermediate, "右小指２");
                    humanoid2mmd.Add(HumanBodyBones.RightLittleDistal, "右小指３");

                }
                var humanoidMapping = new SortedDictionary<string, HumanBodyBones>();
                var humanoidDesc = GetComponent<VRM.VRMHumanoidDescription>();


                foreach (var x in humanoidDesc.Description.human)
                {
                    humanoidMapping.Add(x.boneName, x.humanBone);
                }

                System.Func<string, string> remapBoname = (orgName) =>
                {
                    string remapName = orgName;

                    if (humanoidMapping.ContainsKey(orgName))
                    {
                        HumanBodyBones key = humanoidMapping[orgName];

                        if (humanoid2mmd.ContainsKey(key))
                        {
                            remapName = humanoid2mmd[key];
                        }
                    }

                    return remapName;
                };

                if (!ReplaceBoneName)
                {
                    remapBoname = (orgName) => orgName;
                }


                var bones = new List<Transform>();
                var boneStack = new Stack<Transform>();


                boneStack.Push(transform);
                while (0 < boneStack.Count)
                {
                    var obj = boneStack.Pop();
                    bones.Add(obj);

                    foreach (Transform child in obj.transform)
                    {
                        boneStack.Push(child);
                    }
                }

                var indexedBones = bones.Select((b, idx) => new { id = idx, bone = b }).ToArray();

                var create = indexedBones.Select(b => new PmxBoneData()
                {
                    BoneName = remapBoname(b.bone.name),
                    BoneNameE = b.bone.name,
                    BoneId = b.id,
                    Pos = b.bone.position,
                    ParentId = b.bone.parent == null ? -1 : ((
                        indexedBones.Where(elm => elm.bone.name == b.bone.parent.name).FirstOrDefault()
                        ?? indexedBones.First()
                        ).id),
                    Flag = BoneFlags.VISIBLE | BoneFlags.ROTATE | BoneFlags.OP,
                    PosOffset = new Vector3(0, 0, ConvertArmatuar ? -0.5f : 0.1f),
                });

                pmx.BoneArray = create.ToArray();



                if (ConvertArmatuar)
                {
                    Debug.Log("ConvertArmatuar");
                    int centerId = -1;

                    var hips = pmx.BoneArray.Where(b => b.BoneName.Equals("下半身")).FirstOrDefault();
                    if (hips != null && 0 <= hips.ParentId && hips.ParentId < pmx.BoneArray.Length)
                    {
                        centerId = hips.ParentId;

                        var centerBone = pmx.BoneArray[centerId];
                        centerBone.BoneName = "センター";
                        centerBone.Flag |= BoneFlags.MOVE;

                        pmx.BoneArray[0].BoneName = "全ての親";
                        pmx.BoneArray[0].Flag |= BoneFlags.MOVE;
                    }

                    var spine = pmx.BoneArray.Where(b => b.BoneName.Equals("上半身")).FirstOrDefault();
                    if (0 <= centerId && spine != null)
                    {
                        spine.ParentId = centerId;
                    }


                    var bonesList = new List<PmxBoneData>(pmx.BoneArray);


                    System.Func<string, string, int, int> addIk = (ikname, targetname, parentid) =>
                   {
                       int result = -1;
                       var ik = ikTemplate.BoneArray.Where(b => b.BoneName.Equals(ikname)).FirstOrDefault();
                       if (ik != null)
                       {
                           ik = (PmxBoneData)ik.Clone();

                           result = ik.BoneId = bonesList.Count();

                           ik.ParentId = parentid;

                           var target = bonesList.Where(b => b.BoneName.Equals(targetname)).FirstOrDefault();
                           if (target != null)
                           {
                               ik.IkTargetId = target.BoneId;

                               ik.Pos = target.Pos;
                           }

                           for (int i = 0; i < ik.IkChilds.Length; i++) {

                               var name = ikTemplate.BoneArray[ik.IkChilds[i].ChildId].BoneName;
                               Debug.Log("ik child name : " + name);


                               var childbone = bonesList.Where(b => b.BoneName.Equals(name)).FirstOrDefault();
                               Debug.Log("ik child name : " + name);

                               if (childbone != null)
                               {
                                   ik.IkChilds[i].ChildId = childbone.BoneId;
                               }
                           }
                           bonesList.Add(ik);
                       }

                       return result;
                   };

                    {
                        int pid = addIk("左足ＩＫ", "左足首", 0);
                        if (0 < pid)
                            addIk("左つま先ＩＫ", "左つま先", pid);
                    }

                    {
                        int pid = addIk("右足ＩＫ", "右足首", 0);
                        if (0 < pid)
                            addIk("右つま先ＩＫ", "右つま先", pid);
                    }

                    //*

                    {
                        var head = bonesList.Where(b => b.BoneName.Equals("頭")).FirstOrDefault();

                        var eyes = new PmxBoneData()
                        {
                            BoneName = "両目",
                            BoneNameE = "eyes",
                            BoneId = bonesList.Count(),
                            Pos = new Vector3(0, head.Pos.y * 1.5f, 0),
                            ParentId = head.BoneId,
                            Flag = BoneFlags.VISIBLE | BoneFlags.ROTATE | BoneFlags.OP,
                            PosOffset = new Vector3(0, 0, -1.0f),
                        };

                        bonesList.Add(eyes);


                        var eyeBones = bonesList.Where(b => b.BoneName.Equals("左目") || b.BoneName.Equals("右目"));
                        foreach (PmxBoneData bone in eyeBones)
                        {
                            bone.LinkParentId = eyes.BoneId;
                            bone.LinkWeight = 1.0f;
                            bone.Flag |= BoneFlags.ROTATE_LINK;
                        }


                        //transform level set
                        {
                            var legsStack = new Stack<PmxBoneData>(bonesList.FindAll(b => b.BoneName.Equals("右足") || b.BoneName.Equals("左足")));
                            while(0 < legsStack.Count())
                            {
                                var bone = legsStack.Pop();

                                if (!humanoid2mmd.ContainsValue(bone.BoneName))
                                {
                                    bone.Depth = 2;
                                }


                                var childs = bonesList.FindAll(b => b.ParentId == bone.BoneId);
                                foreach (var child in childs)
                                    legsStack.Push(child);
                            }
                        }
                    }

                    pmx.BoneArray = bonesList.ToArray();

                    /**/
                }


            }

            #endregion



            {//vertex face blendshape
                var vertices = new List<Vector3>();
                var normals = new List<Vector3>();
                var uvs = new List<Vector2>();

                var renderers = new List<Renderer>();
                var boneWeights = new List<BoneWeight>();

                var faces = new List<int[]>();
                var mattex = new List<KeyValuePair<string, string>>();
                var materials = new List<Material>();

                var morphs = new List<PmxMorphData>();

                var orgRenderers = GetComponentsInChildren<Renderer>();
                //GetComponentsInChildren<MeshFilter>(true);

                var Offsets = new int[orgRenderers.Length];

                System.Func<IList, string> getCountLog = (x) => x.ToString() + "count = " + x.Count;

                Debug.Log("mesh count:" + orgRenderers.Length);

                int vertexCount = 0;
                for (int i = 0; i < orgRenderers.Length; i++)
                {
                    //Debug.Log("LoopCount : " + i);
                    Debug.Log("MeshNames : " + orgRenderers[i].gameObject.name);



                    var morphMapping = new SortedDictionary<string, string>();
                    {
                        morphMapping.Add("Face.M_F00_000_Fcl_ALL_Angry", "怒");
                        morphMapping.Add("Face.M_F00_000_Fcl_ALL_Fun", "楽");
                        morphMapping.Add("Face.M_F00_000_Fcl_ALL_Joy", "喜");
                        morphMapping.Add("Face.M_F00_000_Fcl_ALL_Sorrow", "哀");
                        morphMapping.Add("Face.M_F00_000_Fcl_ALL_Surprised", "驚");
                        morphMapping.Add("Face.M_F00_000_Fcl_BRW_Angry", "怒り");
                        morphMapping.Add("Face.M_F00_000_Fcl_BRW_Fun", "にこり２");
                        morphMapping.Add("Face.M_F00_000_Fcl_BRW_Joy", "にこり");
                        morphMapping.Add("Face.M_F00_000_Fcl_BRW_Sorrow", "困る");
                        morphMapping.Add("Face.M_F00_000_Fcl_BRW_Surprised", "上");
                        morphMapping.Add("Face.M_F00_000_Fcl_EYE_Natural", "目通常");
                        morphMapping.Add("Face.M_F00_000_Fcl_EYE_Angry", "ｷﾘｯ");
                        morphMapping.Add("Face.M_F00_000_Fcl_EYE_Close", "まばたき");
                        morphMapping.Add("Face.M_F00_000_Fcl_EYE_Close_R", "ウィンク２");
                        morphMapping.Add("Face.M_F00_000_Fcl_EYE_Close_L", "ｳｨﾝｸ２右");
                        morphMapping.Add("Face.M_F00_000_Fcl_EYE_Joy", "笑い");
                        morphMapping.Add("Face.M_F00_000_Fcl_EYE_Joy_R", "ウィンク");
                        morphMapping.Add("Face.M_F00_000_Fcl_EYE_Joy_L", "ウィンク右");
                        morphMapping.Add("Face.M_F00_000_Fcl_EYE_Sorrow", "じと目");
                        morphMapping.Add("Face.M_F00_000_Fcl_EYE_Surprised", "びっくり");
                        morphMapping.Add("Face.M_F00_000_Fcl_EYE_Extra", "目消");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_Close", "口閉");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_Up", "口上");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_Down", "口下");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_Angry", "∧");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_Small", "口小");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_Large", "口大");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_Neutral", "口角上げ");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_Fun", "にやり");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_Joy", "ワ");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_Sorrow", "▲");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_Surprised", "えー");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_A", "あ");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_I", "い");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_U", "う");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_E", "え");
                        morphMapping.Add("Face.M_F00_000_Fcl_MTH_O", "お");
                        morphMapping.Add("Face.M_F00_000_Fcl_HA_Fung1", "牙");
                        morphMapping.Add("Face.M_F00_000_Fcl_HA_Fung1_Low", "牙下");
                        morphMapping.Add("Face.M_F00_000_Fcl_HA_Fung1_Up", "牙上");
                        morphMapping.Add("Face.M_F00_000_Fcl_HA_Fung2", "ギザ歯");
                        morphMapping.Add("Face.M_F00_000_Fcl_HA_Fung2_Low", "ギザ歯下");
                        morphMapping.Add("Face.M_F00_000_Fcl_HA_Fung2_Up", "ギザ歯上");
                        morphMapping.Add("EyeExtra.M_F00_000_EyeExtra_On", "はぅ");
                    }

                    System.Func<string, string> remapMorphName = (orgName) =>
                    {
                        string remapName = orgName;

                        if (morphMapping.ContainsKey(orgName))
                        {
                            remapName = morphMapping[orgName];
                        }

                        return remapName;
                    };

                    if (!ReplaceMorphName)
                    {
                        remapMorphName = (orgName) => orgName;
                    }



                    Mesh mesh = null;
                    var renderer = orgRenderers[i];

                    bool isSkinned;
                    if (renderer is SkinnedMeshRenderer)
                    {
                        isSkinned = true;

                        Mesh srcMesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
                        mesh = srcMesh.Copy(true);
                        ((SkinnedMeshRenderer)renderer).BakeMesh(mesh);

                        mesh.boneWeights = srcMesh.boneWeights; // restore weights. clear when BakeMesh
                                                                // recalc bindposes

                        mesh.bindposes = srcMesh.bindposes.ToArray();
                        //mesh.bindposes = bones.Select(x => x.worldToLocalMatrix * dst.transform.localToWorldMatrix).ToArray();

                        //var m = src.localToWorldMatrix; // include scaling
                        var m = default(Matrix4x4);
                        m.SetTRS(Vector3.zero, transform.rotation, Vector3.one); // rotation only
                        mesh.ApplyMatrix(m);
                        




                        //mesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
                    }
                    else if (renderer is MeshRenderer)
                    {
                        isSkinned = false;
                        var mfilter = renderer.GetComponent<MeshFilter>();
                        //mesh = mfilter.sharedMesh;
                        
                        mesh = mfilter.sharedMesh.Copy(false);
                        mesh.ApplyMatrix(renderer.gameObject.transform.localToWorldMatrix);
                        //mesh.vertices = mesh.vertices.Select(v => renderer.gameObject.transform.localToWorldMatrix.MultiplyVector(v)).ToArray();

                    }
                    else
                        continue;

                    //vertex

                    Offsets[i] = vertexCount;

                    vertexCount += mesh.vertexCount;

                    vertices.AddRange(mesh.vertices);
                    normals.AddRange(mesh.normals);
                    uvs.AddRange(mesh.uv.Select(x => new Vector2() { x = x.x, y = -x.y }));

                    renderers.AddRange(mesh.vertices.Select(v => renderer));


                    var count = mesh.boneWeights != null ? mesh.boneWeights.Length : 0;

                    boneWeights.AddRange(count != 0
                        ? mesh.boneWeights
                        : mesh.vertices.Select(v => new BoneWeight() { boneIndex0 = -1, weight0 = 1 })
                        );

                    //Debug.Log("mesh concat : " + getCountLog(boneWeights));

                    //face

                    if (isSkinned)
                        for (int subIdx = 0; subIdx < renderer.sharedMaterials.Length; subIdx++)
                            faces.Add(mesh.GetTriangles(subIdx, true).Select(tri => Offsets[i] + tri).ToArray());
                    else
                        faces.Add(mesh.triangles.Select(x => x + Offsets[i]).ToArray());

                    materials.AddRange(renderer.sharedMaterials);

                    Debug.Log(renderer.sharedMaterials);
                    //materials
                    Debug.Log("MaterialCount : " + renderer.sharedMaterials.Length);
                    //Debug.Log("MaterialCount : " + renderer.sharedMaterials.Length);
                    mattex.AddRange(renderer.sharedMaterials.Select(x =>
                    {
                        /*
                        Debug.Log("Textures");
                        x.GetTexturePropertyNames().Select(t => { Debug.Log(t); return t; }).ToArray();
                        */
                        Debug.Log(x.GetTexture("_MainTex").name);
                        return new KeyValuePair<string, string>(
                            x.name
                            , x.GetTexture("_MainTex") != null ? x.GetTexture("_MainTex").name : ""
                            );
                    }));

                    //morph

                    var blendShapes = Enumerable.Range(0, mesh.blendShapeCount).Select(idx =>
                    {
                        var name = mesh.GetBlendShapeName(idx);

                        var bpos = new Vector3[mesh.vertexCount];
                        var bnol = new Vector3[mesh.vertexCount];
                        var btan = new Vector3[mesh.vertexCount];

                        var maxframe = mesh.GetBlendShapeFrameCount(idx);
                        mesh.GetBlendShapeFrameVertices(idx, maxframe - 1, bpos, bnol, btan);

                        var m = transform.localToWorldMatrix;
                        m.SetColumn(3, new Vector4(0, 0, 0, 1)); // remove translation
                        var srBpos = bpos.Select(x => m.MultiplyPoint(x)).ToArray();

                        var blendPos = srBpos.Select((pos, deltaIdx) => new { vertexIndex = Offsets[i] + deltaIdx, delta = pos}).ToArray();
                        return new { blendIdx = idx, name = name, blendPos = blendPos };
                       
                    });

                    morphs.AddRange(
                        blendShapes.Select(x => new PmxMorphData()
                        {
                            MorphName = remapMorphName(x.name),
                            MorphNameE = x.name,
                            MorphType = MorphType.VERTEX,
                            MorphArray = (x.blendPos.Select(pos => (IPmxMorphTypeData)new PmxMorphVertexData()
                            {
                                Index = pos.vertexIndex,
                                Position = pos.delta
                            }).ToArray())
                        }).ToArray());


                }

                #region build 


                Debug.Log(getCountLog(vertices));
                Debug.Log(getCountLog(normals));
                Debug.Log(getCountLog(uvs));
                Debug.Log(getCountLog(renderers));
                Debug.Log(getCountLog(boneWeights));

                Debug.Log(getCountLog(mattex));

                #region build_vertex

                var buildVertices = new List<PmxVertexData>();
                foreach (var pos in vertices.Select((vertex, index) => new { vertex, index }))
                {
                    var boneWeightOrg = new[]{
                    new {id = boneWeights[pos.index].boneIndex0 , weight = boneWeights[pos.index].weight0},
                    new {id = boneWeights[pos.index].boneIndex1 , weight = boneWeights[pos.index].weight1},
                    new {id = boneWeights[pos.index].boneIndex2 , weight = boneWeights[pos.index].weight2},
                    new {id = boneWeights[pos.index].boneIndex3 , weight = boneWeights[pos.index].weight3},
                };

                    int[] boneidRemaped;
                    float[] weightRemaped;



                    if (renderers[pos.index] is SkinnedMeshRenderer)
                    {

                        var smr = (SkinnedMeshRenderer)renderers[pos.index];

                        var boneWeightOrgFilterd = boneWeightOrg.Where(x => 0 < x.weight);

                        var boneWeightRemaped = boneWeightOrgFilterd.Select(x =>
                        {
                            var remapedBone = pmx.BoneArray.Select((b, i) => new { id = i, bone = b }).Where(b => smr.bones[x.id].name.Equals(b.bone.BoneNameE)).FirstOrDefault();

                            return new { id = remapedBone == null ? 0 : remapedBone.id, weight = x.weight };
                        });


                        boneidRemaped = boneWeightRemaped.Select(x => x.id).Concat(Enumerable.Range(0, 4).Select(y => 0)).Take(4).ToArray();
                        weightRemaped = boneWeightRemaped.Select(x => x.weight).Concat(Enumerable.Range(0, 4).Select(y => 0.0f)).Take(4).ToArray();
                    }
                    else
                    {
                        //var parentBone = pmx.BoneArray.Select((b, i) => new { id = i, bone = b }).Where(pb => pb.bone.BoneNameE.Equals(renderers[pos.index].transform.parent.name)).FirstOrDefault();
                        var parentBone = pmx.BoneArray.Select((b, i) => new { id = i, bone = b }).Where(pb => pb.bone.BoneNameE.Equals(renderers[pos.index].transform.name)).FirstOrDefault();

                        boneidRemaped = new int[] { parentBone == null ? 0 : parentBone.id, 0, 0, 0 };
                        weightRemaped = new float[] { 1, 0, 0, 0 };
                    }

                    if (boneidRemaped.Length != 4)
                        Debug.LogError("boneidRemaped count " + boneidRemaped.Length);
                    if (weightRemaped.Length != 4)
                        Debug.LogError("weightRemaped count " + weightRemaped.Length);

                    buildVertices.Add(new PmxVertexData()
                    {
                        Pos = pos.vertex,
                        Normal = normals[pos.index],
                        Uv = uvs[pos.index],

                        WeightType = WeightType.BDEF4,
                        BoneId = boneidRemaped,
                        Weight = weightRemaped,

                        Edge = 1.0f,
                    });
                }

                pmx.VertexArray = buildVertices.ToArray();

                Debug.Log("VertexCount = " + pmx.VertexArray.Length);

                #endregion



                #region build_face

                pmx.VertexIndices = faces.SelectMany(x => x).ToArray();
                Debug.Log("FaceCount = " + pmx.VertexIndices.Length);

                #endregion


                #region build_material_wip

                //long faceBorder = 0;
                pmx.TextureFiles = mattex.Select(x => x.Value).Distinct().ToArray();
                Debug.Log("TextureFiles Count = " + pmx.TextureFiles.Length);

                var orgMaterialTable = from face in faces.Select((f, i) => new { face = f, index = i })
                                       join tex in mattex.Select((t, i) => new { texture = t, index = i })
                                       on face.index equals tex.index
                                       join mat in materials.Select((m, i) => new { material = m, index = i })
                                       on face.index equals mat.index
                                       select new { face.face, tex.texture, mat.material };

                var buildMaterial = orgMaterialTable.Select(x =>
                {
                    var diffuse = new Vector4() { x = 1, y = 1, z = 1, w = 1 };
                    var ambient = new Vector4() { x = 0.5f, y = 0.5f, z = 0.5f, w = 1 };
                    var spec = new Vector4() { x = 0.5f, y = 0.5f, z = 0.5f, w = 1 };

                    try
                    {
                        var color = x.material.GetColor("_Color");
                        diffuse.x = color.r;
                        diffuse.y = color.g;
                        diffuse.z = color.b;
                        diffuse.w = color.a;
                    }
                    catch (System.Exception e) { }

                    try
                    {
                        var emissionColor = x.material.GetColor("_EmissionColor");
                        spec.x = emissionColor.r;
                        spec.y = emissionColor.g;
                        spec.z = emissionColor.b;
                        //ambient.w = emissionColor.a;
                    }
                    catch (System.Exception e) { }

                    try
                    {
                        var shadowColor = x.material.GetColor("_ShadeColor");
                        ambient.x = shadowColor.r;
                        ambient.y = shadowColor.g;
                        ambient.z = shadowColor.b;
                        //spec.w = shadowColor.a;
                    }
                    catch (System.Exception e) { }

                    return new PmxMaterialData()
                    {
                        FaceCount = x.face.Length,
                        TextureId = ArrayUtility.IndexOf(pmx.TextureFiles, x.texture.Value),
                        ToonId = -1,
                        SphereId = -1,
                        MaterialName = x.texture.Key,
                        MaterialNameE = x.texture.Key,
                        Diffuse = diffuse,
                        Ambient = ambient,
                        Specular = spec,
                        Shininess = 1.0f,
                        Flag = RenderFlags.EDGE | RenderFlags.GROUND_SHADOW | RenderFlags.SLEF_SHADOW | RenderFlags.TO_SHADOW_MAP,
                    };
                });

                pmx.MaterialArray = buildMaterial.ToArray();
                Debug.Log("Material Count = " + pmx.MaterialArray.Length);

                pmx.TextureFiles = pmx.TextureFiles.Select(x => "tex\\" + x + ".png").ToArray();

                #endregion

                #region build_morph

                var rebuildMorph = morphs.Select(x =>
                    new PmxMorphData
                    {
                        MorphName = x.MorphName,
                        MorphNameE = x.MorphNameE,
                        MorphType = x.MorphType,
                        MorphArray = x.MorphArray.Where(delta => pmx.VertexArray[((PmxMorphVertexData)delta).Index].Pos != ((PmxMorphVertexData)delta).Position).ToArray()
                    });

                pmx.MorphArray = rebuildMorph.ToArray();

                #endregion


                #endregion


            }


            slots.Add(new PmxSlotData()
            {
                Indices = pmx.BoneArray.Select((x, i) => i).ToArray(),
                NormalSlot = false,
                Type = SlotType.BONE,
                SlotName = "Root",
                SlotNameE = "Root"
            });
            slots.Add(new PmxSlotData()
            {
                Indices = pmx.MorphArray.Select((x, i) => i).ToArray(),
                NormalSlot = false,
                Type = SlotType.MORPH,
                SlotName = "表情",
                SlotNameE = "Exp"
            });

            pmx.SlotArray = slots.ToArray();


            System.Func<int, byte> getIndexSize = (count) =>
            {
                if(count < sbyte.MaxValue)
                {
                    return 1;
                }else if(count < short.MaxValue)
                {
                    return 2;
                }
                else
                {
                    return 4;
                }

            };

            {
                //pmx.Header.Version = 2.0f;
                pmx.Header.NumberOfExtraUv = 0;
                pmx.Header.VertexIndexSize = getIndexSize(pmx.VertexArray.Length);
                pmx.Header.TextureIndexSize = getIndexSize(pmx.TextureFiles.Length);
                pmx.Header.MaterialIndexSize = getIndexSize(pmx.MaterialArray.Length);
                pmx.Header.BoneIndexSize = getIndexSize(pmx.BoneArray.Length);
                pmx.Header.MorphIndexSize = getIndexSize(pmx.MorphArray.Length);

                pmx.Header.RigidIndexSize = 1;
                pmx.Header.ModelName = name;
                pmx.Header.ModelNameE = name;
                pmx.Header.Description = "Unity PMXExporter exported.";
                pmx.Header.DescriptionE = "Unity PMXExporter exported.";
            };

            var filename = savepath;
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                pmx.Write(bw);
            }

            Debug.Log(filename + " is exported.");
        }
        finally
        {
            if (ConvertArmatuar)
            {
                if (tmpRootRot != null) {
                    transform.localScale /= caScaling;
                    transform.rotation = (Quaternion)tmpRootRot;
                }
                var anim = GetComponent<Animator>();
                var lua = anim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                if(tmpLUARot != null)
                    lua.rotation = (Quaternion)tmpLUARot;

                var rua = anim.GetBoneTransform(HumanBodyBones.RightUpperArm);
                if (tmpRUARot != null)
                    rua.rotation = (Quaternion)tmpRUARot;
            }
        }

    }
#endif
}