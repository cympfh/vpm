using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Dynamics.Constraint.Components;

public static class ParentConstraintChecker
{
    [MenuItem("Tools/cympfh/Check Inactive Parent Constraints")]
    public static void CheckInactiveParentConstraints()
    {
        ParentConstraint[] constraints = Object.FindObjectsOfType<ParentConstraint>(true);
        VRCParentConstraint[] vrcConstraints = Object.FindObjectsOfType<VRCParentConstraint>(true);

        bool foundAny = false;

        foreach (ParentConstraint constraint in constraints)
        {
            if (!constraint.constraintActive)
            {
                Debug.Log($"Inactive Parent Constraint found on: <b>{constraint.gameObject.name}</b>", constraint.gameObject);
                foundAny = true;
            }
        }

        foreach (VRCParentConstraint constraint in vrcConstraints)
        {
            if (!constraint.IsActive)
            {
                Debug.Log($"Inactive VRC Parent Constraint found on: <b>{constraint.gameObject.name}</b>", constraint.gameObject);
                foundAny = true;
            }
        }

        if (!foundAny)
        {
            Debug.Log("No inactive Parent Constraints found in the scene.");
        }
    }
}
