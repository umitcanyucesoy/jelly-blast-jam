using System;
using Project.Scripts.Managers.Core;
using Sirenix.OdinInspector;

namespace Project.Scripts.Core
{
    [Serializable]
    public class TunnelSlot
    {
        [GUIColor("@GM.Instance.GetShooterColor(color).colorMaterial.color")]
        [EnumToggleButtons, HideLabel]
        public BallColor color = BallColor.Red;
        public int count = 50;
    }

}