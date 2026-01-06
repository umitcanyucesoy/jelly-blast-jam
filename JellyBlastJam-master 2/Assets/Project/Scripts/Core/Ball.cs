using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Project.Scripts.Managers.Core
{
    public class Ball : MonoBehaviour
    {
        [Serializable]
        public class PieceClass
        {
            public Rigidbody rb;
            public Transform parent;
            public Vector3 defaultPosition;

            public void BreakPart()
            {
                rb.transform.parent = rb.GetComponentInParent<Ball>().transform;
                defaultPosition = rb.transform.localPosition;
                rb.transform.parent = null;
                rb.gameObject.SetActive(true);
                rb.transform.DOScale(0.01f,1f).SetDelay(0.5f).OnComplete((() => {
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.isKinematic = true;
                    rb.transform.parent = parent;
                    rb.transform.localPosition = defaultPosition;
                    rb.transform.localEulerAngles = Vector3.zero;
                    rb.transform.localScale = Vector3.one;
                    rb.gameObject.SetActive(false);
                    rb.isKinematic = false;
                } }));
            }
        }
        
        [SerializeField] private List<PieceClass> breakParts;
        [SerializeField] private bool isBreakable;
        
        public Rigidbody rb;
        public MeshRenderer mRenderer;
        public BallColor myColor;

        public Vector3 baseScale;
        [HideInInspector] public bool droppedOnFloor;
        public BallGroup myGroup;
        public Shooter targetBy;

        private void Awake()
        {
            mRenderer = GetComponent<MeshRenderer>();
            baseScale = transform.localScale;
        }

        private void Start()
        {
            foreach (var pieceClass in breakParts)
            {
                pieceClass.defaultPosition = pieceClass.rb.transform.localPosition;
            }
        }

        private void FixedUpdate()
        {
            var velocity = rb.linearVelocity;
            var speed = velocity.magnitude;

            var stretchAmount = Mathf.Clamp(speed * 0.05f, 0f, 0.3f);

            transform.localScale = Vector3.Lerp(transform.localScale,
                new Vector3(
                    baseScale.x + stretchAmount,
                    baseScale.y - stretchAmount,
                    baseScale.z + stretchAmount),
                Time.fixedDeltaTime * 10f);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.transform.TryGetComponent(out Ball ball))
            {
                if (myGroup && ball.myGroup != myGroup && transform.position.z < -6f)
                {
                    if (ball.myGroup && ball.myGroup.hitGround)
                    {
                        myGroup.ReleaseTension();
                    }
                }
            }
            
        }

        public void LimitMovement()
        {
            if (droppedOnFloor)
            {
                rb.mass = 1000f;
                rb.linearDamping = 1000f;
                rb.angularDamping = 1000f;
                rb.mass = 100f;
                rb.linearDamping = 1f;
                rb.angularDamping = 0.05f;
            }
        }

        public void Die()
        {
            if (isBreakable)
            {
                foreach (var part in breakParts)
                {
                    part.BreakPart();
                }
            }
            myGroup.RemoveBall(this);
            targetBy = null;
            myGroup = null;
            PoolManager.Instance.ReturnBall(this);
            GM.Instance.currentLevel.failChecker.UpdateOnBallDestroyed(this);
        }

        public void Spawn(BallColor ballColor, BallGroup ballGroup)
        {
            SetColor(ballColor);
            myGroup = ballGroup;
        }

        public void SetColor(BallColor color)
        {
            var targetColorSet = GM.Instance.GetBallColor(color);
            myColor = targetColorSet.color;

            var mats = mRenderer.sharedMaterials;
            mats[0] = targetColorSet.colorMaterial;
            mRenderer.sharedMaterials = mats;
            name = $"Ball --> {myColor.ToString()}";
            
            if (isBreakable)
            {
                foreach (var partMesh in breakParts)
                {
                    partMesh.rb.GetComponent<MeshRenderer>().material = targetColorSet.colorMaterial;
                }
            }
        }
    }
}