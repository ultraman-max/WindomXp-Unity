using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System;

[Serializable]
public struct Mpd_Piece
{
    public string visualMesh;
    public string collisionMesh;
    public string scriptText;
}

[Serializable]
public struct Mpd_Object
{
    public Matrix4x4 transform;
    public short pieceID;
    //public string scriptText;
    public int scriptIndex;
}

public class Mpd_WorldGrid
{
    public List<Mpd_Object> objects;
}

public class Mpd:MonoBehaviour
{
    public List<Mpd_Piece> Pieces;
    public Mpd_WorldGrid[,] WorldGrid;
    public List<string> scripts;
    public float gridSize = 0; //grid size
    public int x;
    public int y;
    public int worldParts;

    public bool Load(string filename)
    {
        scripts = new List<string>();
        BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read));
        string signature = new string(br.ReadChars(3));
        if (signature == "MPD")
        {
            worldParts = br.ReadInt32();
            int PiecesCount = br.ReadInt16();
            Debug.Log(PiecesCount);
            Pieces = new List<Mpd_Piece>();
            for (int i = 0; i < PiecesCount; i++)
            {
                Mpd_Piece Piece = new Mpd_Piece();
                byte[] txt = br.ReadBytes(256);
                //scan
                int b = 0;
                for (; b < txt.Length; b++)
                {
                    if (txt[b] == 0)
                        break;
                }
                byte[] txt2 = new byte[b];
                System.Array.Copy(txt, 0, txt2, 0, b);
                Piece.visualMesh = USEncoder.ToEncoding.ToUnicode(txt2).TrimEnd('\0');
                txt = br.ReadBytes(256);
                //scan
                b = 0;
                for (; b < txt.Length; b++)
                {
                    if (txt[b] == 0)
                        break;
                }
                txt2 = new byte[b];
                System.Array.Copy(txt, 0, txt2, 0, b);
                Piece.collisionMesh = USEncoder.ToEncoding.ToUnicode(txt2).TrimEnd('\0');
                br.BaseStream.Seek(3, SeekOrigin.Current);
                int txtCount = br.ReadInt32();
                Piece.scriptText = USEncoder.ToEncoding.ToUnicode(br.ReadBytes(txtCount));
                Pieces.Add(Piece);
            }

            Debug.Log(br.BaseStream.Position.ToString());
            x = br.ReadInt16();
            y = br.ReadInt16();
            WorldGrid = new Mpd_WorldGrid[100, 100];
            gridSize = (float)br.ReadSingle();

            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    int ReadCount = br.ReadInt32();
                    WorldGrid[i, j] = new Mpd_WorldGrid();
                    WorldGrid[i, j].objects = new List<Mpd_Object>();
                    for (int k = 0; k < ReadCount; k++)
                    {
                        Mpd_Object obj = new Mpd_Object();
                        obj.transform = new Matrix4x4();
                        obj.transform.m00 = br.ReadSingle();
                        obj.transform.m10 = br.ReadSingle();
                        obj.transform.m20 = br.ReadSingle();
                        obj.transform.m30 = br.ReadSingle();
                        obj.transform.m01 = br.ReadSingle();
                        obj.transform.m11 = br.ReadSingle();
                        obj.transform.m21 = br.ReadSingle();
                        obj.transform.m31 = br.ReadSingle();
                        obj.transform.m02 = br.ReadSingle();
                        obj.transform.m12 = br.ReadSingle();
                        obj.transform.m22 = br.ReadSingle();
                        obj.transform.m32 = br.ReadSingle();
                        obj.transform.m03 = br.ReadSingle();
                        obj.transform.m13 = br.ReadSingle();
                        obj.transform.m23 = br.ReadSingle();
                        obj.transform.m33 = br.ReadSingle();
                        obj.pieceID = br.ReadInt16();
                        int rCount = br.ReadInt32();
                        string script = USEncoder.ToEncoding.ToUnicode(br.ReadBytes(rCount));
                        if (scripts.Contains(script))
                            obj.scriptIndex = scripts.FindIndex(s => s == script);
                        else
                        {
                            scripts.Add(script);
                            obj.scriptIndex = scripts.Count - 1;
                        }
                        //obj.scriptText = ASCIIEncoding.ASCII.GetString(br.ReadBytes(rCount));
                        WorldGrid[i, j].objects.Add(obj);
                    }
                }
            }
        }
        br.Close();

        return false;
    }

    public void Save(string filename)
    {

        WorldGrid = new Mpd_WorldGrid[100, 100];
        var objectes = transform.GetComponentsInChildren<ObjectData>();
        foreach (var obj in objectes)
        {
            addWorldObject(obj.transform.localToWorldMatrix, obj.ModelID, obj.scriptID);
        }
        WirteToFile(filename);
        Debug.Log($"Saved To {filename}");
    }

    public void WirteToFile(string filename)
    {
        if (File.Exists(filename))
            File.Delete(filename);
        BinaryWriter bw = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite));
        bw.Write(ASCIIEncoding.ASCII.GetBytes("MPD"));
        bw.Write(20000);
        bw.Write((short)200);
        for (int i = 0; i < Pieces.Count; i++)
        {
            byte[] shiftjistext;
            byte[] blankText = new byte[256];
            if (Pieces[i].visualMesh != null)
            {
                shiftjistext = USEncoder.ToEncoding.ToSJIS(Pieces[i].visualMesh);
                bw.Write(shiftjistext);
                bw.BaseStream.Seek(256 - shiftjistext.Length, SeekOrigin.Current);
            }
            else
                bw.Write(blankText);

            if (Pieces[i].collisionMesh != null)
            {
                shiftjistext = USEncoder.ToEncoding.ToSJIS(Pieces[i].collisionMesh);
                bw.Write(shiftjistext);
                bw.BaseStream.Seek(256 - shiftjistext.Length, SeekOrigin.Current);
            }
            else
                bw.Write(blankText);
            bw.Write((short)-1);
            bw.BaseStream.Seek(1, SeekOrigin.Current);

            if (Pieces[i].scriptText != null)
            {
                List<byte> list = new List<byte>();
                list.AddRange(USEncoder.ToEncoding.ToSJIS(Pieces[i].scriptText));
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j] == 0x0A && list[j - 1] != 0x0D)
                        list.Insert(j, 0x0D);
                }
                bw.Write(list.Count);
                bw.Write(list.ToArray());
            }
            else
            {
                bw.Write(1);
                bw.Write((byte)0x00);
            }
        }
        bw.Write((short)100);
        bw.Write((short)100);
        bw.Write(gridSize);
        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {
                if (WorldGrid[x, y] != null)
                {
                    int count = WorldGrid[x, y].objects.Count;
                    bw.Write(count);
                    for (int z = 0; z < count; z++)
                    {
                        bw.Write(WorldGrid[x, y].objects[z].transform.m00);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m10);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m20);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m30);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m01);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m11);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m21);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m31);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m02);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m12);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m22);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m32);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m03);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m13);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m23);
                        bw.Write(WorldGrid[x, y].objects[z].transform.m33);
                        bw.Write(WorldGrid[x, y].objects[z].pieceID);
                        List<byte> list = new List<byte>();
                        list.AddRange(USEncoder.ToEncoding.ToSJIS(scripts[WorldGrid[x, y].objects[z].scriptIndex]));
                        for (int j = 0; j < list.Count; j++)
                        {
                            if (list[j] == 0x0A && list[j - 1] != 0x0D)
                                list.Insert(j, 0x0D);
                        }
                        bw.Write(list.Count);
                        bw.Write(list.ToArray());
                    }
                }
                else
                    bw.Write(0);
            }
        }

        bw.Close();
    }

    public void addWorldObject(Matrix4x4 trs, int pieceID, int scriptID)
    {
        var position = trs.GetColumn(3);

        int x = Mathf.FloorToInt(position.x / gridSize);
        int y = Mathf.FloorToInt(position.z / gridSize);

        Mpd_Object mo;
        mo.transform = trs;
        mo.pieceID = (short)pieceID;
        mo.scriptIndex = scriptID;

        if (WorldGrid[y, x] == null)
        {
            WorldGrid[y, x] = new Mpd_WorldGrid();
            WorldGrid[y, x].objects = new List<Mpd_Object>();
        }
        WorldGrid[y, x].objects.Add(mo);
    }
}
