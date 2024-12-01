using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "WindomAni2", menuName = "ScriptableObjects/WindomAni2", order = 1)]
public class WindomAni2 : ScriptableObject
{
    public string fileName;
    public Hod2v0 structure;
    public List<AniFrames> animations;
    public int selectedAnimation;

    [ContextMenu("LoadFile")]
    public void LoadFile()
    {
        load(fileName);
    }

    [ContextMenu("SaveFile")]
    public void SaveFile()
    {
        save(fileName); 
    }


    [ContextMenu("Test")]
    public void Test()
    {
        foreach (var item in animations)
        {
            if(item.outputAnim)
                for (int i = 0; i < item.scripts.Count; i++)
            {
                WindomScript script = item.scripts[i];
                if (script.squirrel != null)
                {
                        script.squirrel=script.squirrel.Replace("WeaponAttack2(0,24,\t2,20,30,130,12,27,0,0,1,0);", "WeaponAttack2(0,24,\t2,20,30,130,13,27,0,0,1,0);");
                        item.scripts[i] = script;
                        
                        //if (script.squirrel.Contains("90"))
                        //{
                        //    Debug.Log(item.name);
                        //    Debug.Log(script.squirrel);
                        //}
                }
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("ExportAnim")]
    public void ExportAnim()
    {
        structure.buildPaths(out string[] path);

        foreach (var item in animations)
        {
            if (item.outputAnim)
            {
                item.BuildAnimations(path);
            }
        }
    }

    public void ExportAnim(string savePath)
    {
        structure.buildPaths(out string[] path);

        foreach (var item in animations)
        {
            item.BuildAnimations(path,savePath);
        }
    }
#endif
    public bool load(string filename)
    {
        fileName = filename;
        BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read));

        string signature = new string(br.ReadChars(3));
        if (signature == "AN2")
        {
            animations = new List<AniFrames>();
            //Encoding ShiftJis = Encoding.GetEncoding(932);
            string robohod = USEncoder.ToEncoding.ToUnicode(br.ReadBytes(256)).TrimEnd('\0');
            robohod = robohod.Replace("\0", "$$");
            structure = new Hod2v0(robohod);
            structure.loadFromBinary(ref br);

            int aCount = br.ReadInt32();
            for (int i = 0; i < aCount; i++)
            {
                AniFrames aData = new AniFrames();
                aData.loadFromAni(ref br, ref structure);
                animations.Add(aData);
            }
        }
        else if (signature == "ANI")
        {
            StreamWriter debug = new StreamWriter("debug.txt");
            animations = new List<AniFrames>();
            //Encoding ShiftJis = Encoding.GetEncoding(932);
            string robohod = USEncoder.ToEncoding.ToUnicode(br.ReadBytes(256)).TrimEnd('\0');
            Hod1 oldStructure = new Hod1(robohod);
            oldStructure.loadFromBinary(ref br);
            structure = oldStructure.convertToHod2v0();
            debug.WriteLine(br.BaseStream.Position.ToString());
            debug.Close();
            for (int i = 0; i < 200; i++)
            {
                AniFrames aData = new AniFrames();
                aData.loadFromAniOld(ref br);
                animations.Add(aData);
            }
        }
        else if (signature == "HOD")
        {
            br.BaseStream.Seek(0, SeekOrigin.Begin);
            animations = new List<AniFrames>();
            Hod1 hodfile = new Hod1("HOD1 FILE");
            hodfile.loadFromBinary(ref br);
            structure = hodfile.convertToHod2v0();
            AniFrames aData = new AniFrames();
            aData.frames = new List<Hod2v1>
            {
                hodfile.convertToHod2v1()
            };
            aData.scripts = new List<WindomScript>();
            animations.Add(aData);
        }
        br.Close();

        return true;
    }

    public void save(string filename = "")
    {
        if (filename == "")
            filename = fileName;
        //Encoding ShiftJis = Encoding.GetEncoding(932);
        if (File.Exists(filename))
            File.Delete(filename);
        BinaryWriter bw = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite));
        bw.Write(ASCIIEncoding.ASCII.GetBytes("AN2"));
        byte[] shiftjistext = USEncoder.ToEncoding.ToSJIS(structure.filename.Replace("$$","\0"));
        bw.Write(shiftjistext);
        bw.BaseStream.Seek(256 - shiftjistext.Length, SeekOrigin.Current);
        structure.saveToBinary(ref bw);

        bw.Write(animations.Count);
        for (int i = 0; i < animations.Count; i++)
        {
            animations[i].saveToAni(ref bw);
        }

        bw.Close();
    }

    public void addPart(string partName, int parent)
    {
        //Debug.Log(structure.parts.Count);
        int level = structure.parts[parent].treeDepth + 1;
        Hod2v0_Part pHod = structure.parts[parent];
        pHod.childCount++;
        structure.parts[parent] = pHod;
        Hod2v0_Part nPart = new Hod2v0_Part();
        nPart.name = partName;
        nPart.treeDepth = level;
        nPart.flag = 1;
        nPart.unk = new Vector3(1, 1, 1);
        nPart.position = new Vector3(0, 0, 0);
        nPart.rotation = new Quaternion(0, 0, 0, 1);
        nPart.scale = new Vector3(1, 1, 1);
        int i = parent + 1;
        for (; i < structure.parts.Count; i++)
        {
            if (structure.parts[i].treeDepth <= structure.parts[parent].treeDepth)
            {
                break;
            }
        }
        structure.parts.Insert(i, nPart);
        //Debug.Log(structure.parts.Count);
        //Debug.Log(partName);
        Hod2v1_Part nPart1 = new Hod2v1_Part();
        nPart1.name = partName;
        nPart1.treeDepth = level;
        nPart1.position = new Vector3(0, 0, 0);
        nPart1.rotation = new Quaternion(0, 0, 0, 1);
        nPart1.scale = new Vector3(1, 1, 1);
        nPart1.unk1 = new Quaternion();
        nPart1.unk2 = new Quaternion();
        nPart1.unk3 = new Quaternion();
        for (int j = 0; j < animations.Count; j++)
        {
            for (int k = 0; k < animations[j].frames.Count; k++)
            {
                Hod2v1_Part pHod1 = animations[j].frames[k].parts[parent];
                pHod1.childCount++;
                animations[j].frames[k].parts[parent] = pHod1;
                animations[j].frames[k].parts.Insert(i, nPart1);
            }
        }
    }

    public bool removePart(int index)
    {
        if (structure.parts[index].childCount == 0)
        {
            int i = index;
            for (; i >= 0; i--)
            {
                if (structure.parts[i].treeDepth < structure.parts[index].treeDepth)
                {
                    Hod2v0_Part pHod = structure.parts[i];
                    pHod.childCount--;
                    structure.parts[i] = pHod;
                    structure.parts.RemoveAt(index);
                    break;
                }
            }

            for (int j = 0; j < animations.Count; j++)
            {
                for (int k = 0; k < animations[j].frames.Count; k++)
                {
                    Hod2v1_Part pHod1 = animations[j].frames[k].parts[i];
                    pHod1.childCount--;
                    animations[j].frames[k].parts[i] = pHod1;
                    animations[j].frames[k].parts.RemoveAt(index);
                }
            }

        }
        else
            return false;
        return true;
    }
}

