using System.Collections.Generic;
using System.Linq;
using Project.Scripts.Managers.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityExtensions;

namespace Project.Scripts.Core
{
    public class StandingGrid : MonoSingleton<StandingGrid>
    {
        [FormerlySerializedAs("currentMovers")]
        public List<Shooter> currentShooters;

        public List<StandingTile> standingTiles;
        public Transform leftExit;
        public Transform rightExit;

        private void Start()
        {
            standingTiles = new List<StandingTile>(standingTiles.OrderBy((tile => tile.transform.position.x)));
        }


        public StandingTile FirstEmptyTile()
        {
            return standingTiles.FirstOrDefault((tile => !tile.currentShooter));
        }

        public List<Shooter> CurrentStickmanList()
        {
            var list = new List<Shooter>();
            foreach (var tile in standingTiles.Where(tile =>
                         tile.currentShooter is not null && !list.Contains(tile.currentShooter)))
            {
                list.Add(tile.currentShooter);
            }

            return list;
        }

        public int CurrentActiveShooterCount()
        {
            return currentShooters.Count((shooter => shooter.currentState == ShooterState.Shooting && shooter.currentlyShooting));
        }

        public void UpdateShooters(BallColor color)
        {
            var colorCount = GM.Instance.currentLevel.GetBallCount(color);

            var allShooters = currentShooters.FindAll(currentShooter => currentShooter.shooterColor == color)
                .OrderBy((shooter1 => shooter1.bulletCount)).ToList();

            for (var i = 0; i < allShooters.Count; i++)
            {
                var current = allShooters[i];
                current.shootUntilCount = int.MaxValue;
                var bulletCount = current.bulletCount;

                if (colorCount >= bulletCount)
                {
                    current.shootUntilCount = 0;
                    colorCount -= current.bulletCount;
                }
                else
                {
                    current.shootUntilCount = current.bulletCount - colorCount;
                    colorCount -= current.bulletCount - current.shootUntilCount;
                }


                if (colorCount == 0) break;
            }
        }

        public void RemoveShooter(Shooter shooter)
        {
            currentShooters.Remove(shooter);
            standingTiles.Find((tile => tile.currentShooter == shooter)).currentShooter = null;
            UpdateShooters(shooter.shooterColor);
        }

        public void AddShooter(Shooter shooter)
        {
            if (!currentShooters.Contains(shooter))
            {
                currentShooters.Add(shooter);
                UpdateShooters(shooter.shooterColor);
            }
        }
    }
}