using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(WindomAni2))]
//public class WindomAni2Editor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        // ��ȡĿ�����
//        WindomAni2 windomAni2 = (WindomAni2)target;

//        // ����Ĭ�ϵ�Inspector
//        DrawDefaultInspector();

//        //// ���һ���ָ���
//        //EditorGUILayout.Space();
//        //EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);

//        //// ����animations�б�
//        //for (int i = 0; i < windomAni2.animations.Count; i++)
//        //{
//        //    EditorGUILayout.BeginHorizontal();

//        //    // ��ʾ�������ƻ�����
//        //    EditorGUILayout.LabelField($"Animation {i + 1}");

//        //    // ��Ӱ�ť
//        //    if (GUILayout.Button("Play Animation"))
//        //    {
//        //        // ��������Ӱ�ť����¼��Ĵ����߼�
//        //        Debug.Log($"Playing Animation {i + 1}");
//        //        // ���磬���ò��Ŷ����ķ���
//        //        // windomAni2.PlayAnimation(i);
//        //    }

//        //    EditorGUILayout.EndHorizontal();
//        //}

//        //// �������
//        //if (GUI.changed)
//        //{
//        //    EditorUtility.SetDirty(target);
//        //}
//    }
//}