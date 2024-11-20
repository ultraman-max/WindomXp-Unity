using System;
using UnityEngine;

[Serializable]
public struct WindomScript
{
    public int frameCount;
    public float aniSpeed;
    [Multiline(10)]
    public string squirrel;

    public float GetAniFrameLength()
    {
        return frameCount * aniSpeed;
    }
}
