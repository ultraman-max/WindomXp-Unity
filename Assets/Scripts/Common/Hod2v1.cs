using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

[Serializable]
public struct Hod2v1_Part
{
    public string name;
    public int treeDepth;
    public int childCount;
    public Quaternion rotation;
    public Vector3 scale;
    public Vector3 position;
    public Quaternion unk1;
    public Quaternion unk2;
    public Quaternion unk3;
    [HideInInspector]
    public byte[] extraBytes;

    Hod2v1_Part(Hod2v1_Part copy)
    {
        name = copy.name;
        treeDepth = copy.treeDepth;
        childCount = copy.childCount;
        rotation = copy.rotation;
        scale = copy.scale;
        position = copy.position;
        unk1 = copy.unk1;
        unk2 = copy.unk2;
        unk3 = copy.unk3;
        extraBytes = copy.extraBytes;
    }

    public static Hod2v1_Part interpolatePart(in Hod2v1_Part prevFrame, in Hod2v1_Part nextFrame, float ratio)
    {
        if (ratio <= 0)
        {
            return new Hod2v1_Part(prevFrame);
        }
        else if (ratio >= 1)
        {
            return new Hod2v1_Part(nextFrame);
        }

        Hod2v1_Part iPart = new Hod2v1_Part()
        {
            position = Vector3.Lerp(prevFrame.position, nextFrame.position, ratio),
            rotation = Quaternion.Lerp(prevFrame.rotation, nextFrame.rotation, ratio),
            scale = Vector3.Lerp(prevFrame.scale, nextFrame.scale, ratio),
        };

        return iPart;
    }
}

[Serializable]
public struct Hod2v1
{
    public string filename;
    //public byte[] data;
    public List<Hod2v1_Part> parts;
    public Hod2v1(string name)
    {
        filename = name;
        parts = new List<Hod2v1_Part>();
    }

    public bool loadFromBinary(ref BinaryReader br, ref Hod2v0 structure)
    {
        string signature = new string(br.ReadChars(3));
        int version = br.ReadInt32();
        if (signature == "HD2" && version == 1)
        {
            parts = new List<Hod2v1_Part>();
            int partCount = br.ReadInt32();
            for (int i = 0; i < partCount; i++)
            {
                Hod2v1_Part nPart = new Hod2v1_Part();
                nPart.name = structure.parts[i].name;
                nPart.treeDepth = br.ReadInt32();
                nPart.childCount = br.ReadInt32();
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
                nPart.unk1 = new Quaternion();
                nPart.unk1.x = br.ReadSingle();
                nPart.unk1.y = br.ReadSingle();
                nPart.unk1.z = br.ReadSingle();
                nPart.unk1.w = br.ReadSingle();
                nPart.unk2 = new Quaternion();
                nPart.unk2.x = br.ReadSingle();
                nPart.unk2.y = br.ReadSingle();
                nPart.unk2.z = br.ReadSingle();
                nPart.unk2.w = br.ReadSingle();
                nPart.unk3 = new Quaternion();
                nPart.unk3.x = br.ReadSingle();
                nPart.unk3.y = br.ReadSingle();
                nPart.unk3.z = br.ReadSingle();
                nPart.unk3.w = br.ReadSingle();
                //nPart.unk1 = Quaternion.Slerp(nPart.rotation, nPart.unk1, 2f);
                //nPart.unk2 = Quaternion.Slerp(nPart.rotation, nPart.unk2, 2f);
                //nPart.unk3 = Quaternion.Slerp(nPart.rotation, nPart.unk3, 2f);
                nPart.extraBytes = br.ReadBytes(83);
                parts.Add(nPart);
            }
        }
        else
            return false;

        return true;
    }

    public void saveToBinary(ref BinaryWriter bw)
    {
        //Encoding ShiftJis = Encoding.GetEncoding(932);
        byte[] shiftjistext = USEncoder.ToEncoding.ToSJIS(filename);
        bw.Write((short)shiftjistext.Length);
        bw.Write(shiftjistext);
        //bw.Write(data);
        bw.Write(ASCIIEncoding.ASCII.GetBytes("HD2"));
        bw.Write(1);
        bw.Write(parts.Count);
        for (int i = 0; i < parts.Count; i++)
        {
            bw.Write(parts[i].treeDepth);
            bw.Write(parts[i].childCount);
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
            bw.Write(parts[i].unk1.x);
            bw.Write(parts[i].unk1.y);
            bw.Write(parts[i].unk1.z);
            bw.Write(parts[i].unk1.w);
            bw.Write(parts[i].unk2.x);
            bw.Write(parts[i].unk2.y);
            bw.Write(parts[i].unk2.z);
            bw.Write(parts[i].unk2.w);
            bw.Write(parts[i].unk3.x);
            bw.Write(parts[i].unk3.y);
            bw.Write(parts[i].unk3.z);
            bw.Write(parts[i].unk3.w);
            bw.Write(parts[i].extraBytes);
            //bw.BaseStream.Seek(83, SeekOrigin.Current);
        }
    }
}

