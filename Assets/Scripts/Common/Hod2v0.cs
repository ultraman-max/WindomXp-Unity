using Assimp.Unmanaged;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public struct Hod2v0_Part
{
    public string name;
    public int treeDepth;
    public int childCount;
    public Quaternion rotation;
    public Vector3 scale;
    public Vector3 position;
    public byte flag;
    public Vector3 unk;
    [HideInInspector]
    public byte[] extraBytes;
}

[Serializable]
public struct Hod2v0
{
    public string filename;
    public List<Hod2v0_Part> parts;
    public Hod2v0(string name)
    {
        filename = name;
        parts = new List<Hod2v0_Part>();
    }

    public bool loadFromBinary(ref BinaryReader br)
    {
        string signature = new string(br.ReadChars(3));
        int version = br.ReadInt32();
        if (signature == "HD2" && version == 0)
        {
            parts = new List<Hod2v0_Part>();
            int partCount = br.ReadInt32();
            for (int i = 0; i < partCount; i++)
            {
                Hod2v0_Part nPart = new Hod2v0_Part();
                nPart.treeDepth = br.ReadInt32();
                nPart.childCount = br.ReadInt32();
                nPart.name = Utils.TrimEnd(ASCIIEncoding.ASCII.GetString(br.ReadBytes(256)));
                nPart.rotation = new Quaternion();
                nPart.rotation.x = br.ReadSingle();
                nPart.rotation.y = br.ReadSingle();
                nPart.rotation.z = br.ReadSingle();
                nPart.rotation.w = br.ReadSingle();
                nPart.scale = new Vector3();
                nPart.scale.x = br.ReadSingle();
                nPart.scale.y = br.ReadSingle();
                nPart.scale.z = br.ReadSingle();
                nPart.position = new Vector3();
                nPart.position.x = br.ReadSingle();
                nPart.position.y = br.ReadSingle();
                nPart.position.z = br.ReadSingle();
                nPart.flag = br.ReadByte();
                nPart.unk = new Vector3();
                nPart.unk.x = br.ReadSingle();
                nPart.unk.y = br.ReadSingle();
                nPart.unk.z = br.ReadSingle();
                nPart.extraBytes = br.ReadBytes(82); // 保存额外的82个字节
                parts.Add(nPart);
            }
        }
        else
            return false;

        return true;
    }

    public void saveToBinary(ref BinaryWriter bw)
    {
        bw.Write(ASCIIEncoding.ASCII.GetBytes("HD2"));
        bw.Write(0);
        bw.Write(parts.Count);
        for (int i = 0; i < parts.Count; i++)
        {
            bw.Write(parts[i].treeDepth);
            bw.Write(parts[i].childCount);
            byte[] text = ASCIIEncoding.ASCII.GetBytes(parts[i].name);
            bw.Write(text);
            bw.BaseStream.Seek(256 - text.Length, SeekOrigin.Current);
            bw.Write(parts[i].rotation.x);
            bw.Write(parts[i].rotation.y);
            bw.Write(parts[i].rotation.z);
            bw.Write(parts[i].rotation.w);
            bw.Write(parts[i].scale.x);
            bw.Write(parts[i].scale.y);
            bw.Write(parts[i].scale.z);
            bw.Write(parts[i].position.x);
            bw.Write(parts[i].position.y);
            bw.Write(parts[i].position.z);
            bw.Write(parts[i].flag);
            bw.Write(parts[i].unk.x);
            bw.Write(parts[i].unk.y);
            bw.Write(parts[i].unk.z);
            bw.Write(parts[i].extraBytes);
            //bw.BaseStream.Seek(82, SeekOrigin.Current);
        }
    }

    public void buildPaths(out string[] partPaths)
    {
        partPaths = new string[parts.Count];
        for (int i = 0; i < parts.Count; i++)
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
                    if (parts[i].treeDepth - 1 == parts[j].treeDepth)
                    {
                        if (j == 0)
                        {
                            partPaths[i] = parts[i].name.Replace(".x", "");
                        }
                        else
                        {
                            partPaths[i] = partPaths[j] + "/" + parts[i].name.Replace(".x", "");
                        }
                        break;
                    }
                }
            }
            //Debug.Log(partPaths[i]);.Replace(".x", "")
        }

    }

}

