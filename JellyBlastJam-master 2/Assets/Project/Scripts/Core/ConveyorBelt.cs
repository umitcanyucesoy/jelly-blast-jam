using System;
using System.Collections.Generic;
using DG.Tweening;
using Project.Scripts.Managers.Core;
using UnityEngine;

namespace Project.Scripts.Core
{
    public class ConveyorBelt : MonoBehaviour
    {
        public float beltSpeed;
        public MeshRenderer beltRenderer;
        public List<BallGroup> collidedGroups = new List<BallGroup>();
        public List<Ball> collidedBalls = new List<Ball>();
        public float speedUpMultiplier = 2f;

        private void Start()
        {
            beltRenderer.material.DOOffset(-Vector2.right, beltSpeed).SetEase(Ease.Linear).SetSpeedBased().SetLoops(-1, LoopType.Incremental);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Ball ball) && !collidedBalls.Contains(ball))
            {
                Debug.Log(ball);
                collidedBalls.Add(ball);
                if (!collidedGroups.Contains(ball.myGroup))
                {
                    collidedGroups.Add(ball.myGroup);
                    ball.myGroup.MultiplyDefaultSpeed(speedUpMultiplier);
                }
            }
        }
    }
}