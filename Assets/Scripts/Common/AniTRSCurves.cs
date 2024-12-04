using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AniTRSCurves
{
    AnimationCurve PosX;
    AnimationCurve PosY;
    AnimationCurve PosZ;
    AnimationCurve RotX;
    AnimationCurve RotY;
    AnimationCurve RotZ;
    AnimationCurve RotW;
    AnimationCurve ScaleX;
    AnimationCurve ScaleY;
    AnimationCurve ScaleZ;

    public AniTRSCurves()
    {
        PosX = new AnimationCurve();
        PosY = new AnimationCurve();
        PosZ = new AnimationCurve();
        RotX = new AnimationCurve();
        RotY = new AnimationCurve();
        RotZ = new AnimationCurve();
        RotW = new AnimationCurve();
        ScaleX = new AnimationCurve();
        ScaleY = new AnimationCurve();
        ScaleZ = new AnimationCurve();
    }


    public void AddKeyFrame(float time, Hod2v1_Part hod2V1)
    {
        AddKeyFrame(time, hod2V1.position, hod2V1.rotation, hod2V1.scale);
    }

    public void AddKeyFrame(float time, Vector3 pos, Quaternion rot, Vector3 scale)
    {
        Keyframe posX = new Keyframe(time, pos.x, 0, 0);
        PosX.AddKey(posX);
        Keyframe posY = new Keyframe(time, pos.y, 0, 0);
        PosY.AddKey(posY);
        Keyframe posZ = new Keyframe(time, pos.z, 0, 0);
        PosZ.AddKey(posZ);
        Keyframe rotX = new Keyframe(time, rot.x, 0, 0);
        RotX.AddKey(rotX);
        Keyframe rotY = new Keyframe(time, rot.y, 0, 0);
        RotY.AddKey(rotY);
        Keyframe rotZ = new Keyframe(time, rot.z, 0, 0);
        RotZ.AddKey(rotZ);
        Keyframe rotW = new Keyframe(time, rot.w, 0, 0);
        RotW.AddKey(rotW);
        Keyframe scaleX = new Keyframe(time, scale.x, 0, 0);
        ScaleX.AddKey(scaleX);
        Keyframe scaleY = new Keyframe(time, scale.y, 0, 0);
        ScaleY.AddKey(scaleY);
        Keyframe scaleZ = new Keyframe(time, scale.z, 0, 0);
        ScaleZ.AddKey(scaleZ);
    }

    public static void CopyKey(AniTRSCurves fromCurve,AniTRSCurves toCurve,float fromTime,float toTime)
    {
        CopyKey(fromCurve.PosX, toCurve.PosX, fromTime, toTime);
        CopyKey(fromCurve.PosY, toCurve.PosY, fromTime, toTime);
        CopyKey(fromCurve.PosZ, toCurve.PosZ, fromTime, toTime);
        CopyKey(fromCurve.RotX, toCurve.RotX, fromTime, toTime);
        CopyKey(fromCurve.RotY, toCurve.RotY, fromTime, toTime);
        CopyKey(fromCurve.RotZ, toCurve.RotZ, fromTime, toTime);
        CopyKey(fromCurve.RotW, toCurve.RotW, fromTime, toTime);
        CopyKey(fromCurve.ScaleX, toCurve.ScaleX, fromTime, toTime);
        CopyKey(fromCurve.ScaleY, toCurve.ScaleY, fromTime, toTime);
        CopyKey(fromCurve.ScaleZ, toCurve.ScaleZ, fromTime, toTime);
    }

    static void CopyKey(AnimationCurve fromCurve, AnimationCurve toCurve, float fromTime, float toTime)
    {
        var key = fromCurve.Evaluate(fromTime);
        toCurve.AddKey(toTime, key);
    }

    public void MoveKeyFrame(float oldTime,float newTime)
    {
        MoveKeyFrame(PosX, oldTime, newTime);
        MoveKeyFrame(PosY, oldTime, newTime);
        MoveKeyFrame(PosZ, oldTime, newTime);
        MoveKeyFrame(RotX, oldTime, newTime);
        MoveKeyFrame(RotY, oldTime, newTime);
        MoveKeyFrame(RotZ, oldTime, newTime);
        MoveKeyFrame(RotW, oldTime, newTime);
        MoveKeyFrame(ScaleX, oldTime, newTime);
        MoveKeyFrame(ScaleY, oldTime, newTime);
        MoveKeyFrame(ScaleZ, oldTime, newTime);
    }

    private void MoveKeyFrame(AnimationCurve curve, float oldTime, float newTime)
    {
        Keyframe[] keys = curve.keys;
        var key = curve.Evaluate(oldTime);
        curve.AddKey(newTime, key);
    }

    public void SetToAnimClip(AnimationClip animClip, string partPaths)
    {
        animClip.SetCurve(partPaths, typeof(Transform), "localRotation.x", RotX);
        animClip.SetCurve(partPaths, typeof(Transform), "localRotation.y", RotY);
        animClip.SetCurve(partPaths, typeof(Transform), "localRotation.z", RotZ);
        animClip.SetCurve(partPaths, typeof(Transform), "localRotation.w", RotW);

        animClip.SetCurve(partPaths, typeof(Transform), "localScale.x", ScaleX);
        animClip.SetCurve(partPaths, typeof(Transform), "localScale.y", ScaleY);
        animClip.SetCurve(partPaths, typeof(Transform), "localScale.z", ScaleZ);

        animClip.SetCurve(partPaths, typeof(Transform), "localPosition.x", PosX);
        animClip.SetCurve(partPaths, typeof(Transform), "localPosition.y", PosY);
        animClip.SetCurve(partPaths, typeof(Transform), "localPosition.z", PosZ);
    }

    public Vector3 GetPositionAtTime(float time)
    {
        return new Vector3(PosX.Evaluate(time), PosY.Evaluate(time), PosZ.Evaluate(time));
    }

    public Quaternion GetRotationAtTime(float time)
    {
        return new Quaternion(RotX.Evaluate(time), RotY.Evaluate(time), RotZ.Evaluate(time), RotW.Evaluate(time));
    }

    public Vector3 GetScaleAtTime(float time)
    {
        return new Vector3(ScaleX.Evaluate(time), ScaleY.Evaluate(time), ScaleZ.Evaluate(time));
    }
}
