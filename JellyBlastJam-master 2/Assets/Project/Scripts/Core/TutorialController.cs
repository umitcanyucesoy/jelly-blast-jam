using System.Collections;
using System.Linq;
using DG.Tweening;
using Project.Scripts.Managers;
using Project.Scripts.Managers.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Scripts.Core
{
    public class TutorialController : MonoBehaviour
    {
        [FormerlySerializedAs("targetWorldObject")] public BallGroup targetBallGroup;
        public Vector3 offset = Vector3.up;
        public Camera mainCamera;
        public GameObject handParent;
        public RectTransform uiElement;
        public Shooter targetShooter;

        private IEnumerator Start()
        {
            while (!PoolManager.Instance.isInitialized)
            {
                yield return null;
            }
            
            if (GM.level == 0)
            {
                mainCamera = Camera.main;
                yield return null;
                Debug.Log("tutorial 1");
                var level = GM.Instance.currentLevel;
                targetBallGroup = level.spawnedGroup.First();
                var shooters = FindObjectsByType<Shooter>(FindObjectsSortMode.InstanceID);
                foreach (var shooter in shooters)
                {
                    shooter.GetComponentInChildren<BoxCollider>().enabled = false;
                }

                targetShooter = shooters.Last();
                
                if (targetBallGroup == null || uiElement == null || mainCamera == null)
                    yield break;

                yield return new WaitUntil((() => targetBallGroup.transform.position.z <= 14f));
                
                
                targetShooter.transform.DOScale(Vector3.one * 1.1f,0.5f).SetLoops(-1, LoopType.Yoyo).SetId("Tutorial");
                
                targetShooter.GetComponentInChildren<BoxCollider>().enabled = true;
                var allGroups = FindObjectsByType<BallGroup>(FindObjectsSortMode.InstanceID);
                foreach (var ballGroup in allGroups)
                {
                    ballGroup.Stop();
                }
                handParent.SetActive(true);
                var screenPos = mainCamera.WorldToScreenPoint(targetShooter.transform.position + offset);

                if (screenPos.z < 0)
                {
                    uiElement.gameObject.SetActive(false);
                }
                else
                {
                    uiElement.position = screenPos;
                }

                yield return new WaitUntil((() => targetShooter.currentState == ShooterState.GoingPendingArea));
                DOTween.Kill("Tutorial",true);
                handParent.SetActive(false);
                foreach (var ballGroup in allGroups)
                {
                    ballGroup.Resume();
                }
                foreach (var shooter in shooters)
                {
                    shooter.GetComponentInChildren<BoxCollider>().enabled = true;
                }
            }
        }
    }
}