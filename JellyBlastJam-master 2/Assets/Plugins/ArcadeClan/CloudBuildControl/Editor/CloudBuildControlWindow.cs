using UnityEditor;
using UnityEngine;

public class CloudBuildControlWindow : EditorWindow
{
    [MenuItem("Arcade Clan/Cloud Build/Build Control GUI")]
    private static void Init()
    {
        EditorWindow window = GetWindow(typeof(CloudBuildControlWindow));
        window.titleContent = new GUIContent("Cloud Build Control");
        window.maxSize = window.minSize = new Vector2(500.0f, 500.0f);
        window.Show();
    }

    private const float fieldHeight = 30.0f;
    private string androidBuildNumber = string.Empty;
    private string iosBuildNumber = string.Empty;

    private void OnGUI()
    {
        GUIStyle centeredLabel = new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleCenter};
        GUIStyle centeredField = new GUIStyle(EditorStyles.label) {alignment = TextAnchor.MiddleLeft};
        GUIStyle textField = new GUIStyle(EditorStyles.textField) {alignment = TextAnchor.MiddleLeft};

        DrawGeneralBuildControls(centeredLabel);

        DrawAndroidBuildControls(centeredLabel, centeredField, textField);

        DrawIOSBuildControls(centeredLabel, centeredField, textField);
    }

    private static void DrawGeneralBuildControls(GUIStyle centeredLabel)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("General Build Control", centeredLabel);
        EditorGUILayout.Space(1);

        EditorGUILayout.HelpBox(
            "'Start Builds' will start build for all of the target platforms that are setup on Cloud Build dashboard.",
            MessageType.Info);
        EditorGUILayout.HelpBox(
            "'Stop Builds' will stop all of the builds in progress for all of the target platforms that are setup on Cloud Build dashboard.",
            MessageType.Info);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Start Builds", GUILayout.Height(fieldHeight)))
        {
            CloudBuildControl.CreateNewAllBuild();
        }

        if (GUILayout.Button("Stop Builds", GUILayout.Height(fieldHeight)))
        {
            CloudBuildControl.CancelAllBuilds();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }

    private void DrawAndroidBuildControls(GUIStyle centeredLabel, GUIStyle centeredField, GUIStyle textField)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Android Build Control", centeredLabel);
        EditorGUILayout.Space(1);

        EditorGUILayout.HelpBox("'Stop Builds' will stop all of the Android builds in progress.", MessageType.Info);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Start Build", GUILayout.Height(fieldHeight)))
        {
            CloudBuildControl.CreateNewAndroidBuild();
        }

        if (GUILayout.Button("Stop Builds", GUILayout.Height(fieldHeight)))
        {
            CloudBuildControl.CancelAndroidBuild();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal(GUILayout.Height(fieldHeight));
        EditorGUILayout.LabelField("Build Number: ", centeredField, GUILayout.Height(fieldHeight));
        androidBuildNumber = EditorGUILayout.TextField(androidBuildNumber, textField, GUILayout.Height(fieldHeight));

        bool wasEnabledAndroid = GUI.enabled;
        GUI.enabled = int.TryParse(androidBuildNumber, out _);

        if (GUILayout.Button("Stop Build", GUILayout.Height(fieldHeight)))
        {
            if (int.TryParse(androidBuildNumber, out _))
                CloudBuildControl.CancelAndroidBuild(androidBuildNumber);
        }

        GUI.enabled = wasEnabledAndroid;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }

    private void DrawIOSBuildControls(GUIStyle centeredLabel, GUIStyle centeredField, GUIStyle textField)
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("iOS Build Control", centeredLabel);
        EditorGUILayout.Space(1);
        EditorGUILayout.HelpBox("'Stop Builds' will stop all of the iOS builds in progress.", MessageType.Info);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Start Build", GUILayout.Height(fieldHeight)))
        {
            CloudBuildControl.CreateNewIOSBuild();
        }

        if (GUILayout.Button("Stop Builds", GUILayout.Height(fieldHeight)))
        {
            CloudBuildControl.CancelIOSBuild();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal(GUILayout.Height(fieldHeight));
        EditorGUILayout.LabelField("Build Number: ", centeredField, GUILayout.Height(fieldHeight));
        iosBuildNumber = EditorGUILayout.TextField(iosBuildNumber, textField, GUILayout.Height(fieldHeight));

        bool wasEnabledIOS = GUI.enabled;
        GUI.enabled = int.TryParse(iosBuildNumber, out _);

        if (GUILayout.Button("Stop Build", GUILayout.Height(fieldHeight)))
        {
            if (int.TryParse(iosBuildNumber, out _))
                CloudBuildControl.CancelIOSBuild(iosBuildNumber);
        }

        GUI.enabled = wasEnabledIOS;
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(2);
        EditorGUILayout.EndVertical();
    }
}