using UnityEditor;
using UnityEngine;

//[CustomEditor(typeof(WindomAni2))]
//public class WindomAni2Editor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        // 获取目标对象
//        WindomAni2 windomAni2 = (WindomAni2)target;

//        // 绘制默认的Inspector
//        DrawDefaultInspector();

//        //// 添加一个分隔线
//        //EditorGUILayout.Space();
//        //EditorGUILayout.LabelField("Animations", EditorStyles.boldLabel);

//        //// 遍历animations列表
//        //for (int i = 0; i < windomAni2.animations.Count; i++)
//        //{
//        //    EditorGUILayout.BeginHorizontal();

//        //    // 显示动画名称或索引
//        //    EditorGUILayout.LabelField($"Animation {i + 1}");

//        //    // 添加按钮
//        //    if (GUILayout.Button("Play Animation"))
//        //    {
//        //        // 在这里添加按钮点击事件的处理逻辑
//        //        Debug.Log($"Playing Animation {i + 1}");
//        //        // 例如，调用播放动画的方法
//        //        // windomAni2.PlayAnimation(i);
//        //    }

//        //    EditorGUILayout.EndHorizontal();
//        //}

//        //// 保存更改
//        //if (GUI.changed)
//        //{
//        //    EditorUtility.SetDirty(target);
//        //}
//    }
//}