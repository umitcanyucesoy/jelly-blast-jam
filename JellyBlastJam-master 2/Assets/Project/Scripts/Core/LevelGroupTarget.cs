using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Project.Scripts.Managers.Core
{
    [Serializable]
    public class LevelGroupTarget
    {
        [PropertySpace(15)]
        [GUIColor("@GM.Instance.GetBallColor(ballColor).colorMaterial.color")]
        [EnumToggleButtons, HideLabel]
        public BallColor ballColor = BallColor.Red;

        public Vector2 spawnPosition = Vector2.zero;
        public int ballCount = 50;
    }
}