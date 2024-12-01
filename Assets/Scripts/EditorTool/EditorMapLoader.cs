using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Assets;
using System.Linq;
using UnityDds;
using UnityEditor;

public class EditorMapLoader : MonoBehaviour
{
    public string mapPath = "E:\\Other Game\\WindomXP\\Tools\\Tools\\Windom Map Editor 0.2\\maps\\City";
    public PieceData[] pieces;
    public Assimp.AssimpImporter Importer = new Assimp.AssimpImporter();
    public Material baseMat;
    [HideInInspector]
    public List<string> scripts = new List<string>();
    //[HideInInspector]
    public GameObject mapObjects;
    //[HideInInspector]
    public GameObject HitArea;
    //[HideInInspector]
    public GameObject SkyMap;

    public Mpd map;

    CypherTranscoder transcoder = new CypherTranscoder();


    [ContextMenu("Load")]
    public void Load()
    {
        Helper.TextureCache.Clear();

        map = CreateMPD();

        pieces = new PieceData[map.Pieces.Count];
        scripts = map.scripts;

        for (int j = 0; j < pieces.Length; j++)
        {
            UpdatePiece(j, map.Pieces[j].visualMesh, map.Pieces[j].collisionMesh, map.Pieces[j].scriptText);
        }

        CreateMapObject(map);

        CreateHitArea();

        CreateSkyBox();

    }

    private Mpd CreateMPD()
    {
        string[] files = Directory.GetFiles(mapPath);
        foreach (string file in files)
        {
            if (transcoder.findCypher(file))
                break;
        }
        NewMapObjects();
        map = mapObjects.AddComponent<Mpd>();
        map.Load(Path.Combine(mapPath, "map.mpd"));
        return map;
    }

    private void CreateMapObject(Mpd map)
    {
        ObjectData od;
        int nextInstId = 0;
        for (int x = 0; x < map.WorldGrid.GetLength(0); x++)
        {
            for (int y = 0; y < map.WorldGrid.GetLength(1); y++)
            {
                Mpd_WorldGrid wg2 = map.WorldGrid[x, y];
                for (int gridInstId = 0; gridInstId < wg2.objects.Count; gridInstId++)
                {
                    Mpd_Object obj = wg2.objects[gridInstId];
                    if (pieces[obj.pieceID].visualMesh != null)
                    {
                        GameObject go = new GameObject(pieces[obj.pieceID].visualMesh.name);
                        od = go.AddComponent<ObjectData>();
                        od.instanceID = nextInstId++;
                        od.ModelID = obj.pieceID;
                        od.scriptID = obj.scriptIndex;
                        od.matrix = obj.transform;
                        od.gridInstId = gridInstId;
                        od.grid = new Vector2Int(x, y);
                        go.transform.SetParent(mapObjects.transform);
                        go.transform.localPosition = Utils.GetPosition(obj.transform);
                        go.transform.localRotation = Utils.GetRotation(obj.transform);
                        go.transform.localScale = Utils.GetScale(obj.transform);
                        BuildObject(od);
                    }

                }
            }
        }
    }

    private void NewMapObjects()
    {
        if (mapObjects != null)
        {
            DestroyImmediate(mapObjects);
        }
        mapObjects = new GameObject("Map");
    }

    private ObjectData CreateSkyBox()
    {
        ObjectData od;
        if (SkyMap) DestroyImmediate(SkyMap);
        SkyMap = new GameObject("Sky.x");
        od = SkyMap.AddComponent<ObjectData>();
        od.ModelID = 198;
        od.scriptID = -1;
        SkyMap.transform.localPosition = new Vector3(1500, 0, 1500);
        SkyMap.transform.localScale = Vector3.one * 3000;
        BuildObject(od);
        return od;
    }

    private ObjectData CreateHitArea()
    {
        ObjectData od;
        if (HitArea) DestroyImmediate(HitArea);
        HitArea = new GameObject("Hit.x");
        od = HitArea.AddComponent<ObjectData>();
        od.ModelID = 199;
        od.scriptID = -1;
        HitArea.transform.localPosition = new Vector3(1500, 0, 1500);
        BuildObject(od);
        return od;
    }

    public void BuildObject(ObjectData objectData)
    {
        int ModelID = objectData.ModelID;
        GameObject gameObject = objectData.gameObject;

        if (pieces[ModelID].visualMesh != null)
        {
            gameObject.name = pieces[ModelID].visualMesh.name;
            if (gameObject.GetComponent<MeshFilter>() == null)
                gameObject.AddComponent<MeshFilter>().mesh = pieces[ModelID].visualMesh;
            else
                gameObject.GetComponent<MeshFilter>().mesh = pieces[ModelID].visualMesh;

            if (gameObject.GetComponent<MeshRenderer>() == null)
                gameObject.AddComponent<MeshRenderer>().materials = pieces[ModelID].materials;
            else
                gameObject.GetComponent<MeshRenderer>().materials = pieces[ModelID].materials;

            if (pieces[ModelID].colliderMesh != null)
            {
                if (gameObject.GetComponent<MeshCollider>() == null)
                    gameObject.AddComponent<MeshCollider>().sharedMesh = pieces[ModelID].colliderMesh;
                else
                    gameObject.GetComponent<MeshCollider>().sharedMesh = pieces[ModelID].colliderMesh;
            }
        }
    }

    [ContextMenu("Save")]
    public void Save()
    {
        map.Save(Path.Combine(mapPath, "map.mpd"));
    }

    public void UpdatePiece(int index, string visualMesh, string collisionMesh, string scriptText)
    {
        if (visualMesh != "")
        {

            Mesh visualM = new Mesh();
            Mesh colliderM = new Mesh();
            Material[] mats = new Material[0];
            bool isAlpha = false;
            try
            {
                if (index == 199)
                    isAlpha = true;

                if (visualMesh != "")
                    ImportModel(Path.Combine(mapPath, visualMesh), ref visualM, ref mats, isAlpha);

                if (visualMesh == collisionMesh)
                { colliderM = visualM; Debug.Log("Equals Visual"); }
                if (collisionMesh != "")
                { colliderM = ImportModel(Path.Combine(mapPath, collisionMesh)); Debug.Log("New Mesh Loaded"); }
            }
            catch
            {
                Debug.Log("Invalid Load: " + visualMesh);
            }
            visualM.name = visualMesh;
            if (colliderM != null)
                colliderM.name = collisionMesh;

            pieces[index].visualMesh = visualM;
            pieces[index].colliderMesh = colliderM;
            if (index == 199)
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    Texture tex = mats[i].mainTexture;
                    mats[i] = new Material(baseMat.shader);
                    mats[i].mainTexture = tex;
                }
            }
            pieces[index].materials = mats;
            pieces[index].script = scriptText;
        }
    }

    void ImportModel(string file, ref Mesh mesh, ref Material[] materials, bool isAlpha = false)
    {
        Debug.Log(file);
        if (File.Exists(file))
        {
            Debug.Log("File Exists");
            try
            {
                string Modelpath = Path.GetDirectoryName(file);

                byte[] data = transcoder.Transcode(file);
                MemoryStream ms = new MemoryStream(data);
                var scen = Importer.ImportFileFromStream(ms, Helper.PostProcessStepflags, "x");
                mesh.CombineMeshes(scen.Meshes.Select(x => new CombineInstance()
                {
                    mesh = x.ToUnityMesh(),
                    transform = scen.RootNode.Transform.ToUnityMatrix()
                }).ToArray(), false);
                materials = new Material[scen.Meshes.Length];
                Debug.Log(materials.Length);
                for (int index = 0; index < materials.Length; index++)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));

                    if (scen.Materials[scen.Meshes[index].MaterialIndex] != null)
                    {
                        if (isAlpha)
                            mat.CopyPropertiesFromMaterial(baseMat);
                        mat.name = scen.Materials[scen.Meshes[index].MaterialIndex].Name;
                        var textures = scen.Materials[scen.Meshes[index].MaterialIndex].GetAllTextures();



                        if (textures.Length > 0 && File.Exists(Path.Combine(Modelpath, textures[0].FilePath)))
                        {
                            try
                            {
                                FileInfo f = new FileInfo(Path.Combine(Modelpath, textures[0].FilePath));
                                if (f.Extension == ".dds")
                                {
                                    mat.mainTexture = DdsTextureLoader.LoadTexture(f.FullName);
                                    mat.SetTextureScale("_MainTex", new Vector2(1, -1));
                                }
                                else
                                    mat.mainTexture = Helper.LoadTexture(f.FullName);
                            }
                            catch (System.Exception e ) { 
                                Debug.LogError(e);
                            }
                        }
                    }

                    materials[index] = mat;
                }
            }
            catch
            {
            }
        }
        else
            Debug.Log("File Doesn't Exist");
    }

    Mesh ImportModel(string file)
    {
        if (File.Exists(file))
        {
            try
            {
                string Modelpath = Path.GetDirectoryName(file);

                byte[] data = transcoder.Transcode(file);
                MemoryStream ms = new MemoryStream(data);
                var scen = Importer.ImportFileFromStream(ms, Helper.PostProcessStepflags, "x");
                Mesh mesh = new Mesh();
                mesh.CombineMeshes(scen.Meshes.Select(x => new CombineInstance()
                {
                    mesh = x.ToUnityMesh(),
                    transform = scen.RootNode.Transform.ToUnityMatrix()
                }).ToArray(), false);


                return mesh;
            }
            catch
            {
            }
        }
        return null;
    }
}
