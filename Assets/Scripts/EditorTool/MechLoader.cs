using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MechLoader : MonoBehaviour
{
    public string MechaFolder;
    public GameObject Robo;

    public void loadMecha(string folder)
    {
        if (Robo == null) Robo = gameObject;
        MechStruct roboStructure = GetComponent<MechStruct>();
        roboStructure.folder = folder;
        roboStructure.transcoder = new CypherTranscoder();
        if (File.Exists(Path.Combine(folder, "Script.ani")))
        {
            roboStructure.buildStructure();
        }
        else
            Debug.Log("Missing Script Ani");
    }



}

