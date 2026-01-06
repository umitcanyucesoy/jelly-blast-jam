using System;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityExtensions;

public class ProgressPanel : MonoBehaviour
{

    [Serializable]
    public class ProgressPointsClass
    {
        public string message;
        public Vector2Int position;
        public Sprite iconImage;
    }
    public ProgressPointsClass[] progressPoints;

    public Image iconImage;
    public Slider progressImage;
    public TMP_Text messageText;
    public TMP_Text percentageText;

    
    void Start()
    {
        int level = GM.level+1;
        ProgressPointsClass point = progressPoints.FirstOrDefault(p=>p.position.x < level && p.position.y >= level);
        if (point == null)
        {
            gameObject.Hide();
            print("No Match");
            return;
        }
        iconImage.sprite = point.iconImage;
        if (point.position.y == level)
            messageText.text = point.message+" ENABLED!";
        else
            messageText.text = point.message;
        float startValue = (level - 1 - point.position.x) / (1f * (point.position.y - point.position.x));
        percentageText.text = "%" + Mathf.RoundToInt(startValue * 100);
        float endValue = (level - point.position.x) / (1f * (point.position.y - point.position.x));
        progressImage.value = startValue;
        progressImage.DOValue(endValue,0.5f).SetDelay(1.5f);
        DOVirtual.Int(Mathf.RoundToInt(startValue*100), Mathf.RoundToInt(endValue*100),
            0.5f,v=>percentageText.text = "%"+v).SetDelay(1.5f);

    }

}