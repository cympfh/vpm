using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class AnimationCombiner : EditorWindow
{
    private List<AnimationClip> clips = new List<AnimationClip>();
    private float intervalSeconds = 1f;
    private string saveName = "CombinedAnimation";

    [MenuItem("Tools/cympfh/AnimationCombiner")]
    private static void OpenWindow()
    {
        var window = GetWindow<AnimationCombiner>("AnimationCombiner");
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("結合するAnimationClipを順番に指定してください", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("1番目 → 0秒開始\n2番目 → 1秒開始\n…と自動で配置されます", MessageType.Info);

        for (int i = 0; i < clips.Count; i++)
        {
            clips[i] = (AnimationClip)EditorGUILayout.ObjectField($"Clip {i + 1}", clips[i], typeof(AnimationClip), false);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("＋ クリップを追加"))
            clips.Add(null);
        if (GUILayout.Button("－ 最後を削除") && clips.Count > 0)
            clips.RemoveAt(clips.Count - 1);
        EditorGUILayout.EndHorizontal();

        intervalSeconds = EditorGUILayout.FloatField("間隔（秒）", intervalSeconds);
        saveName = EditorGUILayout.TextField("新しいClipの名前", saveName);

        if (GUILayout.Button("結合して新しいAnimationClipを作成", GUILayout.Height(40)))
        {
            if (clips.Count == 0 || clips.Any(c => c == null))
            {
                EditorUtility.DisplayDialog("エラー", "すべてのクリップを正しく指定してください", "OK");
                return;
            }
            CreateCombinedClip();
        }
    }

    private void CreateCombinedClip()
    {
        var newClip = new AnimationClip();
        float maxEndTime = 0f;

        var mergedCurves = new Dictionary<EditorCurveBinding, List<Keyframe>>();
        var allEvents = new List<AnimationEvent>();

        for (int i = 0; i < clips.Count; i++)
        {
            var clip = clips[i];
            float offset = i * intervalSeconds;
            maxEndTime = Mathf.Max(maxEndTime, offset + clip.length);

            // カーブの結合
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

        // カーブをセット（これで自動的に length が maxEndTime になります）
        foreach (var kv in mergedCurves)
        {
            kv.Value.Sort((a, b) => a.time.CompareTo(b.time));
            var mergedCurve = new AnimationCurve(kv.Value.ToArray());
            AnimationUtility.SetEditorCurve(newClip, kv.Key, mergedCurve);
        }

        // イベントをセット
        AnimationUtility.SetAnimationEvents(newClip, allEvents.ToArray());

        // フレームレートを引き継ぎ（length はカーブから自動計算されるため削除）
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

        string path = EditorUtility.SaveFilePanelInProject("結合したAnimationClipを保存", saveName, "anim", "Combined animation clip", folder);
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newClip, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newClip;
            Debug.Log($"✅ 結合完了！ → {path}");
            EditorUtility.DisplayDialog("成功", $"新しいAnimationClipを作成しました\n{path}", "OK");
        }
    }
}