using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Assets;
using System.Linq;
using System;
using UnityEditor;
using Assimp.Unmanaged;

public class MechStruct : MonoBehaviour
{
    const string roboResPath = "Assets/Resources/Robo";
    const string materialFolderName = "Material";
    const string meshFolderName = "Mesh";
    const string textureFolderName = "Texture";
    const string aniFolderName = "Ani";

    public GameObject root;
    public List<GameObject> parts = new List<GameObject>();
    public List<bool> isTop = new List<bool>();
    public Hod2v0 hod;
    public Assimp.AssimpImporter Importer = new Assimp.AssimpImporter();
    public string folder;
    public string mechName;
    public string filename;
    public CypherTranscoder transcoder = new CypherTranscoder();

    WindomAni2 aniFile;
    string[] partPaths;

    public static GameObject LoadMech(string path, string name)
    {
        GameObject mechGo = new GameObject(name);
        var mechStruct = mechGo.AddComponent<MechStruct>();
        mechStruct.folder = path;
        mechStruct.mechName = name;
        mechStruct.BuildStructure();
        return mechGo;
    }

    #region PathUtil
    public string GetMaterialFolder()
    {
        string path = Path.Combine(roboResPath, mechName, materialFolderName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }
    public string GetMeshFolder()
    {
        string path = Path.Combine(roboResPath, mechName, meshFolderName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }
    public string GetTextureFolder()
    {
        string path = Path.Combine(roboResPath, mechName, textureFolderName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }
    public string GetAniFolder()
    {
        string path = Path.Combine(roboResPath, mechName, aniFolderName);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }

    public string GetStructPath()
    {
        return Path.Combine(roboResPath, mechName, "Struct.prefab");
    }
    #endregion

    public void BuildStructure()
    {
        aniFile = WindomAni2.CreateInstance<WindomAni2>();
        //ani.load(Path.Combine(folder, "robo.hod"));
        aniFile.load(Path.Combine(folder, "Script.ani"));

        BuildStructure(aniFile.structure);
        OutputStructurePrefab();
#if UNITY_EDITOR
        BuildAnimations();
#endif
    }

    public void BuildStructure(Hod2v0 Robo)
    {
        isTop.Clear();
        //find cypher
        string[] files = Directory.GetFiles(folder);
        foreach (string file in files)
        {
            if (transcoder.findCypher(file))
                break;
        }

        hod = Robo;


        //build Ani

        parts.Clear();

        if (root != null)
            GameObject.Destroy(root);

        for (int i = 0; i < Robo.parts.Count; i++)
        {
            GameObject part = new GameObject((Robo.parts[i].name).Replace(".x",""));
            if (i == 0)
            {
                root = part;
                part.transform.parent = transform;
            }
            parts.Add(part);

            if (i == 0)
            {
                parts[i].transform.localPosition = Robo.parts[i].position;
                parts[i].transform.localRotation = Robo.parts[i].rotation;
                parts[i].transform.localScale = Robo.parts[i].scale;
                isTop.Add(false);
            }
            else
            {
                //find next level higher in tree.
                for (int j = i - 1; j >= 0; j--)
                {
                    if (Robo.parts[i].treeDepth - 1 == Robo.parts[j].treeDepth)
                    {
                        if (j == 0)
                        {
                            parts[i].transform.SetParent(parts[0].transform);
                            parts[i].transform.localPosition = Robo.parts[i].position;
                            parts[i].transform.localRotation = Robo.parts[i].rotation;
                            parts[i].transform.localScale = Robo.parts[i].scale;
                        }
                        else
                        {
                            parts[i].transform.SetParent(parts[j].transform);
                            parts[i].transform.localPosition = Robo.parts[i].position;
                            parts[i].transform.localRotation = Robo.parts[i].rotation;
                            parts[i].transform.localScale = Robo.parts[i].scale;

                        }

                        if (parts[i].name == "Body" || isTop[j])
                            isTop.Add(true);
                        else
                            isTop.Add(false);

                        break;
                    }
                }
            }
        }

        for (int i = 0; i < Robo.parts.Count; i++)
        {
            try
            {
                if (i != 0)
                    ImportModelEncrypted(parts[i], Path.Combine(folder, Robo.parts[i].name.Split("\0")[0]));

                if (Robo.parts[i].name == "Body_d")
                {
                    MeshCollider mc = parts[i].AddComponent<MeshCollider>();
                    mc.sharedMesh = ImportModel(Path.Combine(folder, "Hit.x"));
                    mc.convex = true;
                    parts[i].layer = 7;

                }

                if (Robo.parts[i].name == "Body")
                {
                    MeshCollider mc = parts[i].AddComponent<MeshCollider>();
                    mc.sharedMesh = ImportModel(Path.Combine(folder, "HitUp.x"));
                    mc.convex = true;
                    parts[i].layer = 7;
                }
            }
            catch (Exception e) { 
                Debug.LogError($"ImportModelFail of Part: {Robo.parts[i].name}"); 
                Debug.LogException(e); 
            }
        }
    }

    public void OutputStructurePrefab()
    {
#if UNITY_EDITOR
        if (root == null) { Debug.LogError("Null Root"); return; }
        PrefabUtility.SaveAsPrefabAssetAndConnect(root,GetStructPath(),InteractionMode.AutomatedAction);
#endif
    }
    void BuildPaths()
    {
        partPaths = new string[aniFile.structure.parts.Count];
        for (int i = 0; i < aniFile.structure.parts.Count; i++)
        {
            if (i == 0)
            {
                partPaths[i] = "";
            }
            else
            {
                //find next level higher in tree.
                for (int j = i - 1; j >= 0; j--)
                {
                    if (aniFile.structure.parts[i].treeDepth - 1 == aniFile.structure.parts[j].treeDepth)
                    {
                        if (j == 0)
                        {
                            partPaths[i] = aniFile.structure.parts[i].name.Replace(".x", "");
                        }
                        else
                        {
                            partPaths[i] = partPaths[j] + "/" + aniFile.structure.parts[i].name.Replace(".x", "");
                        }
                        break;
                    }
                }
            }
            //Debug.Log(partPaths[i]);
        }

    }

#if UNITY_EDITOR
    void BuildAnimations()
    {
        BuildPaths();

        aniFile.ExportAnim(GetAniFolder());
    }
#endif

    void ImportModelEncrypted(GameObject GO, string file)
    {

        if (File.Exists(file))
        {
            try
            {
                string Modelpath = Path.GetDirectoryName(file);

                //var scen = Importer.ImportFile(file, Helper.PostProcessStepflags);
                byte[] data = transcoder.Transcode(file);
                MemoryStream ms = new MemoryStream(data);
                var scen = Importer.ImportFileFromStream(ms, Helper.PostProcessStepflags, "x");
                if (scen != null && scen.Meshes != null)
                {
                    Mesh mesh = new Mesh();
                    mesh.CombineMeshes(scen.Meshes.Select(x => new CombineInstance()
                    {
                        mesh = x.ToUnityMesh(),
                        transform = scen.RootNode.Transform.ToUnityMatrix()
                    }).ToArray(), false);
                    mesh.name = Path.GetFileNameWithoutExtension(file);
                    SaveMesh(ref mesh);

                    Material[] materials = new Material[scen.Meshes.Length];

                    for (int index = 0; index < materials.Length; index++)
                    {
                        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

                        int materialIndex = scen.Meshes[index].MaterialIndex;
                        if (scen.Materials[materialIndex] != null)
                        {
                            mat.name = $"{mesh.name}_{materialIndex}";
                            var textures = scen.Materials[materialIndex].GetAllTextures();
                            var color = scen.Materials[materialIndex].ColorDiffuse;
                            mat.color = new Color(color.R, color.G, color.B, color.A);
                            mat.SetFloat("_Glossiness", scen.Materials[materialIndex].ShininessStrength);

                            if(textures.Length > 0)
                            {
                                string texturePath = Path.Combine(Modelpath, textures[0].FilePath);
                                if (File.Exists(texturePath))
                                {
                                    try
                                    {
                                        Texture2D mainTexture = Helper.LoadTextureEncrypted(texturePath, ref transcoder);
                                        mainTexture.name = Path.GetFileName(texturePath);
                                        SaveTexture(ref mainTexture);
                                        mat.mainTexture = mainTexture;
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogException(e);
                                    }
                                }
                            }
                        }
                        SaveMaterial(ref mat);
                        materials[index] = mat;
                    }

                    GO.AddComponent<MeshFilter>().sharedMesh = mesh;
                    GO.AddComponent<MeshRenderer>().sharedMaterials = materials;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(file + "LoadFail");
                Debug.LogException(e);
            }
        }
    }

    Mesh ImportModel(string file)
    {
        if (File.Exists(file))
        {
            try
            {
                string Modelpath = Path.GetDirectoryName(file);

                var scen = Importer.ImportFile(file, Helper.PostProcessStepflags);
                Mesh mesh = new Mesh();
                mesh.CombineMeshes(scen.Meshes.Select(x => new CombineInstance()
                {
                    mesh = x.ToUnityMesh(),
                    transform = scen.RootNode.Transform.ToUnityMatrix()
                }).ToArray(), false);
                mesh.name = Path.GetFileName(file);
                SaveMesh(ref mesh);

                return mesh;
            }
            catch (Exception e)
            {
                Debug.LogError(file + "LoadFail");
                Debug.LogException(e);
            }
        }
        return null;
    }

    void SaveMaterial(ref Material material)
    {
#if UNITY_EDITOR
        string path = Path.Combine(GetMaterialFolder(), material.name + ".mat");
        if(!File.Exists(path)) 
            AssetDatabase.CreateAsset(material, path);
        material = AssetDatabase.LoadAssetAtPath<Material>(path);
#endif
    }

    void SaveMesh(ref Mesh mesh)
    {
#if UNITY_EDITOR
        string path = Path.Combine(GetMeshFolder(), mesh.name + ".asset");
        if(!File.Exists(path))
            AssetDatabase.CreateAsset(mesh, path);
        mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
#endif
    }

    void SaveTexture(ref Texture2D texture)
    {
#if UNITY_EDITOR
        string path = Path.Combine(GetTextureFolder(), texture.name + ".asset");
        if (!File.Exists(path))
            AssetDatabase.CreateAsset(texture, path);
        texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
#endif
    }

    void SaveAni(ref AnimationClip ani)
    {
#if UNITY_EDITOR
        string path = Path.Combine(GetAniFolder(), ani.name + ".anim");
        if (!File.Exists(path))
            AssetDatabase.CreateAsset(ani, path);
        ani = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
#endif
    }
}
