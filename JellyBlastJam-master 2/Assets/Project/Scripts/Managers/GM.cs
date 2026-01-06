using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using Project.Scripts.Core;
using Project.Scripts.Managers.Core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityExtensions;
using Random = UnityEngine.Random;

public class GM : MonoSingleton<GM>
{
    public List<Level> levels = new List<Level>();
    public List<Level> excludedLevels;
    public Level currentLevel;
    public int currentLevelIndex;
    public string state = "Playing";
    public Camera cam;
    [Header("Game Settings")] public bool timerBased;
    public int failedCost = 50;
    public int buyAmount = 20;
    bool timerStarted;

    public ShooterTile shooterTilePrefab;
    public Shooter shooterPrefab;

    public List<BallColorSet> ballColors;
    public List<ShooterColorSet> shooterColors;
    public BallColorSet hiddenColorSet;
    public bool useShooterColorsForEach = false;
    public float onFailDistanceChangerPercent = 0.1f;


    #region Initialisers

    private void Awake()
    {
        Application.targetFrameRate = 60;
        cam = FindAnyObjectByType<Camera>();
        LoadLevel();
    }

    void Start()
    {
        Analytics.Instance.SendLevelStart();
        DOTween.SetTweensCapacity(750, 250);
        StartInput();
    }

    #endregion

    #region LevelLoadSystem

    public void LoadLevel()
    {
        currentLevel = FindObjectOfType<Level>();
        if (!currentLevel)
        {
            var levelIndex = level;
            currentLevelIndex = level;
            if (levelIndex < levels.Count)
                currentLevel = Instantiate(levels[levelIndex]);
            else
            {
                List<Level> lastLevels = new(levels);
                excludedLevels.ForEach(l => levels.Remove(l));
                Random.InitState(17);
                levels.Shuffle();
                Random.InitState((int)DateTime.Now.Ticks);
                currentLevel = Instantiate(levels[levelIndex % levels.Count]);
                currentLevelIndex = lastLevels.IndexOf(levels[levelIndex % levels.Count]);
            }
        }
    }

    public void Won()
    {
        if (state != "Playing")
            return;
        state = "Won";
        PlayerPrefs.DeleteKey("FailHelper");
        Analytics.Instance.SendLevelComplete();
        level += 1;
        if (timerBased)
        {
            StopCoroutine("TimerUpdate");
            UIM.Instance.clockAnim.DOPause();
        }

        cam.GetComponentInChildren<ParticleSystem>(true).Show();
        UIM.Instance.LevelCompleted();
    }

    public void Lost()
    {
        if (state != "Playing")
            return;
        state = "Lost";

        var currentValue = PlayerPrefs.GetFloat("FailHelper", 0f);
        PlayerPrefs.SetFloat("FailHelper", currentValue + onFailDistanceChangerPercent);

        UIM.Instance.LevelFailed();
        currentLevel.OnFail();
        StopInput();
    }


    public void BuyMoves()
    {
        money -= failedCost;
        UIM.Instance.UpdateMoney();
        UIM.Instance.levelFailedMenu.Hide();
        Analytics.Instance.TimeMoveBought();
        state = "Playing";
        StartInput();
        if (timerBased)
        {
            currentLevel.timer += buyAmount;
            UIM.Instance.UpdateTimer();
        }
        else
        {
            currentLevel.moveCount += buyAmount;
            UIM.Instance.moveText.color = Color.white;
            UIM.Instance.UpdateMove();
        }
    }

    IEnumerator TimerUpdate()
    {
        UIM.Instance.clockAnim.DOPlay();
        timerStarted = true;
        while (true)
        {
            yield return new WaitForSeconds(1);
            currentLevel.timer -= 1;
            UIM.Instance.UpdateTimer();
            if (currentLevel.timer > 0)
                continue;
            Lost();
            break;
        }

        UIM.Instance.clockAnim.DOPause();

        timerStarted = false;
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReloadLevelWithFail()
    {
        var currentValue = PlayerPrefs.GetInt("FailHelper", 0);
        PlayerPrefs.SetInt("FailHelper", currentValue + 1);
        Analytics.Instance.SendLevelFailed();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void DecreaseMoveCount()
    {
        if (timerBased)
            return;
        if (currentLevel.moveCount == 0)
            return;
        currentLevel.moveCount -= 1;
        UIM.Instance.UpdateMove();
        if (currentLevel.moveCount <= 0)
            Lost();
    }

    #endregion

    #region Input

    [SerializeField] private LayerMask selectableLayerMask;

    private IClickable _lastClickedObject;
    private bool _canClick;
    private Vector3 _mousePositionOnClick;
    public SkinnedMeshRenderer levelArea;

    void Down(PointerEventData data)
    {
        DecreaseMoveCount();
        if (timerBased && !timerStarted)
            StartCoroutine("TimerUpdate");

        if (state != "Playing") return;

        _mousePositionOnClick = Input.mousePosition;
        var ray = cam.ScreenPointToRay(_mousePositionOnClick);

        if (Physics.Raycast(ray, out var hit, 1000, selectableLayerMask))
        {
            if (hit.transform.TryGetComponent<IClickable>(out var clickable) &&
                clickable is Shooter { currentState: ShooterState.Idling })
            {
                _lastClickedObject = clickable;
                clickable.Click();

                Taptic.Medium();
            }

            if (clickable is ShooterTile { currentShooter: null, isBlocked: true } tile)
            {
                tile.Shake();
                Taptic.Warning();
            }
        }
    }

    void Drag(PointerEventData data)
    {
    }

    void Up(PointerEventData data)
    {
    }

    #endregion

    #region BallData

    public ShooterColorSet GetShooterColor(BallColor color)
    {
        return shooterColors.Find((colors => colors.color == color));
    }

    public BallColorSet GetBallColor(BallColor color)
    {
        if (useShooterColorsForEach)
        {
            var ballColorSet = GetShooterColor(color);
            var temp = new BallColorSet()
            {
                color = ballColorSet.color,
                colorMaterial = ballColorSet.colorMaterial,
                shadowMaterial = ballColorSet.shadowMaterial
            };
            return temp;
        }

        return ballColors.Find((colors => colors.color == color));
    }

    public BallColorSet GetHiddenColor()
    {
        return hiddenColorSet;
    }

    [Serializable]
    public struct BallColorSet
    {
        public BallColor color;
        public Material colorMaterial;
        public Material shadowMaterial;
    }

    [Serializable]
    public struct ShooterColorSet
    {
        public BallColor color;
        public Material colorMaterial;
        public Material shadowMaterial;
    }

    #endregion

    #region InputInitialisers

    void StartInput()
    {
        InputPanel.Instance.OnPointerDownFullInfo.AddListener(Down);
        InputPanel.Instance.OnDragFullInfo.AddListener(Drag);
        InputPanel.Instance.OnPointerUpFullInfo.AddListener(Up);
    }

    void StopInput()
    {
        InputPanel.Instance.OnPointerDownFullInfo.RemoveListener(Down);
        InputPanel.Instance.OnDragFullInfo.RemoveListener(Drag);
        InputPanel.Instance.OnPointerUpFullInfo.RemoveListener(Up);
    }

    #endregion

    #region Properties

    public static int money
    {
        get => PlayerPrefs.GetInt("Money");
        set => PlayerPrefs.SetInt("Money", value);
    }

    public static int level
    {
        get => PlayerPrefs.GetInt("Level");
        set => PlayerPrefs.SetInt("Level", value);
    }

    #endregion

    #region Helpers

#if UNITY_EDITOR

    [Button]
    public void RenameLevels()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogWarning("Prefab list is empty!");
            return;
        }

        var tempNames = new Dictionary<string, string>();
        var assetPaths = new List<string>();

        for (var i = 0; i < levels.Count; i++)
        {
            var prefab = levels[i];
            if (prefab == null) continue;

            var assetPath = AssetDatabase.GetAssetPath(prefab);
            if (!string.IsNullOrEmpty(assetPath))
            {
                var fileName = Path.GetFileNameWithoutExtension(assetPath);
                var randomName = fileName == $"Level {i + 1}"
                    ? fileName
                    : "Temp_" + Guid.NewGuid().ToString("N")[..8];

                tempNames[assetPath] = randomName;
                assetPaths.Add(assetPath);
            }
        }

        foreach (var entry in tempNames)
        {
            AssetDatabase.RenameAsset(entry.Key, entry.Value);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        for (var i = 0; i < assetPaths.Count; i++)
        {
            var assetPath = assetPaths[i];
            if (tempNames.TryGetValue(assetPath, out var tempName))
            {
                var fileName = Path.GetFileNameWithoutExtension(assetPath);

                if (fileName == $"Level {i + 1}")
                {
                    Debug.Log($"Skipping {fileName}, already correctly named.");
                    continue;
                }

                var newName = "Level " + (i + 1);
                var tempPath = Path.GetDirectoryName(assetPath) + "/" + tempName + ".prefab";

                AssetDatabase.RenameAsset(tempPath, newName);
                Debug.Log($"Renamed: {tempPath} -> {newName}");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Prefab names updated!!");
    }
#endif

        void Update()
    {
        if (!Application.isEditor)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            PlayerPrefs.DeleteAll();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Won();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Lost();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            level += 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            DecreaseMoveCount();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            money += 100;
            UIM.Instance.UpdateMoney();
        }
    }

    #endregion
}