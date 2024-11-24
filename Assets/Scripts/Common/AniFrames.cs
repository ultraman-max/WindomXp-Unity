using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Assimp.Unmanaged;
using UnityEditor;
using System.Diagnostics.Eventing.Reader;

[Serializable]
public struct AniFrames
{
    public string name;
    public string squirrelInit;
    [HideInInspector]
    public List<Hod2v1> frames;
    public List<WindomScript> scripts;

    public bool outputAnim;

    public AniFrames(string name, string squirrelInit, List<Hod2v1> frames, List<WindomScript> scripts)
    {
        this.name = name;
        this.squirrelInit = squirrelInit;
        this.frames = frames;
        this.scripts = scripts;
        outputAnim = false;
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
        else
        {
            squirrelInit = "";
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
            ns.frameCount = br.ReadInt32();
            ns.aniSpeed = br.ReadSingle();
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
            ns.frameCount = br.ReadInt32();
            ns.aniSpeed = br.ReadSingle();
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
            bw.Write(scripts[i].frameCount);
            bw.Write(scripts[i].aniSpeed);
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

    public Hod2v1_Part interpolatePart(float frame, int part)
    {
        if(frames.Count == 0)
        {
            return new Hod2v1_Part();
        }
        if (frame <= 0)
        {
            return frames[0].parts[part];
        }
        if (frame >= frames.Count - 1)
        {
            return frames[frames.Count - 1].parts[part];
        }
        int iFrame = (int)frame;
        float time = frame - iFrame;
        return Hod2v1_Part.interpolatePart(frames[iFrame].parts[part], frames[iFrame + 1].parts[part], time);
    }

#if UNITY_EDITOR
    public void BuildAnimations(string[] partPaths, string savePath = "Assets/")
    {
        if( frames.Count==0)return;
        AnimationClip nClip = new AnimationClip();
        nClip.name = name;

        for (int i = 0; i < partPaths.Length; i++)
        {
            int gameFrameSoFar = 0;
            float aniFrameSoFar = 0.0f;
            float fps = 60;

            AnimationCurve RotX = new AnimationCurve();
            AnimationCurve RotY = new AnimationCurve();
            AnimationCurve RotZ = new AnimationCurve();
            AnimationCurve RotW = new AnimationCurve();

            AnimationCurve ScaleX = new AnimationCurve();
            AnimationCurve ScaleY = new AnimationCurve();
            AnimationCurve ScaleZ = new AnimationCurve();

            AnimationCurve PosX = new AnimationCurve();
            AnimationCurve PosY = new AnimationCurve();
            AnimationCurve PosZ = new AnimationCurve();


            var curFrameAni = frames[0].parts[i];

            RotX.AddKey(0, curFrameAni.rotation.x);
            RotY.AddKey(0, curFrameAni.rotation.y);
            RotZ.AddKey(0, curFrameAni.rotation.z);
            RotW.AddKey(0, curFrameAni.rotation.w);

            ScaleX.AddKey(0, curFrameAni.scale.x);
            ScaleY.AddKey(0, curFrameAni.scale.y);
            ScaleZ.AddKey(0, curFrameAni.scale.z);

            PosX.AddKey(0, curFrameAni.position.x);
            PosY.AddKey(0, curFrameAni.position.y);
            PosZ.AddKey(0, curFrameAni.position.z);


            if (scripts.Count == 0) {
                for (int h = 1; h < frames.Count; h++)
                {
                    float time = h/6f;  //fix speed for no script ani, not exact speed
                    RotX.AddKey(time, frames[h].parts[i].rotation.x);
                    RotY.AddKey(time, frames[h].parts[i].rotation.y);
                    RotZ.AddKey(time, frames[h].parts[i].rotation.z);
                    RotW.AddKey(time, frames[h].parts[i].rotation.w);

                    ScaleX.AddKey(time, frames[h].parts[i].scale.x);
                    ScaleY.AddKey(time, frames[h].parts[i].scale.y);
                    ScaleZ.AddKey(time, frames[h].parts[i].scale.z);

                    PosX.AddKey(time, frames[h].parts[i].position.x);
                    PosY.AddKey(time, frames[h].parts[i].position.y);
                    PosZ.AddKey(time, frames[h].parts[i].position.z);
                }
            }
            else
            {
                for (int j = 0; j < scripts.Count; j++)
                {
                    var script = scripts[j];
                    float aniFrameCurSpt = script.GetAniFrameLength();

                    if(aniFrameCurSpt == 0.0f) continue;

                    float aniFramePrevSpt = aniFrameSoFar;
                    float aniFrameNextSpt = aniFrameSoFar + aniFrameCurSpt;
                    int gameFramePrevSpt = gameFrameSoFar;
                    int gameFrameNextSpt = gameFrameSoFar + script.frameCount;

                    for (int k = 0; k < frames.Count - 1; k++)
                    {
                        if (k <= aniFramePrevSpt || k >= aniFrameNextSpt) continue;

                        float aniSpeed = script.aniSpeed;

                        float gameFrameCountCurAniFrame = gameFramePrevSpt + (k - aniFramePrevSpt) / aniSpeed;

                        float keyFrameTime = gameFrameCountCurAniFrame / fps;

                        RotX.AddKey(keyFrameTime, frames[k].parts[i].rotation.x);
                        RotY.AddKey(keyFrameTime, frames[k].parts[i].rotation.y);
                        RotZ.AddKey(keyFrameTime, frames[k].parts[i].rotation.z);
                        RotW.AddKey(keyFrameTime, frames[k].parts[i].rotation.w);

                        ScaleX.AddKey(keyFrameTime, frames[k].parts[i].scale.x);
                        ScaleY.AddKey(keyFrameTime, frames[k].parts[i].scale.y);
                        ScaleZ.AddKey(keyFrameTime, frames[k].parts[i].scale.z);

                        PosX.AddKey(keyFrameTime, frames[k].parts[i].position.x);
                        PosY.AddKey(keyFrameTime, frames[k].parts[i].position.y);
                        PosZ.AddKey(keyFrameTime, frames[k].parts[i].position.z);
                    }

                    aniFrameSoFar += aniFrameCurSpt;
                    gameFrameSoFar += script.frameCount;

                    curFrameAni = interpolatePart(aniFrameSoFar, i);
                    float curFrameTime = gameFrameSoFar / fps;

                    RotX.AddKey(curFrameTime, curFrameAni.rotation.x);
                    RotY.AddKey(curFrameTime, curFrameAni.rotation.y);
                    RotZ.AddKey(curFrameTime, curFrameAni.rotation.z);
                    RotW.AddKey(curFrameTime, curFrameAni.rotation.w);

                    ScaleX.AddKey(curFrameTime, curFrameAni.scale.x);
                    ScaleY.AddKey(curFrameTime, curFrameAni.scale.y);
                    ScaleZ.AddKey(curFrameTime, curFrameAni.scale.z);

                    PosX.AddKey(curFrameTime, curFrameAni.position.x);
                    PosY.AddKey(curFrameTime, curFrameAni.position.y);
                    PosZ.AddKey(curFrameTime, curFrameAni.position.z);
                }
            }

            //load curves into clip
            nClip.SetCurve(partPaths[i], typeof(Transform), "localRotation.x", RotX);
            nClip.SetCurve(partPaths[i], typeof(Transform), "localRotation.y", RotY);
            nClip.SetCurve(partPaths[i], typeof(Transform), "localRotation.z", RotZ);
            nClip.SetCurve(partPaths[i], typeof(Transform), "localRotation.w", RotW);

            nClip.SetCurve(partPaths[i], typeof(Transform), "localScale.x", ScaleX);
            nClip.SetCurve(partPaths[i], typeof(Transform), "localScale.y", ScaleY);
            nClip.SetCurve(partPaths[i], typeof(Transform), "localScale.z", ScaleZ);

            nClip.SetCurve(partPaths[i], typeof(Transform), "localPosition.x", PosX);
            nClip.SetCurve(partPaths[i], typeof(Transform), "localPosition.y", PosY);
            nClip.SetCurve(partPaths[i], typeof(Transform), "localPosition.z", PosZ);
        }

        if (nClip.name != "")
        {
            try
            {

                AssetDatabase.CreateAsset(nClip, Path.Combine(savePath,nClip.name.Replace(':','_') + ".anim"));
                AssetDatabase.SaveAssets();
            }
            catch
            {
                Debug.Log("Error in Creating Clip");
            }
        }
    }
#endif
}
