using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Project.Scripts.Core;
using TMPro;
using UnityExtensions;

namespace Project.Scripts.Managers.Core
{
    using UnityEngine;

    [SelectionBase]
    public class Shooter : MonoBehaviour, IClickable
    {
        private static readonly int Walking = Animator.StringToHash("Walking");
        private static readonly int Shooting = Animator.StringToHash("Shooting");
        private static readonly int CanMove = Animator.StringToHash("CanMove");
        private static readonly int Win = Animator.StringToHash("Win");
        private static readonly int Fail = Animator.StringToHash("Fail");
        private static readonly int Disappear = Animator.StringToHash("Disappear");
        private static readonly int Reveal = Animator.StringToHash("Reveal");
        [SerializeField] private float moveSpeed = 5f;
        public ShooterState currentState = ShooterState.Idling;
        public Tile myTile;
        public BallColor shooterColor;
        public List<Renderer> renderers;
        public List<Tile> directions;
        public float fireRate = 1f;
        private float nextFireTime = 0f;
        private Ball targetBall = null;
        private bool isHidden = false;
        public int bulletCount = 10;
        [SerializeField] private Transform cantMoveIcon;
        [SerializeField] private Transform headTransform;

        [SerializeField] private List<Renderer> dummyBulletRenderers;
        [SerializeField] private Transform firePoint;
        public Animator shooterAnimator;
        [SerializeField] private TMP_Text bulletCountText;

        public int shootUntilCount;
        private bool exploded = false;
        [HideInInspector] public bool currentlyShooting;

        private void Start()
        {
            shootUntilCount = int.MaxValue;

            foreach (var r in renderers)
            {
                r.material.SetFloat("_OutlineWidth", 0f);
            }

            foreach (var dummyBulletRenderer in dummyBulletRenderers)
            {
                dummyBulletRenderer.material.SetFloat("_OutlineWidth", 0f);
            }
        }

        public void DummyBulletColorChanger()
        {
            foreach (var dummyBulletRenderer in dummyBulletRenderers)
            {
                dummyBulletRenderer.material = isHidden
                    ? GM.Instance.GetHiddenColor().colorMaterial
                    : GM.Instance.GetShooterColor(shooterColor).colorMaterial;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Ball ball) && currentState == ShooterState.Failed && !exploded)
            {
                Explode();
            }

            var tile = other.GetComponentInParent<StandingTile>();

            if (tile)
            {
                shooterAnimator.transform.DOLocalMoveY(0.5f, 0.1f);
            }
        }

        private void Update()
        {
            if (currentState != ShooterState.Shooting || bulletCount <= shootUntilCount)
            {
                if (currentlyShooting)
                {
                    currentlyShooting = false;
                    transform.DOLocalRotate(Vector3.zero, 0.1f);
                }
                shooterAnimator.SetBool(Shooting, false);
               
                return;
            }

            if (Time.time >= nextFireTime)
            {
                targetBall = FindNearestTarget();

                if (targetBall != null)
                {
                    currentlyShooting = true;
                    FireAtTarget(targetBall);
                    HideDummyBullets();
                    shooterAnimator.SetBool(Shooting, true);
                }

                nextFireTime = Time.time + fireRate;
            }
        }

        private void Explode()
        {
            shooterAnimator.SetTrigger(Fail);
            exploded = true;
            var explosionForce = 10f;
            var upwardModifier = 2f;
            var torqueForce = 5f;
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            var explosionDirection = (Vector3.up * upwardModifier + Vector3.back).normalized;
            rb.AddForce(explosionDirection * explosionForce, ForceMode.Impulse);

            var randomTorque = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            ).normalized * torqueForce;

            rb.AddTorque(randomTorque, ForceMode.Impulse);
        }

        public void EnableOutline()
        {
            directions = GM.Instance.currentLevel.TryFindPath((ShooterTile)myTile);
            if (directions == null) return;

            if (isHidden)
            {
                isHidden = false;
                shooterAnimator.SetTrigger(Reveal);
                SetColorLerp(shooterColor, 0.25f);
                DummyBulletColorChanger();
                bulletCountText.DOText(bulletCount.ToString(), 0.25f);
            }
            else
            {
                shooterAnimator.SetTrigger(CanMove);
            }
            
            foreach (var r in renderers)
            {
                r.material.SetFloat("_OutlineWidth", 4f);
            }
        }

        private void HideDummyBullets()
        {
            var currentlyActiveRenderers = dummyBulletRenderers.FindAll((renderer1 => renderer1.gameObject.activeSelf));
            var rnd = currentlyActiveRenderers.GetRandom();
            rnd.gameObject.SetActive(false);
            if (bulletCount % 10 == 0)
            {
                for (var i = 0; i < dummyBulletRenderers.Count; i++)
                {
                    var dummyBulletRenderer = dummyBulletRenderers[i];
                    dummyBulletRenderer.gameObject.SetActive(true);
                    dummyBulletRenderer.transform.DOScale(0f, 0.05f).From().SetEase(Ease.InBack).SetDelay(i * 0.05f);
                }
            }
        }

        private void SetColor(Material material)
        {
            foreach (var r in renderers)
            {
                r.material = material;
            }
        }

        public void SetData(ShooterTile tile, BallColor valueColor, int valueBulletCount, bool hidden = false)
        {
            myTile = tile;
            isHidden = hidden;
            shooterColor = valueColor;
            bulletCount = valueBulletCount;
            if (isHidden)
            {
                SetColor(GM.Instance.GetHiddenColor().colorMaterial);
                DummyBulletColorChanger();
                bulletCountText.text = "?";
                return;
            }

            bulletCountText.text = bulletCount.ToString();
            SetColor(GM.Instance.GetShooterColor(valueColor).colorMaterial);
            DummyBulletColorChanger();
        }

        private void SetColorLerp(BallColor colorSet, float time)
        {
            var to = GM.Instance.GetShooterColor(colorSet).colorMaterial;
            var seq = DOTween.Sequence();
            var toColor = to.GetColor("_BaseColor");
            var toHColor = to.GetColor("_HColor");
            var toSColor = to.GetColor("_SColor");
            foreach (var mRenderer in renderers)
            {
                seq.Join(mRenderer.material.DOColor(toColor, "_BaseColor", time));
                seq.Join(mRenderer.material.DOColor(toHColor, "_HColor", time));
                seq.Join(mRenderer.material.DOColor(toSColor, "_SColor", time));
            }

            seq.OnComplete((() =>
            {
                SetColor(to);
                foreach (var r in renderers)
                {
                    r.material.SetFloat("_OutlineWidth", 4f);
                }
            }));
        }

        private void FireAtTarget(Ball target)
        {
            var bullet = PoolManager.Instance.GetBullet(firePoint);

            if (bullet != null)
            {
                bullet.ShootAtTarget(target);
                bullet.SetColor(shooterColor);
                AudioManager.Instance.PlayPitchedAudioRandom("Shoot");
                Taptic.Light();
            }

            bulletCount -= 1;
            bulletCountText.text = bulletCount.ToString();
            GM.Instance.currentLevel.RemoveBallCount(shooterColor);
            if (bulletCount <= 0)
            {
                bulletCountText.gameObject.SetActive(false);
                currentState = ShooterState.Exiting;
                GM.Instance.currentLevel.RemoveShooter(this);
                shooterAnimator.SetTrigger(Disappear);
                transform.DOScale(0f, 0.5f).SetDelay(0.25f);
            }
        }

        public void Click()
        {
            TryFindPath();
        }

        private Ball FindNearestTarget()
        {
            var ballGroups =
                GM.Instance.currentLevel.spawnedGroup.FindAll(
                    (group => group.ballColor == shooterColor && group.spawned && group.shootable));
            if (ballGroups.Count <= 0) return null;
            var allBalls = (from @group in ballGroups
                from groupBall in @group.balls
                where !groupBall.targetBy
                select groupBall).ToList();
            if (allBalls.Count <= 0) return null;

            Ball nearest = null;
            var minSqrDistance = float.MaxValue;
            var currentPosition = transform.position;

            for (var i = 0; i < allBalls.Count; i++)
            {
                var ball = allBalls[i];
                var sqrDistance = (ball.transform.position - currentPosition).sqrMagnitude;

                if (!(sqrDistance < minSqrDistance)) continue;
                minSqrDistance = sqrDistance;
                nearest = ball;
            }

            if (nearest)
            {
                nearest.targetBy = this;
                DOTween.Kill(GetInstanceID() + "PunchScale", false);
                DOTween.Kill(GetInstanceID() + "LookAtTarget", false);
                transform.DOLookAt(nearest.transform.position, fireRate, AxisConstraint.X | AxisConstraint.Z)
                    .SetId(GetInstanceID() + "LookAtTarget");
                headTransform.DOPunchScale(Vector3.one * 0.1f, 0.1f).SetId(GetInstanceID() + "PunchScale");
            }

            return nearest;
        }

        private void TryFindPath()
        {
            var manager = GM.Instance;
            var level = manager.currentLevel;
            var standingTile = level.standingGrid.FirstEmptyTile();
            if (standingTile)
            {
                directions = level.TryFindPath((ShooterTile)myTile);

                if (directions == null)
                {
                    if (myTile is ShooterTile { } tile)
                    {
                        var neighbors = tile.neighborTiles.FindAll((tile1 => tile1.isBlocked));
                        foreach (var neighbor in neighbors)
                        {
                            neighbor.Shake();
                        }
                    }

                    CantMoveFeedback();
                    return;
                }

                currentState = ShooterState.GoingPendingArea;
                if (myTile is ShooterTile { } shooterTile)
                {
                    shooterTile.currentShooter = null;
                    shooterTile.OnTileEmptied?.Invoke();
                    level.CheckLeftoversPath();
                }

                standingTile.SetShooter(this);
                var targetPositions = directions.Select((tile => tile.transform.position)).ToList();
                directions.Add(standingTile);

                var xDelta = transform.position.x - standingTile.transform.position.x;
                var t = Mathf.InverseLerp(-12f, 12f, xDelta);
                var standingTileOffset = Vector3.Lerp(Vector3.left * 2f, Vector3.right * 2f, t);
                var shooterTileOffset = Vector3.Lerp(Vector3.right, Vector3.left, t);

                foreach (var tile in directions)
                {
                    if (tile is not ShooterTile { ExitTile: true } exitTile) continue;
                    targetPositions.Add(exitTile.exitPosition.position + shooterTileOffset);
                    break;
                }

                targetPositions.Add(standingTile.enterPosition.position + standingTileOffset);
                targetPositions.Add(standingTile.transform.position);

                shooterAnimator.SetBool(Walking, true);
                Tween path = null;
                path = transform.DOPath(targetPositions.ToArray(), moveSpeed, PathType.CatmullRom, PathMode.Full3D, 50,
                        Color.black)
                    .SetEase(Ease.Linear)
                    .SetSpeedBased()
                    .SetLookAt(0.01f)
                    .OnUpdate((
                        () =>
                        {
                            if (path.ElapsedPercentage() >= 0.95f)
                            {
                                shooterAnimator.SetBool(Walking, false);
                            }
                        }))
                    .OnComplete((() =>
                    {
                        currentState = ShooterState.Pending;
                        myTile = standingTile;
                        level.AddShooterGrid(this);

                        if (GM.Instance.state != "Lost")
                        {
                            transform.DOLocalRotate(Vector3.zero, 0.1f);
                            currentState = ShooterState.Shooting;
                            shooterAnimator.SetBool(Shooting, true);
                        }
                    }));
            }
            else
            {
                CantMoveFeedback();
            }
        }

        private void CantMoveFeedback()
        {
            var id = "CantMove" + GetInstanceID();
            DOTween.Kill(id, true);
            var seq = DOTween.Sequence().SetId(id);
            seq.Join(cantMoveIcon.DOScale(1f, 0.5f).SetEase(Ease.OutBounce));
            seq.Join(cantMoveIcon.GetChild(0).DOLocalMoveY(1f, 0.5f));
            seq.Join(transform.DOPunchScale(Vector3.one * 0.2f, 0.25f));
            seq.Append(cantMoveIcon.DOPunchScale(Vector3.one * 0.1f, 0.25f));
            seq.Append(cantMoveIcon.DOScale(0f, 0.1f));
            seq.OnComplete((() => cantMoveIcon.GetChild(0).transform.localPosition = Vector3.zero));
            Taptic.Warning();
        }

        public void Stop()
        {
            currentState = ShooterState.Failed;
            bulletCountText.gameObject.SetActive(false);
            shooterAnimator.SetTrigger(Fail);
        }

        public void Success()
        {
            currentState = ShooterState.Idling;
            shooterAnimator.SetTrigger(Win);
        }
    }
}