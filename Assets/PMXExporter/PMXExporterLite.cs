using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MMDataIO.Pmx;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif
 
[ExecuteInEditMode]
public class PMXExporterLite : MonoBehaviour
{

#if UNITY_EDITOR

    [ContextMenu("Export")]
    void Init()
    {

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

        /*
        using (FileStream fs = new FileStream("Assets/pmxexport/vanillacp.pmx", FileMode.OpenOrCreate))
        using (BinaryWriter bw = new BinaryWriter(fs))
        {
            pmx.Write(bw);
        }
        Debug.Log("Saved testing vanillacp.pmx");
        */

        var slots = new List<PmxSlotData>();

        #region build_bone
        {//bone

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
                BoneName = b.bone.name,
                BoneNameE = b.bone.name,
                BoneId = b.id,
                Pos = b.bone.position,
                ParentId = b.bone.parent == null ? -1 : ((
                    indexedBones.Where(elm => elm.bone.name == b.bone.parent.name).FirstOrDefault()
                    ?? indexedBones.First()
                    ).id),
                Flag = BoneFlags.VISIBLE | BoneFlags.ROTATE | BoneFlags.OP,
                PosOffset = new Vector3(0, 0, 0.1f),
            });

            pmx.BoneArray = create.ToArray();
            
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



                Mesh mesh = null;
                var renderer = orgRenderers[i];

                bool isSkinned;
                if (renderer is SkinnedMeshRenderer)
                {
                    isSkinned = true;

                    //(Mesh) Instantiate(((SkinnedMeshRenderer)renderer).sharedMesh);
                    mesh = ((SkinnedMeshRenderer)renderer).sharedMesh;
                }
                else if (renderer is MeshRenderer)
                {
                    isSkinned = false;
                    var mfilter = renderer.GetComponent<MeshFilter>();
                    mesh = mfilter.sharedMesh;
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
                    var blendPos = bpos.Select((pos, deltaIdx) => new { vertexIndex = Offsets[i] + deltaIdx, delta = pos }).ToArray();

                    return new { blendIdx = idx, name = name, blendPos = blendPos };
                });

                morphs.AddRange(
                    blendShapes.Select(x => new PmxMorphData()
                    {
                        MorphName = x.name,
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
                    var parentBone = pmx.BoneArray.Select((b, i) => new { id = i, bone = b }).Where(pb => pb.bone.BoneNameE.Equals(renderers[pos.index].transform.parent.name)).FirstOrDefault();

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
            NormalSlot = true,
            Type = SlotType.BONE,
            SlotName = "Root",
            SlotNameE = "Root"
        });

        slots.Add(new PmxSlotData()
        {
            Indices = pmx.MorphArray.Select((x, i) => i).ToArray(),
            NormalSlot = true,
            Type = SlotType.MORPH,
            SlotName = "表情",
            SlotNameE = "Exp"
        });

        pmx.SlotArray = slots.ToArray();

        {
            //pmx.Header.Version = 2.0f;
            pmx.Header.NumberOfExtraUv = 0;
            pmx.Header.VertexIndexSize = 4;
            pmx.Header.TextureIndexSize = 4;
            pmx.Header.MaterialIndexSize = 4;
            pmx.Header.BoneIndexSize = 4;
            pmx.Header.MorphIndexSize = 4;
            pmx.Header.RigidIndexSize = 4;
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
#endif
}