using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

public class AssertionBuildValidator : IVRCSDKBuildRequestedCallback, IVRCSDKPreprocessAvatarCallback
{
    public int callbackOrder => 0;

    public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
    {
        if (requestedBuildType != VRCSDKRequestedBuildType.Scene)
            return true;

        return ValidateAll(Object.FindObjectsOfType<Assertion>(true));
    }

    public bool OnPreprocessAvatar(GameObject avatarGameObject)
    {
        return ValidateAll(avatarGameObject.GetComponentsInChildren<Assertion>(true));
    }

    private static bool ValidateAll(IEnumerable<Assertion> assertions)
    {
        bool allValid = true;

        foreach (var assertion in assertions)
        {
            if (assertion.EvaluateAll(out var messages))
                continue;

            allValid = false;
            foreach (var message in messages)
                Debug.LogError($"[Assertion] {message}", assertion.gameObject);
        }

        return allValid;
    }
}
