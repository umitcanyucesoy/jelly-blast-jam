using UnityEngine;

[CreateAssetMenu(fileName = "New CloudBuildSettings", menuName = "CloudBuildSettings", order = 1)]
public class CloudBuildSettings : ScriptableObject
{
    public string OrganizationID = "2483540";
    public string ProjectID = "06e4b92f-04d1-445d-afc0-92ae0ca10cba";
    public string IOSBuildTargetID = "ios";
    public string AndroidBuildTargetID = "android";
    public string APIKey = "0f799ed723547f75211fe12ef849ab51";
}
