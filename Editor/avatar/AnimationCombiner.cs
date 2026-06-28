using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class AnimationCombiner : EditorWindow
{
    private List<AnimationClip> clips = new List<AnimationClip>();
    private ReorderableList reorderableList;
    private float intervalSeconds = 1f;
    private string saveName = "CombinedAnimation";
    private Vector2 scrollPos;

    [Serializable]
    private class CombinedAnimationMeta
    {
        public List<string> clipPaths = new List<string>();
        public float intervalSeconds;
    }

    [MenuItem("Tools/cympfh/AnimationCombiner")]
    private static void OpenWindow()
    {
        var window = GetWindow<AnimationCombiner>("AnimationCombiner");
        window.Show();
    }

    private void OnEnable()
    {
        BuildReorderableList();
    }

    private void BuildReorderableList()
    {
        reorderableList = new ReorderableList(clips, typeof(AnimationClip), true, true, true, true);

        reorderableList.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, "AnimationClips（ドラッグで並べ替え可）");

        reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;
            float labelWidth = 60f;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, labelWidth, rect.height), $"Clip {index + 1}");
            clips[index] = (AnimationClip)EditorGUI.ObjectField(
                new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, rect.height),
                clips[index], typeof(AnimationClip), false);
        };

        reorderableList.onAddCallback = list => clips.Add(null);
        reorderableList.onRemoveCallback = list =>
        {
            if (list.index >= 0 && list.index < clips.Count)
                clips.RemoveAt(list.index);
        };
    }

    private void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.HelpBox("1番目 → 0秒開始\n2番目 → 間隔秒開始\n…と自動で配置されます", MessageType.Info);

        // D&Dエリア
        Rect dropArea = GUILayoutUtility.GetRect(0, 50, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "_meta.json をここにドロップ（クリップリストと間隔を復元）");
        HandleJsonDragAndDrop(dropArea);

        EditorGUILayout.Space(4);

        if (reorderableList == null) BuildReorderableList();
        reorderableList.DoLayoutList();

        intervalSeconds = EditorGUILayout.FloatField("間隔（秒）", intervalSeconds);
        saveName = EditorGUILayout.TextField("新しいClipの名前", saveName);

        EditorGUILayout.Space(4);

        if (GUILayout.Button("結合して新しいAnimationClipを作成", GUILayout.Height(40)))
        {
            if (clips.Count == 0 || clips.Any(c => c == null))
            {
                EditorUtility.DisplayDialog("エラー", "すべてのクリップを正しく指定してください", "OK");
                EditorGUILayout.EndScrollView();
                return;
            }
            CreateCombinedClip();
        }

        EditorGUILayout.EndScrollView();
    }

    private void HandleJsonDragAndDrop(Rect dropArea)
    {
        Event evt = Event.current;
        if (!dropArea.Contains(evt.mousePosition))
            return;

        if (evt.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            evt.Use();
        }
        else if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            foreach (string path in DragAndDrop.paths)
            {
                if (path.EndsWith("_meta.json", StringComparison.OrdinalIgnoreCase))
                {
                    LoadMetaFromPath(path);
                    break;
                }
            }
            evt.Use();
        }
    }

    private void LoadMetaFromPath(string jsonPath)
    {
        try
        {
            string json = File.ReadAllText(jsonPath);
            var meta = JsonUtility.FromJson<CombinedAnimationMeta>(json);

            clips.Clear();
            foreach (var path in meta.clipPaths)
            {
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                if (clip != null)
                    clips.Add(clip);
                else
                    Debug.LogWarning($"クリップが見つかりませんでした: {path}");
            }
            intervalSeconds = meta.intervalSeconds;
            BuildReorderableList();
            // EditorUtility.DisplayDialog("ロード成功", $"クリップ {clips.Count}個 と間隔を復元しました", "OK");
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog("エラー", $"JSONの読み込みに失敗しました\n{ex.Message}", "OK");
        }
    }

    private void CreateCombinedClip()
    {
        var newClip = new AnimationClip();
        float maxEndTime = 0f;

        // floatカーブ用
        var mergedCurves = new Dictionary<EditorCurveBinding, List<Keyframe>>();
        // ObjectReferenceCurve用（マテリアル変更などを保持）
        var mergedObjectCurves = new Dictionary<EditorCurveBinding, List<ObjectReferenceKeyframe>>();

        var allEvents = new List<AnimationEvent>();

        for (int i = 0; i < clips.Count; i++)
        {
            var clip = clips[i];
            float offset = i * intervalSeconds;
            maxEndTime = Mathf.Max(maxEndTime, offset + clip.length);

            // floatカーブの結合
            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve == null) continue;

                if (!mergedCurves.ContainsKey(binding))
                    mergedCurves[binding] = new List<Keyframe>();

                foreach (var key in curve.keys)
                {
                    var k = key;
                    k.time += offset;
                    mergedCurves[binding].Add(k);
                }
            }

            // ObjectReferenceCurveの結合（マテリアル変更を保持）
            var objBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            foreach (var binding in objBindings)
            {
                var curve = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                if (curve == null) continue;

                if (!mergedObjectCurves.ContainsKey(binding))
                    mergedObjectCurves[binding] = new List<ObjectReferenceKeyframe>();

                foreach (var key in curve)
                {
                    var k = key;
                    k.time += offset;
                    mergedObjectCurves[binding].Add(k);
                }
            }

            // アニメーションイベントの結合
            var events = AnimationUtility.GetAnimationEvents(clip);
            foreach (var ev in events)
            {
                var newEv = new AnimationEvent
                {
                    time = ev.time + offset,
                    functionName = ev.functionName,
                    stringParameter = ev.stringParameter,
                    floatParameter = ev.floatParameter,
                    intParameter = ev.intParameter,
                    objectReferenceParameter = ev.objectReferenceParameter,
                    messageOptions = ev.messageOptions
                };
                allEvents.Add(newEv);
            }
        }

        // floatカーブをセット
        foreach (var kv in mergedCurves)
        {
            kv.Value.Sort((a, b) => a.time.CompareTo(b.time));
            var mergedCurve = new AnimationCurve(kv.Value.ToArray());
            AnimationUtility.SetEditorCurve(newClip, kv.Key, mergedCurve);
        }

        // ObjectReferenceCurveをセット
        foreach (var kv in mergedObjectCurves)
        {
            kv.Value.Sort((a, b) => a.time.CompareTo(b.time));
            AnimationUtility.SetObjectReferenceCurve(newClip, kv.Key, kv.Value.ToArray());
        }

        // イベントをセット
        AnimationUtility.SetAnimationEvents(newClip, allEvents.ToArray());

        if (clips.Count > 0 && clips[0] != null)
            newClip.frameRate = clips[0].frameRate;

        // 保存
        string folder = "Assets";
        if (clips[0] != null)
        {
            string origPath = AssetDatabase.GetAssetPath(clips[0]);
            if (!string.IsNullOrEmpty(origPath))
                folder = System.IO.Path.GetDirectoryName(origPath);
        }

        string animPath = EditorUtility.SaveFilePanelInProject("結合したAnimationClipを保存", saveName, "anim", "Combined animation clip", folder);
        if (string.IsNullOrEmpty(animPath)) return;

        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
        if (existing != null)
        {
            EditorUtility.CopySerialized(newClip, existing);
        }
        else
        {
            AssetDatabase.CreateAsset(newClip, animPath);
        }
        AssetDatabase.SaveAssets();

        // メタ情報JSON出力
        var meta = new CombinedAnimationMeta();
        foreach (var clip in clips)
        {
            string clipPath = AssetDatabase.GetAssetPath(clip);
            if (!string.IsNullOrEmpty(clipPath))
                meta.clipPaths.Add(clipPath);
        }
        meta.intervalSeconds = intervalSeconds;

        string metaPath = animPath.Replace(".anim", "_meta.json");
        string json = JsonUtility.ToJson(meta, true);
        File.WriteAllText(metaPath, json);

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newClip;
        Debug.Log($"結合完了！\n.anim → {animPath}\n_meta.json → {metaPath}");
        EditorUtility.DisplayDialog("成功", $"新しいAnimationClipとメタJSONを作成しました", "OK");
    }
}
