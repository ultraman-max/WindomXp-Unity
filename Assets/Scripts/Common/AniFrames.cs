using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

[Serializable]
public struct AniFrames
{
    public string name;
    public string squirrelInit;
    public List<Hod2v1> frames;
    public List<WindomScript> scripts;

    public AniFrames(string name, string squirrelInit, List<Hod2v1> frames, List<WindomScript> scripts)
    {
        this.name = name;
        this.squirrelInit = squirrelInit;
        this.frames = frames;
        this.scripts = scripts;
    }

    public void loadFromAni(ref BinaryReader br, ref Hod2v0 structure)
    {
        frames = new List<Hod2v1>();
        scripts = new List<WindomScript>();

        //load Name
        //Encoding ShiftJis = Encoding.GetEncoding(932);
        long position = br.BaseStream.Position;
        try
        {
            name = USEncoder.ToEncoding.ToUnicode(br.ReadBytes(256)).TrimEnd('\0');
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            br.BaseStream.Seek(position + 256, SeekOrigin.Begin);
        }

        //Read Initial Script
        int textLength = br.ReadInt32();
        if (textLength != 0)
        {
            squirrelInit = USEncoder.ToEncoding.ToUnicode(br.ReadBytes(textLength));
        }

        //Read Hod Files
        int hodCount = br.ReadInt32();
        for (int i = 0; i < hodCount; i++)
        {
            short nameLength = br.ReadInt16();
            Hod2v1 nHod = new Hod2v1(USEncoder.ToEncoding.ToUnicode(br.ReadBytes(nameLength)));
            nHod.loadFromBinary(ref br, ref structure);
            frames.Add(nHod);
        }

        //Read Script Files
        int scriptCount = br.ReadInt32();
        for (int i = 0; i < scriptCount; i++)
        {
            WindomScript ns = new WindomScript();
            ns.unk = br.ReadInt32();
            ns.time = br.ReadSingle();
            textLength = br.ReadInt32();
            ns.squirrel = USEncoder.ToEncoding.ToUnicode(br.ReadBytes(textLength));
            scripts.Add(ns);
        }
    }

    public void loadFromAniOld(ref BinaryReader br)
    {
        frames = new List<Hod2v1>();
        scripts = new List<WindomScript>();

        //load Name
        //Encoding ShiftJis = Encoding.GetEncoding(932);
        name = USEncoder.ToEncoding.ToUnicode(br.ReadBytes(256)).TrimEnd('\0');

        //Read Hod Files
        int hodCount = br.ReadInt32();
        for (int i = 0; i < hodCount; i++)
        {
            Hod1 nHod = new Hod1(USEncoder.ToEncoding.ToUnicode(br.ReadBytes(30)).TrimEnd('\0'));
            nHod.loadFromBinary(ref br);
            frames.Add(nHod.convertToHod2v1());
        }

        //Read Script Files
        int scriptCount = br.ReadInt32();
        for (int i = 0; i < scriptCount; i++)
        {
            WindomScript ns = new WindomScript();
            ns.unk = br.ReadInt32();
            ns.time = br.ReadSingle();
            int textLength = br.ReadInt32();
            ns.squirrel = USEncoder.ToEncoding.ToUnicode(br.ReadBytes(textLength));
            scripts.Add(ns);
        }
    }

    public void saveToAni(ref BinaryWriter bw)
    {
        //Encoding ShiftJis = Encoding.GetEncoding(932);
        byte[] shiftjistext = USEncoder.ToEncoding.ToSJIS(name);
        bw.Write(shiftjistext);
        bw.BaseStream.Seek(256 - shiftjistext.Length, SeekOrigin.Current);
        shiftjistext = USEncoder.ToEncoding.ToSJIS(squirrelInit);
        bw.Write(shiftjistext.Length);
        if (shiftjistext.Length > 0)
            bw.Write(shiftjistext);
        bw.Write(frames.Count);

        for (int i = 0; i < frames.Count; i++)
        {
            frames[i].saveToBinary(ref bw);
        }

        bw.Write(scripts.Count);
        for (int i = 0; i < scripts.Count; i++)
        {
            bw.Write(scripts[i].unk);
            bw.Write(scripts[i].time);
            //shiftjistext = USEncoder.ToEncoding.ToSJIS(scripts[i].squirrel);
            //bw.Write(shiftjistext.Length);
            //if (shiftjistext.Length > 0)
            //    bw.Write(shiftjistext);
            List<byte> list = new List<byte>();
            list.AddRange(USEncoder.ToEncoding.ToSJIS(scripts[i].squirrel));
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j] == 0x0A && list[j - 1] != 0x0D)
                    list.Insert(j, 0x0D);
            }
            bw.Write(list.Count);
            bw.Write(list.ToArray());
        }
    }

    public Hod2v1_Part interpolatePart(int frame, int part, float time)
    {
        Hod2v1_Part iPart = new Hod2v1_Part();
        if (frame < frames.Count - 1)
        {
            iPart.position = Vector3.Lerp(frames[frame].parts[part].position, frames[frame + 1].parts[part].position, time);
            iPart.rotation = Quaternion.Lerp(frames[frame].parts[part].rotation, frames[frame + 1].parts[part].rotation, time);
            iPart.scale = Vector3.Lerp(frames[frame].parts[part].scale, frames[frame + 1].parts[part].scale, time);
        }
        else
        {
            iPart.position = frames[frames.Count - 1].parts[part].position;
            iPart.rotation = frames[frames.Count - 1].parts[part].rotation;
            iPart.scale = frames[frames.Count - 1].parts[part].scale;
        }
        return iPart;
    }
}
