using Project.Scripts.Managers.Core;
using UnityExtensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Project.Scripts.Managers
{
    public class PoolManager : MonoSingleton<PoolManager>
    {
        [SerializeField] private BallGroup ballGroupPrefab;
        [SerializeField] private string ballPrefabAddress = "Assets/Project/Prefabs/Ball.prefab";
        [SerializeField] private string bulletPrefabAddress = "Assets/Project/Prefabs/Bullet.prefab";
        [SerializeField] private Bullet bulletPrefabOriginal;
        [SerializeField] private Ball ballPrefabOriginal;

        [SerializeField] private int ballPoolSize = 500;
        [SerializeField] private int bulletPoolSize = 500;

        private List<Ball> ballPool = new List<Ball>();
        private List<Bullet> bulletPool = new List<Bullet>();
        private List<Ball> activeBalls = new List<Ball>();
        private List<Bullet> activeBullets = new List<Bullet>();

        [HideInInspector] public bool isInitialized = false;

        private void Awake()
        {
            for (var i = 0; i < ballPoolSize; i++)
            {
                Addressables.InstantiateAsync(ballPrefabAddress).Completed += handle =>
                {
                    var go = handle.Result;
                    go.transform.parent = transform;
                    go.SetActive(false);
                    var ball = go.GetComponent<Ball>();
                    ballPool.Add(ball);

                    if (ballPool.Count == ballPoolSize)
                    {
                        isInitialized = true;
                    }
                };
            }

            for (var i = 0; i < bulletPoolSize; i++)
            {
                Addressables.InstantiateAsync(bulletPrefabAddress).Completed += handle =>
                {
                    var go = handle.Result;
                    go.transform.parent = transform;
                    go.SetActive(false);
                    var bullet = go.GetComponent<Bullet>();
                    bulletPool.Add(bullet);
                };
            }
        }

        public BallGroup SpawnBallGroup(BallColor ballColor, Vector3 pos, int ballCount, Transform parent)
        {
            var ballGroup = Instantiate(ballGroupPrefab, pos, Quaternion.identity, parent);
            ballGroup.SetData(ballColor, ballCount);
            return ballGroup;
        }

        public Bullet GetBullet(Transform firePoint)
        {
            if (bulletPool.Count > 0)
            {
                var bullet = bulletPool[0];
                bulletPool.RemoveAt(0);

                bullet.gameObject.SetActive(true);
                bullet.transform.position = firePoint.position;
                activeBullets.Add(bullet);

                return bullet;
            }

            Debug.LogWarning("Bullet pool exhausted! Adding a new bullet.");

            var newBullet = Instantiate(bulletPrefabOriginal, transform);
            newBullet.transform.position = firePoint.position;
            newBullet.gameObject.SetActive(true);
            activeBullets.Add(newBullet);

            return newBullet;
        }

        public void ReturnBullet(Bullet bullet)
        {
            bullet.gameObject.SetActive(false);
            activeBullets.Remove(bullet);
            bulletPool.Add(bullet);
            bullet.transform.parent = transform;
            bullet.transform.localPosition = Vector3.zero;
        }

        public Ball GetBall(BallGroup group, BallColor color, Vector3 position)
        {
            if (ballPool.Count > 0)
            {
                var ball = ballPool[0];
                ballPool.RemoveAt(0);

                ball.transform.position = position;
                ball.gameObject.SetActive(true);
                ball.Spawn(color, group);
                activeBalls.Add(ball);

                return ball;
            }

            Debug.LogWarning("Ball pool exhausted! Adding a new ball.");

            var newBall = Instantiate(ballPrefabOriginal, transform);
            newBall.gameObject.SetActive(true);
            activeBalls.Add(newBall);
            newBall.Spawn(color, group);
            newBall.transform.position = position;

            return newBall;
        }

        public void ReturnBall(Ball ball)
        {
            ball.gameObject.SetActive(false);
            activeBalls.Remove(ball);
            ballPool.Add(ball);
            ball.transform.parent = transform;
            ball.transform.localPosition = Vector3.zero;
        }

        private void OnApplicationQuit()
        {
            ballPool.Clear();
            bulletPool.Clear();
            activeBalls.Clear();
            activeBullets.Clear();
        }
    }
}