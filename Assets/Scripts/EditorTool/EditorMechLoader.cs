using System.IO;
using UnityEngine;

public class EditorMechLoader : MonoBehaviour
{
    public GameObject mechGo;
    public string folder = "Windom_Data\\Robo";
    public string mechName;
    private string path;
    [ContextMenu("LoadMech")]
    void LoadMech()
    {
        if (GetPath())
        {
            LoadAsset();
        }
        else
        {
            PathNoFoundError();
        }
    }

    private static void PathNoFoundError()
    {
        Debug.LogError("[LoadFailed]: FileNotFound");
    }

    private bool GetPath()
    {
        path = Path.Combine(folder, mechName);
        return Directory.Exists(path);
    }

    private void LoadAsset()
    {
        if (mechGo) DestroyImmediate(mechGo);
        mechGo = MechStruct.LoadMech(path, mechName);
    }
}
