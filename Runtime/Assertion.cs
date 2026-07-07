using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Dynamics.Constraint.Components;
using VRC.SDKBase;

public enum AssertionType
{
    ObjectActiveState,
    AllParentConstraintsValid,
}

[Serializable]
public class AssertionEntry
{
    public AssertionType type;
    public GameObject targetObject;
    public bool expectedActive = true;
}

[AddComponentMenu("cympfh/Assertion")]
public class Assertion : MonoBehaviour, IEditorOnly
{
    public List<AssertionEntry> entries = new List<AssertionEntry>();

    public bool Evaluate(AssertionEntry entry, out string message)
    {
        switch (entry.type)
        {
            case AssertionType.ObjectActiveState:
                if (entry.targetObject == null)
                {
                    message = "対象オブジェクトが未設定です";
                    return false;
                }
                if (entry.targetObject.activeSelf != entry.expectedActive)
                {
                    message = $"'{entry.targetObject.name}' は {(entry.expectedActive ? "Active" : "Inactive")} である必要があります（現在: {(entry.targetObject.activeSelf ? "Active" : "Inactive")}）";
                    return false;
                }
                message = null;
                return true;

            case AssertionType.AllParentConstraintsValid:
                var invalidObjects = GetInvalidParentConstraintObjects();
                if (invalidObjects.Count > 0)
                {
                    message = $"ParentConstraint が未設定または無効です: {string.Join(", ", invalidObjects.ConvertAll(o => o.name))}";
                    return false;
                }
                message = null;
                return true;

            default:
                message = "未知の Assertion タイプです";
                return false;
        }
    }

    public List<GameObject> GetInvalidParentConstraintObjects()
    {
        var invalid = new List<GameObject>();

        foreach (var constraint in GetComponentsInChildren<ParentConstraint>(true))
        {
            bool hasValidSource = constraint.sourceCount > 0;
            for (int i = 0; hasValidSource && i < constraint.sourceCount; i++)
            {
                if (constraint.GetSource(i).sourceTransform == null)
                    hasValidSource = false;
            }

            if (!hasValidSource || !constraint.constraintActive)
                invalid.Add(constraint.gameObject);
        }

        foreach (var constraint in GetComponentsInChildren<VRCParentConstraint>(true))
        {
            bool hasValidSource = constraint.Sources.Count > 0;
            for (int i = 0; hasValidSource && i < constraint.Sources.Count; i++)
            {
                if (constraint.Sources[i].SourceTransform == null)
                    hasValidSource = false;
            }

            if (!hasValidSource || !constraint.IsActive)
                invalid.Add(constraint.gameObject);
        }

        return invalid;
    }

    public bool EvaluateAll(out List<string> messages)
    {
        messages = new List<string>();
        bool allValid = true;

        foreach (var entry in entries)
        {
            if (!Evaluate(entry, out var message))
            {
                messages.Add(message);
                allValid = false;
            }
        }

        return allValid;
    }
}
