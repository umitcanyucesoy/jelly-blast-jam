using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    #region Debug

    [Header("Main Debug Properties")] [SerializeField]
    private GameObject debugPanel = null;

    [SerializeField] private TextMeshProUGUI lastActionText = null;

    public void OpenDebugMenu()
    {
        debugPanel.SetActive(true);
        
        ChangeGizmoVisibility(gizmosVisibility == DebugState.On);
        ChangeTimeScaleInteractivity(true);
        UpdateLastActionText("Debug Panel Opened");
    }

    public void CloseDebugMenu()
    {
        debugPanel.SetActive(false);

        ChangeTimeScaleInteractivity(false);
        UpdateLastActionText("Debug Panel Closed");
    }

    private void UpdateLastActionText(string lastAction)
    {
        if (!lastActionText)
        {
            return;
        }

        lastActionText.text = $"Last Action\n{lastAction}";
    }

    #endregion Debug

    #region Gizmos Visibility

    [Space] [Header("Gizmos Visibility")] public TextMeshProUGUI gizmosText = null;
    public DebugState gizmosVisibility = DebugState.Off;
    public UnityEvent<DebugState> onGizmosVisibilityChange = null;
    public List<GameObject> gizmoObjects = new List<GameObject>();

    public void ToggleGizmosVisibility()
    {
        gizmosVisibility = gizmosVisibility == DebugState.On
            ? DebugState.Off
            : DebugState.On;

        gizmosText.text = $"Gizmos\n{gizmosVisibility.ToString().ToUpper()}";
        onGizmosVisibilityChange?.Invoke(gizmosVisibility);

        ChangeGizmoVisibility(gizmosVisibility == DebugState.On);
        UpdateLastActionText($"Toggle Gizmos Visibility to <b>{gizmosVisibility.ToString()}</b>");
    }

    public void ChangeGizmoVisibility(bool visibility)
    {
        gizmoObjects.ForEach(g =>
        {
            if (g)
            {
                g.SetActive(visibility);
            }
        });
    }

    #endregion Gizmos Visibility

    #region Data Reset

    [Space] [Header("Data Reset")] public UnityEvent onDataReset = null;

    public void ResetSavedData()
    {
        PlayerPrefs.DeleteAll();
        //DataManager.Instance.Delete();

        onDataReset?.Invoke();
        UpdateLastActionText($"Reset saved data");
    }

    #endregion Data Reset

    #region Timescale Set

    [Space] [SerializeField] private TextMeshProUGUI timeScaleText = null;
    [SerializeField] private Slider timeScaleSlider = null;

    public void OnTimeScaleValueChanged(float value)
    {
        float timeScale = Mathf.Max(value, 0.0f);
        Time.timeScale = timeScale;

        timeScaleText.text = $"Current Time Scale : {timeScale:0.0}";
        UpdateLastActionText($"Time Scale set to : {timeScale:0.0}");
    }

    public void ChangeTimeScaleInteractivity(bool interactable)
    {
        if (!timeScaleSlider)
        {
            return;
        }

        timeScaleSlider.interactable = interactable;

        if (interactable)
        {
            timeScaleSlider.onValueChanged.AddListener(OnTimeScaleValueChanged);
        }
        else
        {
            timeScaleSlider.onValueChanged.RemoveListener(OnTimeScaleValueChanged);
        }
    }

    public void ResetTimeScale()
    {
        Time.timeScale = 1.0f;
        timeScaleSlider.value = 1.0f;
        
        timeScaleText.text = $"Current Time Scale : 1.0";
        UpdateLastActionText($"Time Scale reset to : 1.0");
    }
    #endregion Timescale Set

    #region Current Level

    [SerializeField] private TextMeshProUGUI levelNameText = null;

    public void SetCurrentLevelName(string levelName)
    {
        levelNameText.text = levelName;
    }

    #endregion
}

public enum DebugState
{
    On,
    Off
}
