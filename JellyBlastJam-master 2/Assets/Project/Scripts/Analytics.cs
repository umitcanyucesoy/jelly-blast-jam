using System.Collections;
using System.Collections.Generic;
using ElephantSDK;
using UnityEngine;
using UnityExtensions;

public class Analytics : MonoSingleton<Analytics>
{


    public void SendLevelStart()
        {
            Elephant.LevelStarted(GM.level + 1, Params.New().
            Set("money", GM.money).
            Set("originalLevel", GM.Instance.currentLevelIndex + 1));
            Debug.Log("LevelStartEvent" + (GM.level + 1));
        }
    
        public void SendLevelComplete()
        {
            Elephant.LevelCompleted(GM.level + 1, Params.New().
            Set("used_move_count", GM.Instance.currentLevel.moveCount).
            Set("time", Time.timeSinceLevelLoadAsDouble).
            Set("money", GM.money + GM.Instance.currentLevel.moneyReward).
            Set("originalLevel", GM.Instance.currentLevelIndex + 1));
            Debug.Log("SendLevelComplete" + (GM.level + 1) + "At time : " + Time.timeSinceLevelLoad);
        }
    
        public void SendLevelFailed()
        {
            Elephant.LevelFailed(GM.level + 1, Params.New().
            Set("used_move_count", GM.Instance.currentLevel.moveCount).
            Set("time", Time.timeSinceLevelLoadAsDouble).
            Set("money", GM.money).
            Set("originalLevel", GM.Instance.currentLevelIndex + 1));
            Debug.Log("SendLevelFailed" + (GM.level + 1) + "At time : " + Time.timeSinceLevelLoad);
        }
    
        public void TimeMoveBought()
        {
            Elephant.Event("TimeMoveBought", GM.level + 1);
            Debug.Log("TimeMoveBought" + (GM.level + 1));
        }

}