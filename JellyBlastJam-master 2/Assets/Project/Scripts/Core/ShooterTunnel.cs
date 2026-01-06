using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Core
{
    public class ShooterTunnel : MonoBehaviour
    {
        private static readonly int SpawnShooter = Animator.StringToHash("SpawnShooter");
        [SerializeField] private Animator tunnelAnimator;
        
        public TunnelForward tunnelForward;
        public List<TunnelSlot> targetColors;
        public Transform tunnelModel;
        public TMP_Text leftCount;
        public ShooterTile myTile;
        public ShooterTile targetTile;

        public void GetTargetShooters(List<TunnelSlot> targets, TunnelForward direction)
        {
            tunnelForward = direction;
            targetColors = new List<TunnelSlot>(targets);
        }
        private void Start()
        {
            myTile = GetComponentInParent<ShooterTile>();
            if (tunnelForward == TunnelForward.Front)
            {
                targetTile = myTile.neighborTiles.First();
            }
            else if (tunnelForward == TunnelForward.Left)
            {
                targetTile = myTile.neighborTiles.Find((tile => tile.transform.position.x < transform.position.x));
            }
            else
            {
                targetTile =  myTile.neighborTiles.Find((tile => tile.transform.position.x > transform.position.x));
            }
            
            targetTile.OnTileEmptied += OnTileEmptied;
            tunnelModel.LookAt(targetTile.transform);
            var euler = tunnelModel.localEulerAngles;
            euler.x = 0f;
            tunnelModel.localEulerAngles = euler;
            leftCount.text = targetColors.Count.ToString();
            leftCount.transform.forward = Camera.main.transform.forward;
        }

        private void OnTileEmptied()
        {
            if (targetColors.Count >= 1)
            {
                var first = targetColors.First();
                var shooter = Instantiate(GM.Instance.shooterPrefab, targetTile.transform);
                GM.Instance.currentLevel.AddShooterDefault(shooter);
                
                tunnelAnimator.SetTrigger(SpawnShooter);
                shooter.transform.localScale = Vector3.one * 0.01f;
                
                shooter.transform.position = transform.position;
                var seq = DOTween.Sequence();
                
                seq.Join(shooter.transform.DOLocalMove(Vector3.zero, 0.25f));
                seq.Join(shooter.transform.DOScale(1f, 0.25f));
                seq.Append(shooter.transform.DOPunchScale(Vector3.one * 0.2f, 0.1f));
                shooter.SetData(targetTile,first.color,first.count);
                targetTile.currentShooter = shooter;
                targetColors.RemoveAt(0);
                leftCount.text = targetColors.Count.ToString();
                seq.OnComplete((() => shooter.EnableOutline()));
                if (targetColors.Count == 0)
                {
                    leftCount.gameObject.SetActive(false);
                }
            }
            else
            {
                leftCount.gameObject.SetActive(false);
            }
            
        }
    }
}