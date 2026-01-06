using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Project.Scripts.Core;
using Project.Scripts.Managers;
using Project.Scripts.Managers.Core;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public class Level : MonoBehaviour
{
    public int counterMax = 100;
    public int moneyReward = 100;
    public int moveCount = 10;
    [HideInInspector] public int timer = 120;

    [TabGroup("Level Targets", TextColor = "green")]
    public List<LevelGroupTarget> levelGroupTargets = new List<LevelGroupTarget>();

    private List<BallGroup> pendingGroup = new List<BallGroup>();
    [HideInInspector] public List<BallGroup> spawnedGroup = new List<BallGroup>();

    [TabGroup("Level Grids", TextColor = "blue"), SerializeField]
    private ShooterGrid shooterGrid;

    private List<ShooterTile> shooterTiles = new List<ShooterTile>();
    public StandingGrid standingGrid;
    public int preSpawnedCount = 1;
    public FailChecker failChecker;
    public bool GroupsFinished => pendingGroup.Count == 0 && spawnedGroup.Count == 0;
    [SerializeField] private Transform ballGroupParent;
    [SerializeField] private List<Shooter> allShooters = new List<Shooter>();
    [ShowInInspector] public Dictionary<BallColor, int> totalCountDictionary = new Dictionary<BallColor, int>();
    
    [Serializable]
    public class BallDragConfig
    {
        public float ballCount;
        public float defaultDragValue;
        public float dragOnCloseHit;
    }

    public List<BallDragConfig> dragConfigs;

    private IEnumerator Start()
    {
        Generate();
        SnapArea();

        yield return null;
        foreach (var shooter in allShooters)
        {
            shooter.EnableOutline();
        }

        while (!PoolManager.Instance.isInitialized)
        {
            yield return null;
        }

        var helperOffset = 1f;
        for (var i = 0; i < levelGroupTargets.Count; i++)
        {
            if (i != 0) helperOffset =  (1f + PlayerPrefs.GetInt("FailHelper", 0) * GM.Instance.onFailDistanceChangerPercent);

            var levelGroupTarget = levelGroupTargets[i];
            var pos = new Vector3(levelGroupTarget.spawnPosition.x, 0.5f,
                levelGroupTarget.spawnPosition.y * helperOffset);
            var ballGroup = PoolManager.Instance.SpawnBallGroup(levelGroupTarget.ballColor, pos,
                levelGroupTarget.ballCount, ballGroupParent);
            pendingGroup.Add(ballGroup);
            ballGroup.onGroupSpawned += OnGroupSpawned;
        }

        for (var i = 0; i < preSpawnedCount; i++)
        {
            var group = pendingGroup[0];
            if (!spawnedGroup.Contains(group))
            {
                spawnedGroup.Add(group);
                AddBallCount(group);
            }

            group.PreSpawnBalls();
        }

        yield return null;
        ChangeGroupSpeeds();
    }

    private void SnapArea()
    {
        var levelArea = GM.Instance.levelArea;
        var gridTiles = shooterGrid.cellPlacement;
        var zPos = shooterTiles.Last().transform.position.z + 1f;
        levelArea.transform.position = Vector3.forward * zPos;
        levelArea.SetBlendShapeWeight(0, 10f + ((gridTiles.GetLength(0) - 2) * 15f));
        //levelArea.SetBlendShapeWeight(1, 100f + (gridTiles.GetLength(1) - 3) * 20f);
        levelArea.SetBlendShapeWeight(1, gridTiles.GetLength(1) * 20f);
        levelArea.SetBlendShapeWeight(2, 200);
        var mesh = new Mesh();
        levelArea.BakeMesh(mesh);
        var meshCollider = levelArea.GetComponent<MeshCollider>();
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    private void OnGroupSpawned(BallGroup ballGroup)
    {
        pendingGroup.Remove(ballGroup);
        if (!spawnedGroup.Contains(ballGroup))
        {
            spawnedGroup.Add(ballGroup);
            AddBallCount(ballGroup);
            standingGrid.UpdateShooters(ballGroup.ballColor);
        }

        ChangeGroupSpeeds();
        ballGroup.onGroupSpawned -= OnGroupSpawned;
    }

    public void AddBallCount(BallGroup group)
    {
        if (!totalCountDictionary.TryAdd(group.ballColor, group.ballCount))
        {
            totalCountDictionary[group.ballColor] += group.ballCount;
        }
    }

    public void RemoveBallCount(BallColor ballColor)
    {
        if (!totalCountDictionary.ContainsKey(ballColor)) return;
        totalCountDictionary[ballColor] -= 1;

        if (totalCountDictionary[ballColor] <= 0)
        {
            totalCountDictionary.Remove(ballColor);
        }
    }

    public int GetBallCount(BallColor color)
    {
        return totalCountDictionary.GetValueOrDefault(color, 0);
    }


    public List<Tile> TryFindPath(ShooterTile beginTile)
    {
        var path = new List<Tile>() { beginTile };
        var currentTile = beginTile;
        if (currentTile.ExitTile) return path;

        var discardedTiles = new List<ShooterTile>();

        var possibleExitTiles = shooterTiles.FindAll((tile => tile.ExitTile && tile.IsWalkable));
        if (possibleExitTiles.Count == 0) return null;

        while (true)
        {
            var tempNeighbors = new List<ShooterTile>(currentTile.neighborTiles);
            var closestExit = possibleExitTiles
                .OrderBy((tile => Vector3.Distance(tile.transform.position, currentTile.transform.position))).First();
            if (closestExit.gridPosition.x < currentTile.gridPosition.x)
            {
                tempNeighbors = currentTile.neighborTiles.OrderBy((tile => tile.gridPosition.x)).ToList();
            }
            else if (closestExit.gridPosition.x > currentTile.gridPosition.x)
            {
                tempNeighbors = currentTile.neighborTiles.OrderByDescending((tile => tile.gridPosition.x)).ToList();
            }

            var neighbors = tempNeighbors
                .Where((tile => tile.IsWalkable && !path.Contains(tile) && !discardedTiles.Contains(tile))).ToList();
            if (neighbors.Count == 0)
            {
                discardedTiles.Add(currentTile);
                if (currentTile == beginTile) return null;

                currentTile = beginTile;
                path = new List<Tile>() { beginTile };
            }

            foreach (var neighbor in neighbors.Where(neighbor => neighbor.IsWalkable && !path.Contains(neighbor)))
            {
                path.Add(neighbor);
                currentTile = neighbor;
                if (!currentTile.ExitTile) break;
                return path;
            }
        }
    }

    public void OnSuccess()
    {
        foreach (var shooter in allShooters)
        {
            shooter.Success();
        }
    }

    public void OnFail()
    {
        foreach (var group in pendingGroup)
        {
            group.Stop();
        }

        foreach (var group in spawnedGroup)
        {
            group.Drop();
        }

        foreach (var shooter in allShooters)
        {
            shooter.Stop();
        }
    }

    public void AddShooterDefault(Shooter shooter)
    {
        if (!allShooters.Contains(shooter))
        {
            allShooters.Add(shooter);
        }
    }

    public void AddShooterGrid(Shooter shooter)
    {
        standingGrid.AddShooter(shooter);
    }

    public void RemoveShooter(Shooter shooter)
    {
        allShooters.Remove(shooter);
        standingGrid.RemoveShooter(shooter);
    }

    public void CheckLeftoversPath()
    {
        foreach (var shooter in allShooters.FindAll((shooter => shooter.currentState == ShooterState.Idling)))
        {
            shooter.EnableOutline();
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (shooterGrid == null || shooterGrid.cellPlacement == null)
            return;

        var sGrid = shooterGrid.cellPlacement;
        var columns = sGrid.GetLength(0);
        var rows = sGrid.GetLength(1);

        var xOffset = shooterGrid.gridOffsetX;
        var zOffset = shooterGrid.gridOffsetZ;
        var midOffset = shooterGrid.beginOffsetToMidPoint;

        var basePos = Vector3.zero;

        basePos += Vector3.forward * -midOffset;

        var totalXOffset = xOffset * (columns - 1);
        var halfX = totalXOffset * 0.5f;

        Gizmos.color = Color.yellow;

        for (var c = 0; c < columns; c++)
        {
            var xPos = Mathf.Lerp(-halfX, halfX, Mathf.InverseLerp(0, columns - 1, c));

            var totalZOffset = zOffset * (rows - 1);
            var halfZ = totalZOffset * 0.5f;

            for (var r = 0; r < rows; r++)
            {
                var zPos = Mathf.Lerp(-halfZ, halfZ, Mathf.InverseLerp(0, rows - 1, r));

                var spherePos = basePos + new Vector3(xPos, 0f, zPos);
                Gizmos.DrawSphere(spherePos, 0.1f);
            }
        }

        if (Application.isPlaying) return;

        foreach (var levelGroupTarget in levelGroupTargets)
        {
            var color = GM.Instance.GetShooterColor(levelGroupTarget.ballColor).colorMaterial.color;
            Handles.color = color;
            var pos = new Vector3(levelGroupTarget.spawnPosition.x, 0.5f, levelGroupTarget.spawnPosition.y);

            Handles.DrawAAPolyLine(10f, pos, pos + Vector3.up * 1.9f);

            Handles.Label(pos + Vector3.up * 2f, $"{levelGroupTarget.ballCount}", new GUIStyle()
            {
                fontSize = 48,
                normal = new GUIStyleState() { textColor = color },
                alignment = TextAnchor.MiddleCenter,
            });

            Handles.DrawWireDisc(pos, Vector3.up, 1f);
        }
    }
#endif

    public void Generate()
    {
        shooterTiles = new List<ShooterTile>();

        var sGrid = shooterGrid.cellPlacement;
        var rowParent = new GameObject
        {
            name = "Stickman Rows",
            transform =
            {
                parent = transform
            }
        };

        var beginPos = (-sGrid.GetLength(0) * 0.5f) + 0.5f;

        var delay = 0f;
        for (var c = 0; c < sGrid.GetLength(0); c++)
        {
            var row = new GameObject
            {
                transform =
                {
                    parent = rowParent.transform,
                    position = new Vector3(beginPos, 0f, -9f)
                },
                name = $"Shooter Row {(c + 1).ToString()}"
            };

            var asd = DOTween.Sequence();
            for (var r = 0; r < sGrid.GetLength(1); r++)
            {
                var value = sGrid[c, r];
                var tile = Instantiate(GM.Instance.shooterTilePrefab, row.transform);
                tile.transform.localPosition = new Vector3(0f, 0f, (sGrid.GetLength(1) - r + 1));
                asd.Append(tile.transform.DOScale(Vector3.zero, 0.1f).From().SetEase(Ease.OutBack)
                    .OnComplete(Taptic.Light));
                tile.gridPosition = new Vector2Int(c, r);
                tile.name = $"Grid {c.ToString()} - {r.ToString()}";
                if (value.isTunnel)
                {
                    tile.TunnelTile(value.tunnelSlots, value.forwardDirection);
                }
                else
                {
                    if (value.count > 0)
                    {
                        var shooter = Instantiate(GM.Instance.shooterPrefab, tile.transform);
                        shooter.name = $"({r}|{c})  |  {value.color}  |  {value.count}";
                        shooter.SetData(tile, value.color, value.count, value.isHidden);
                        AddShooterDefault(shooter);
                    }
                    else if (value.count == 0)
                    {
                        tile.BlockTile();
                    }
                }

                shooterTiles.Add(tile);
            }

            beginPos += 1f;
            delay += 0.05f;
        }

        rowParent.transform.localPosition = Vector3.forward * -shooterGrid.beginOffsetToMidPoint;


        foreach (var stickmanTile in shooterTiles)
        {
            var temp = new List<ShooterTile>(shooterTiles);
            temp.Remove(stickmanTile);
            stickmanTile.FindNeighbors(temp);
        }

        CenterTileTransformWithOffset(shooterGrid.gridOffsetX, shooterGrid.beginOffsetToMidPoint,
            shooterGrid.gridOffsetZ);
    }

    private void CenterTileTransforms()
    {
        var target = GetComponentsInChildren<Transform>().FirstOrDefault((o => o.name == "Stickman Rows"));

        for (var i = 0; i < shooterGrid.column; i++)
        {
            var targetRow = target.GetChild(i);
            var pos = targetRow.localPosition;
            pos.z = 0f;
            targetRow.localPosition = pos;
            var grids = targetRow.GetComponentsInChildren<ShooterTile>().ToList();
            var beginZ = (grids.Count - 1) * 0.5f;

            for (var j = 0; j < grids.Count; j++)
            {
                var grid = grids[j];
                pos = grid.transform.localPosition;
                pos.z = beginZ;
                beginZ -= 1f;
                grid.transform.localPosition = pos;
            }
        }
    }

    private void CenterTileTransformWithOffset(float xOffset = 1.2f, float midOffset = -2f, float zOffset = 1.5f)
    {
        CenterTileTransforms();
        var parent = GetComponentsInChildren<Transform>().FirstOrDefault((o => o.name == "Stickman Rows"));

        parent.transform.position = Vector3.forward * -midOffset;

        for (var i = 0; i < shooterGrid.column; i++)
        {
            var totalOffset = xOffset * (shooterGrid.column - 1);
            var half = totalOffset * 0.5f;
            var targetRow = parent.GetChild(i);
            var pos = targetRow.localPosition;
            pos.x = Mathf.Lerp(-half, half, Mathf.InverseLerp(0, shooterGrid.column - 1, i));
            targetRow.localPosition = pos;

            var grids = targetRow.GetComponentsInChildren<ShooterTile>().ToList();
            grids.Reverse();

            totalOffset = zOffset * (grids.Count - 1);
            half = totalOffset * 0.5f;

            for (var j = 0; j < grids.Count; j++)
            {
                var grid = grids[j];
                pos = grid.transform.localPosition;
                pos.z = Mathf.Lerp(-half, half, Mathf.InverseLerp(0, grids.Count - 1, j));
                grid.transform.localPosition = pos;
            }
        }
    }
    
    public (float defaultDrag, float dragOnCloseHit) EvaluateDrag(float currentBallCount)
    {
        if (dragConfigs.Count < 2)
            throw new Exception("At least two config entries are required.");

        var sorted = dragConfigs.OrderBy(c => c.ballCount).ToList();

        if (currentBallCount <= sorted.First().ballCount)
            return (sorted.First().defaultDragValue, sorted.First().dragOnCloseHit);
        if (currentBallCount >= sorted.Last().ballCount)
            return (sorted.Last().defaultDragValue, sorted.Last().dragOnCloseHit);

        for (var i = 0; i < sorted.Count - 1; i++)
        {
            var b1 = sorted[i].ballCount;
            var b2 = sorted[i + 1].ballCount;

            if (currentBallCount >= b1 && currentBallCount <= b2)
            {
                var t = Mathf.InverseLerp(b1, b2, currentBallCount);

                var drag1 = Mathf.Lerp(sorted[i].defaultDragValue, sorted[i + 1].defaultDragValue, t);
                var drag2 = Mathf.Lerp(sorted[i].dragOnCloseHit, sorted[i + 1].dragOnCloseHit, t);

                return (drag1, drag2);
            }
        }

        throw new Exception("Drag evaluation failed.");
    }
    public void ChangeGroupSpeeds()
    {
        var activeBalls = spawnedGroup.Sum((group => group.balls.Count));
        var diff = EvaluateDrag(activeBalls);
        foreach (var ballGroup in pendingGroup)
        {
            ballGroup.ChangeRbSpeed(diff);
        }
        
        foreach (var ballGroup in spawnedGroup)
        {
            ballGroup.ChangeRbSpeed(diff);
        }
    }

#if UNITY_EDITOR


    #region ShooterData

    [FoldoutGroup("ShooterNeededCount"), ShowInInspector]
    public int NeededRedCount => ShowNeededCount(BallColor.Red);

    [FoldoutGroup("ShooterNeededCount"), ShowInInspector]
    public int NeededBlueCount => ShowNeededCount(BallColor.Blue);

    [FoldoutGroup("ShooterNeededCount"), ShowInInspector]
    public int NeededGreenCount => ShowNeededCount(BallColor.Green);

    [FoldoutGroup("ShooterNeededCount"), ShowInInspector]
    public int NeededPinkCount => ShowNeededCount(BallColor.Pink);

    [FoldoutGroup("ShooterNeededCount"), ShowInInspector]
    public int NeededPurpleCount => ShowNeededCount(BallColor.Purple);

    [FoldoutGroup("ShooterNeededCount"), ShowInInspector]
    public int NeededYellowCount => ShowNeededCount(BallColor.Yellow);

    [FoldoutGroup("ShooterNeededCount"), ShowInInspector]
    public int NeededOrangeCount => ShowNeededCount(BallColor.Orange);

    [FoldoutGroup("ShooterNeededCount"), ShowInInspector]
    public int NeededBrownCount => ShowNeededCount(BallColor.Brown);

    [FoldoutGroup("ShooterNeededCount"), ShowInInspector]
    public int NeededTurquoiseCount => ShowNeededCount(BallColor.Turquoise);
    
    [FoldoutGroup("ShooterNeededCount"), ShowInInspector]
    public int TotalBallCount => AllBallCount();

    private int AllBallCount()
    {
        var sum = 0;
        var enumValues = Enum.GetValues(typeof(BallColor));
        var totalEnumCount = enumValues.Length;
        for (var i = 0; i < totalEnumCount; i++)
        {
            BallColor color = (BallColor)enumValues.GetValue(i);
            sum += TotalBallCountWithColor(color);
        }

        return sum;
    }
                             
    private int TotalBallCountWithColor(BallColor color)
    {
        return levelGroupTargets.Where(target => target.ballColor == color).Sum(target => target.ballCount);
    }

    private int ShowNeededCount(BallColor color)
    {
        if (shooterGrid is null) return 0;

        var c = 0;
        for (var i = 0; i < shooterGrid.cellPlacement.GetLength(0); i++)
        {
            for (var j = 0; j < shooterGrid.cellPlacement.GetLength(1); j++)
            {
                var element = shooterGrid.cellPlacement[i, j];
                if (element.isTunnel)
                {
                    c += element.tunnelSlots.Where(tunnelSlot => tunnelSlot.color == color)
                        .Sum(tunnelSlot => tunnelSlot.count);
                }
                else
                {
                    if (element.color == color)
                    {
                        c += element.count;
                    }
                }
            }
        }

        return TotalBallCountWithColor(color) - c;
    }

    #endregion

#endif
    
}