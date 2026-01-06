namespace Project.Scripts.Managers.Core
{
    using UnityEngine;

    public class Bullet : MonoBehaviour
    {
        [SerializeField] private Renderer mRenderer;

        [SerializeField] private Rigidbody rb;
        [SerializeField] private TrailRenderer trailRenderer;

        public Ball targetBall;
        public float speed = 10f;


        public void SetColor(BallColor ballColor)
        {
            mRenderer.material = GM.Instance.GetShooterColor(ballColor).colorMaterial;
            trailRenderer.startColor = mRenderer.material.color;
            trailRenderer.endColor = mRenderer.material.color;
        }

        public void ShootAtTarget(Ball target)
        {
            targetBall = target;
            var pos = transform.position;
            pos.y = targetBall.transform.position.y;
            transform.position = pos;
        }

        private void Update()
        {
            if (targetBall != null)
            {
                var direction = (targetBall.transform.position - transform.position).normalized;
                transform.position += direction * (speed * Time.deltaTime);
                //rb.MovePosition(transform.position + direction * (speed * Time.deltaTime));
                transform.LookAt(targetBall.transform);
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            var ball = other.gameObject.GetComponentInParent<Ball>();
            if (ball && ball == targetBall)
            {
                if (ball == targetBall)
                {
                    targetBall.Die();
                    targetBall = null;
                    trailRenderer.Clear();
                    PoolManager.Instance.ReturnBullet(this);
                }
                else if (ball.droppedOnFloor)
                {
                    ball.LimitMovement();
                }
            }
        }
    }
}