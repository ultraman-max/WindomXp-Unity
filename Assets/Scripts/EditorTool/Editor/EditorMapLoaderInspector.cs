using UnityEngine;
using UnityEditor;
using System.Linq;
using Codice.Client.BaseCommands;

[CustomEditor(typeof(EditorMapLoader))]
public class EditorMapLoaderInspector : Editor
{
    private bool showAddPieceFields = false;
    private string visualMesh = string.Empty;
    private string colliderMesh = string.Empty;
    private string script = string.Empty;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var editorMapLoader = (EditorMapLoader)target;

        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("MapFile", "Button", EditorStyles.boldLabel);

                if (GUILayout.Button("Load"))
                {
                    editorMapLoader.Load();
                }

                if (GUILayout.Button("Save"))
                {
                    editorMapLoader.Save();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (!showAddPieceFields)
            {
                if (GUILayout.Button("AddPiece")&&editorMapLoader.map!=null)
                {
                    showAddPieceFields = true;
                }
            }

            if (showAddPieceFields)
            {
                visualMesh = EditorGUILayout.TextField("VisualMesh", visualMesh);
                colliderMesh = EditorGUILayout.TextField("ColliderMesh", colliderMesh);
                script = EditorGUILayout.TextField("Script", script);
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Add") && visualMesh.Length>0)
                    {
                        showAddPieceFields = false;
                        for (int i = 0; i < editorMapLoader.map.Pieces.Count && i<198; i++)
                        {
                            var pieceData = editorMapLoader.map.Pieces[i];
                            if (pieceData.visualMesh.Length == 0 && pieceData.collisionMesh.Length == 0 && pieceData.scriptText.Length == 0)
                            {
                                pieceData = new Mpd_Piece
                                {
                                    visualMesh = visualMesh,
                                    collisionMesh = colliderMesh,
                                    scriptText = script
                                };
                                editorMapLoader.map.Pieces[i] = pieceData;
                                break;
                            }
                        }
                        EditorUtility.DisplayDialog("Pieces Full", "Cannot add more pieces. The maximum limit of 198 pieces has been reached.", "OK");
                    }
                    if(GUILayout.Button("Cancel"))
                    {
                        showAddPieceFields = false;
                    }
                }

                GUILayout.EndHorizontal();

            }
        }
        GUILayout.EndVertical();
    }
}
