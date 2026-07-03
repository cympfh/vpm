using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(Assertion))]
public class AssertionEditor : Editor
{
    private ReorderableList list;

    private void OnEnable()
    {
        list = new ReorderableList(serializedObject, serializedObject.FindProperty("entries"), true, true, true, true);

        list.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Assertions");

        list.elementHeightCallback = index =>
        {
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            var type = (AssertionType)element.FindPropertyRelative("type").enumValueIndex;
            int lines = type == AssertionType.ObjectActiveState ? 3 : 1;
            return lines * (EditorGUIUtility.singleLineHeight + 2) + 4;
        };

        list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            rect.y += 2;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            var typeProp = element.FindPropertyRelative("type");
            var targetProp = element.FindPropertyRelative("targetObject");
            var expectedProp = element.FindPropertyRelative("expectedActive");

            var assertion = (Assertion)target;
            bool isValid = assertion.Evaluate(assertion.entries[index], out _);

            var statusStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                normal = { textColor = isValid ? Color.green : Color.red }
            };
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 20, lineHeight), isValid ? "●" : "✕", statusStyle);

            EditorGUI.PropertyField(new Rect(rect.x + 20, rect.y, rect.width - 20, lineHeight), typeProp, GUIContent.none);

            if ((AssertionType)typeProp.enumValueIndex == AssertionType.ObjectActiveState)
            {
                EditorGUI.PropertyField(
                    new Rect(rect.x + 20, rect.y + lineHeight + 2, rect.width - 20, lineHeight),
                    targetProp, new GUIContent("Target"));
                EditorGUI.PropertyField(
                    new Rect(rect.x + 20, rect.y + (lineHeight + 2) * 2, rect.width - 20, lineHeight),
                    expectedProp, new GUIContent("Expected Active"));
            }
        };

        EditorApplication.update += Repaint;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Repaint;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }
}
