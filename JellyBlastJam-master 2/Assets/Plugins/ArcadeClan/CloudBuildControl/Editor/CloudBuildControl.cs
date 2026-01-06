using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class CloudBuildControl
{
#if UNITY_EDITOR
    
    private const string CreateNewBuildAPIUrl = "https://build-api.cloud.unity3d.com/api/v1/orgs/{0}/projects/{1}/buildtargets/{2}/builds";
    private const string CancelBuildAPIUrl = "https://build-api.cloud.unity3d.com/api/v1/orgs/{0}/projects/{1}/buildtargets/{2}/builds";

    [MenuItem("Arcade Clan/Cloud Build/Android/Start Build")]
    public static void CreateNewAndroidBuild()
    {
        CreateNewCloudBuild(GetSettings().AndroidBuildTargetID);
    }
    
    [MenuItem("Arcade Clan/Cloud Build/Android/Stop Build")]
    public static void CancelAndroidBuild()
    {
        CancelCloudBuild(GetSettings().AndroidBuildTargetID);
    }
    
    public static void CancelAndroidBuild(string number)
    {
        CancelCloudBuild(GetSettings().AndroidBuildTargetID, number);
    }

    [MenuItem("Arcade Clan/Cloud Build/iOS/Start Build")]
    public static void CreateNewIOSBuild()
    {
        CreateNewCloudBuild(GetSettings().IOSBuildTargetID);
    }
        
    [MenuItem("Arcade Clan/Cloud Build/iOS/Stop Build")]
    public static void CancelIOSBuild()
    {
        CancelCloudBuild(GetSettings().IOSBuildTargetID);
    }

    public static void CancelIOSBuild(string number)
    {
        CancelCloudBuild(GetSettings().IOSBuildTargetID, number);
    }

    [MenuItem("Arcade Clan/Cloud Build/All/Start Builds")]
    public static void CreateNewAllBuild()
    {
        CreateNewCloudBuild("_all");
    }

    [MenuItem("Arcade Clan/Cloud Build/All/Stop Builds")]
    public static void CancelAllBuilds()
    {
        CancelCloudBuild("_all");
    }
    
    private static void CreateNewCloudBuild(string buildTarget)
    {
        CloudBuildSettings settings = GetSettings();

        if (settings.ProjectID == "SetupRequired")
        {
            throw new Exception("Project ID has not been assigned.Setup Required");
        }
        
        string callUrl = string.Format(CreateNewBuildAPIUrl, settings.OrganizationID, settings.ProjectID, buildTarget);
        UnityWebRequest request = UnityWebRequest.PostWwwForm(callUrl, string.Empty);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Basic {settings.APIKey}");
        request.SendWebRequest();
    }

    private static void CancelCloudBuild(string buildTarget)
    {
        CloudBuildSettings settings = GetSettings();

        if (settings.ProjectID == "SetupRequired")
        {
            throw new Exception("Project ID has not been assigned.Setup Required");
        }

        string callUrl = string.Format(CancelBuildAPIUrl, settings.OrganizationID, settings.ProjectID, buildTarget);
        UnityWebRequest request = UnityWebRequest.Delete(callUrl);
        
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Basic {settings.APIKey}");
        request.SendWebRequest();
    }

    private static void CancelCloudBuild(string buildTarget, string number)
    {
        CloudBuildSettings settings = GetSettings();

        string callUrl = $"{string.Format(CancelBuildAPIUrl, settings.OrganizationID, settings.ProjectID, buildTarget)}/{number}";
        UnityWebRequest request = UnityWebRequest.Delete(callUrl);
        
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Basic {settings.APIKey}");
        request.SendWebRequest();
    }

    private static CloudBuildSettings cloudBuildSettings = null;
    private static CloudBuildSettings GetSettings()
    {
        if (cloudBuildSettings == null)
        {
            cloudBuildSettings = Resources.Load<CloudBuildSettings>("CloudBuildSettings");
        }
        return cloudBuildSettings;
    }
#endif
}