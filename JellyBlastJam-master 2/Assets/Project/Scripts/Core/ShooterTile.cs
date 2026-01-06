using System.Collections.Generic;
using DG.Tweening;
using Project.Scripts.Managers.Core;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Project.Scripts.Core
{
    public class ShooterTile : Tile , IClickable
    {
        public List<ShooterTile> neighborTiles;
        public Vector2Int gridPosition;
        public bool isBlocked;
        public bool hasTunnel;
        public Shooter currentShooter;
        public Blocker blockerObject;
        public ShooterTunnel shooterTunnelObject;
        [ShowInInspector] public bool IsWalkable => !currentShooter && !isBlocked && !hasTunnel;
        public bool ExitTile => gridPosition.y == 0;

        public UnityAction OnTileEmptied;
        public Transform exitPosition;

        private void Start()
        {
            currentShooter ??= GetComponentInChildren<Shooter>();
        }

        public void FindNeighbors(List<ShooterTile> tiles)
        {
            tiles = new List<ShooterTile>(tiles);

            var forwardTile = tiles.Find((tile => tile.gridPosition.x == gridPosition.x && tile.gridPosition.y - gridPosition.y == -1));
            var backwardTile = tiles.Find((tile => tile.gridPosition.x == gridPosition.x && tile.gridPosition.y - gridPosition.y == 1));
            var leftTile = tiles.Find((tile => tile.gridPosition.y == gridPosition.y && tile.gridPosition.x - gridPosition.x == -1));
            var rightTile = tiles.Find((tile => tile.gridPosition.y == gridPosition.y && tile.gridPosition.x - gridPosition.x == 1));

            neighborTiles.Add(forwardTile);
            neighborTiles.Add(leftTile);
            neighborTiles.Add(rightTile);
            neighborTiles.Add(backwardTile);
            neighborTiles.RemoveAll((tile => tile == null));
        }

        public void BlockTile()
        {
            blockerObject.gameObject.SetActive(true);
            isBlocked = true;
        }

        public void TunnelTile(List<TunnelSlot> tunnelSlots, TunnelForward direction)
        {
            hasTunnel = true;
            isBlocked = true;
            shooterTunnelObject.GetTargetShooters(tunnelSlots,direction);
            shooterTunnelObject.gameObject.SetActive(true);
            shooterTunnelObject.leftCount.text = tunnelSlots.ToString();
        }

        public void Click()
        {
            //currentShooter?.Click();
        }

        public void Release()
        {
        }

        public void Shake()
        {
            DOTween.Kill(GetInstanceID()+"Shake", true);
            blockerObject?.transform.DOShakeRotation(0.25f, Vector3.forward * 10f, 3).SetId(GetInstanceID()+"Shake");
        }
    }
}