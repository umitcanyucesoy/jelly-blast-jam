using Project.Scripts.Managers.Core;
using TMPro;
using UnityEngine;

namespace Project.Scripts.Core
{
    public class StandingTile : Tile
    {
        public StandingGrid standingGrid;
        public GameObject enabledGridVisual;
        public GameObject disabledGridVisual;
        public int enablingLevel = 0;
        public Shooter currentShooter;
        public SpriteRenderer stickmanSprite;
        public TMP_Text unlockLevelText;
        public Transform enterPosition;
        public bool IsEnabled => GM.level + 1 >= enablingLevel;

        private void Awake()
        {
            standingGrid = GetComponentInParent<StandingGrid>();
            //enabledGridVisual.SetActive(IsEnabled);
            disabledGridVisual.SetActive(!IsEnabled);
            unlockLevelText.text = $"Level {enablingLevel.ToString()}";
            if (IsEnabled)
            {
                standingGrid.standingTiles.Add(this);
            }
        }

        public void SetShooter(Shooter shooter)
        {
            if (shooter is not null)
            {
                currentShooter = shooter;
                //stickmanSprite.gameObject.SetActive(true);
                //stickmanSprite.transform.DOScale(0f, 0.3f).From().SetEase(Ease.OutBack);
                //stickmanSprite.color = GameManager.Instance.GetColor(stickman.myColor).colorMaterial.color;
            }
            else
            {
                standingGrid.currentShooters.Remove(currentShooter);
                currentShooter = null;
                //stickmanSprite.gameObject.SetActive(false);
            }
        }
    }
}