using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityExtensions;
using Random = UnityEngine.Random;

public class UIM : MonoSingleton<UIM>
{
    
    public GameObject levelCompletedMenu;
    public Button levelCompletedButton;
    public GameObject[] levelFailedButtons;
    public GameObject levelFailedMenu;
    public TMP_Text[] levelTexts;
    public TMP_Text moneyText;
    public TMP_Text moveText;
    public Transform[] moneyAnimationTransforms;
    public TMP_Text timerText;
    public DOTweenAnimation clockAnim;
    public TMP_Text moneyRewardText;
    public TMP_Text failedCostText;
    public TMP_Text buyText;

    
    void Start()
    {
        moneyText.text = GM.money.ToString();
        failedCostText.text = GM.Instance.failedCost.ToString();
        buyText.text = "+"+GM.Instance.buyAmount.ToString();
        moneyRewardText.text = GM.Instance.currentLevel.moneyReward.ToString();
        levelTexts[0].text = "LEVEL " + (GM.level + 1);
        levelTexts[1].text = "LEVEL " + (GM.level + 1);
        if(GM.Instance.timerBased)
            UpdateTimer();
        else
            UpdateMove();
    }

    public void LevelCompleted()
    {
        AudioManager.Instance.PlayAudio("Won");
        GM.money += GM.Instance.currentLevel.moneyReward;
        levelCompletedMenu.Show();
    }

    public void NextLevel()
    {
        levelCompletedButton.interactable = false;
        AudioManager.Instance.PlayAudio("Coin");
        UpdateMoney();
        FlyMoney();
        this.Delay(GM.Instance.ReloadLevel,1);
    }
    
    public void LevelFailed()
    {
        //AudioManager.Instance.PlayAudio("Failed");
        levelFailedButtons[0].SetActive(GM.money >= GM.Instance.failedCost);
        levelFailedButtons[1].SetActive(GM.money < GM.Instance.failedCost);
        levelFailedButtons[0].transform.parent.GetComponent<Button>().interactable = GM.money >= GM.Instance.failedCost;
        levelFailedMenu.Show();
        moveText.color = Color.red;
    }

    public void UpdateMove()
    {
        moveText.text = "" + GM.Instance.currentLevel.moveCount;
    }

    public void UpdateTimer()
    {
        int timeLeft = GM.Instance.currentLevel.timer;
        int minutes = timeLeft / 60;
        int seconds = timeLeft % 60;
        timerText.text = minutes.ToString("0") + ":" + seconds.ToString("00");

        timerText.color = Color.white;
        if (timeLeft == 10)
        {
            timerText.color = Color.red;
            //AudioManager.Instance.Play("beep");
        }
    }
#region MoneyVisualiser

    public void UpdateMoney()
    {
        moneyText.transform.parent.DOKill();
        moneyText.transform.parent.DOScale(1.2f, 0.25f);
        moneyText.transform.parent.DOScale(1f, 0.25f).SetEase(Ease.InSine).SetDelay(0.85f);
        DOVirtual.Int(int.Parse(moneyText.text), GM.money, 0.5f, MoneyAnimation).SetDelay(0.25f);
    }

    void MoneyAnimation(int value)
    {
        moneyText.text = value.ToString();
    }

    void FlyMoney()
    {
        for (int a = 0; a < moneyAnimationTransforms.Length; a++)
        {
            Vector3 randomStart = levelCompletedButton.transform.position + Random.insideUnitSphere * 0.1f;
            Vector3 randomEnd = moneyText.transform.position + Random.insideUnitSphere * 0.1f;
            moneyAnimationTransforms[a].transform.position = randomStart;
            moneyAnimationTransforms[a].DOMove(randomEnd,0.25f).OnStart(moneyAnimationTransforms[a].Show)
                .OnComplete(moneyAnimationTransforms[a].Hide).SetDelay(a*0.05f);
        }
    }
    
#endregion
    
}