using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MonKey.Extensions;
using Project.Scripts.Managers;
using Project.Scripts.Managers.Core;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using ListExtensions = UnityExtensions.ListExtensions;
using Random = UnityEngine.Random;

public class BallGroup : MonoBehaviour
{
    [SerializeField] private Rigidbody mRigidBody;
    [Header("Ball Settings")] public BallColor ballColor;
    public int ballCount = 20;
    public float spawnRadius = 3f;

    public List<Ball> balls = new List<Ball>();

    [Header("Physics Settings")] public float attractionForce = 10f;
    public float maxAttractionDistance = 10f;
    public float minDistanceBetweenBalls = 1.5f;
    public float springStiffness = 100f;
    public float damping = 2f;

    private NativeArray<float3> positions;
    private NativeArray<float3> velocities;
    private NativeArray<float3> newVelocities;
    private bool disposing;
    public bool spawned = false;
    public bool shootable = false;
    [HideInInspector] public bool hitGround = false;
    public UnityAction<BallGroup> onGroupSpawned;
    [SerializeField] private float dragDefault = 6f;
    [SerializeField] private float dragOnCloseHit;
    [SerializeField] private float dragOnDistanceHit;
    [SerializeField] private float dragChangeTime = 0.5f;

    [Header("Attraction Setting On Floor")] [SerializeField] private float targetAttractionForce = 5f;

    [SerializeField] private float targetAttractionDistance = 1f;
    [SerializeField] private float targetBallDrag = 0.1f;
    private Vector3 dragRatioCalcStartPosition;
    private Vector3 dragRatioCalcEndPosition;
    private float dragOnCloseHitDefault;

    private void Start()
    {
        dragOnCloseHitDefault = dragOnCloseHit;
        mRigidBody.isKinematic = false;
        mRigidBody.linearDamping = dragDefault;
        dragRatioCalcEndPosition = transform.parent.position;
    }

    public void SetData(BallColor color, int count)
    {
        ballColor = color;
        ballCount = count;
    }

    public void PreSpawnBalls()
    {
        SpawnBalls();
        shootable = true;
    }

    private void SpawnBalls()
    {
        var poolManager = PoolManager.Instance;
        for (var i = 0; i < ballCount; i++)
        {
            var spawnPos = transform.position + Random.insideUnitSphere * spawnRadius;
            spawnPos.y = transform.position.y;

            var ball = poolManager.GetBall(this, ballColor, spawnPos);
            balls.Add(ball);
        }

        positions = new NativeArray<float3>(balls.Count, Allocator.Persistent);
        velocities = new NativeArray<float3>(balls.Count, Allocator.Persistent);
        newVelocities = new NativeArray<float3>(balls.Count, Allocator.Persistent);

        onGroupSpawned?.Invoke(this);
        spawned = true;
        dragRatioCalcStartPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag($"BallSpawnerTarget") && !spawned)
        {
            SpawnBalls();
        }

        if (other.transform.CompareTag($"BallShootableTarget") && !shootable)
        {
            shootable = true;
        }
    }

    private void FixedUpdate()
    {
        if (positions.IsCreated && velocities.IsCreated && newVelocities.IsCreated && !disposing && spawned)
        {
            for (var i = 0; i < balls.Count; i++)
            {
                positions[i] = balls[i].transform.position;
                velocities[i] = balls[i].rb.linearVelocity;
            }

            var job = new BallAttractionJob
            {
                deltaTime = Time.fixedDeltaTime,
                attractionForce = attractionForce,
                maxAttractionDistance = maxAttractionDistance,
                minDistanceBetweenBalls = minDistanceBetweenBalls,
                springStiffness = springStiffness,
                damping = damping,
                center = transform.position,
                positions = positions,
                velocities = velocities,
                newVelocities = newVelocities
            };

            var handle = job.Schedule(balls.Count, 1);
            handle.Complete();

            for (var i = 0; i < balls.Count; i++)
            {
                balls[i].rb.linearVelocity = newVelocities[i];
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (var i = 0; i < Random.Range(1, 5); i++)
            {
                var ball = ListExtensions.GetRandom(balls);
                RemoveBall(ball);
            }
        }
    }

    public void RemoveBall(Ball ballToRemove)
    {
        if (ballToRemove != null)
        {
            StopCoroutine("DragOnHitCoroutine");
            StartCoroutine("DragOnHitCoroutine");
            disposing = true;
            balls.Remove(ballToRemove);
            ShakeGroup(ballToRemove);

            positions.Dispose();
            velocities.Dispose();
            newVelocities.Dispose();

            positions = new NativeArray<float3>(balls.Count, Allocator.Persistent);
            velocities = new NativeArray<float3>(balls.Count, Allocator.Persistent);
            newVelocities = new NativeArray<float3>(balls.Count, Allocator.Persistent);


            for (var i = 0; i < balls.Count; i++)
            {
                positions[i] = balls[i].transform.position;
                velocities[i] = balls[i].rb.linearVelocity;
            }

            disposing = false;

            if (balls.Count == 0)
            {
                GM.Instance.currentLevel.spawnedGroup.Remove(this);
                if (positions.IsCreated) positions.Dispose();
                if (velocities.IsCreated) velocities.Dispose();
                if (newVelocities.IsCreated) newVelocities.Dispose();
                if (GM.Instance.currentLevel.GroupsFinished)
                {
                    GM.Instance.Won();
                }
                else
                {
                    GM.Instance.currentLevel.ChangeGroupSpeeds();
                }

                gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator DragOnHitCoroutine()
    {
        //mRigidBody.AddForce(Vector3.forward * 100f, ForceMode.Force);
        if (GM.Instance.state == "Lost")
        {
            Drop();
            yield break;
        }

        var ratio = transform.InverseLerp(dragRatioCalcStartPosition, dragRatioCalcEndPosition);
        mRigidBody.linearDamping = Mathf.Lerp(dragOnDistanceHit, dragOnCloseHit, ratio);
        yield return new WaitForSeconds(dragChangeTime);
        mRigidBody.linearDamping = dragDefault;
    }

    public void ChangeRbSpeed((float defaultDrag, float dragOnCloseHit) diff)
    {
        if (speedUpBoost) return;
        dragDefault = diff.defaultDrag;
        dragOnCloseHit = diff.dragOnCloseHit;
        mRigidBody.linearDamping = dragDefault;
    }

    public bool speedUpBoost = false;
    public void MultiplyDefaultSpeed(float multiplier)
    {
        speedUpBoost = true;
        dragDefault /= multiplier;
        dragOnCloseHit /= multiplier;
        mRigidBody.linearDamping = dragDefault;
    }

    public void Resume()
    {
        mRigidBody.isKinematic = false;
    }

    public void Stop()
    {
        mRigidBody.isKinematic = true;
    }

    public void Drop()
    {
        mRigidBody.linearDamping = 0.1f;
        foreach (var ball in balls)
        {
            ball.rb.linearDamping = 0.1f; 
            ball.rb.constraints = RigidbodyConstraints.None;
        }
        
        hitGround = true;
        attractionForce = targetAttractionForce;
        maxAttractionDistance = targetAttractionDistance;
    }

    private void OnDestroy()
    {
        if (positions.IsCreated) positions.Dispose();
        if (velocities.IsCreated) velocities.Dispose();
        if (newVelocities.IsCreated) newVelocities.Dispose();
    }

    private void OnDrawGizmos()
    {
        if (spawned) return;
        var color = GM.Instance.GetBallColor(ballColor).colorMaterial.color;
        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position, 1f);
    }

    public void ReleaseTension(bool isFail = false)
    {
        if (!hitGround)
        {
            hitGround = true;
            var attForce = attractionForce;
            var dist = maxAttractionDistance;
            DOVirtual.Float(attForce, targetAttractionForce, 1f, value => attractionForce = value);
            DOVirtual.Float(dist, targetAttractionDistance, 1f, value => maxAttractionDistance = value);

            foreach (var ball in balls)
            {
                ball.droppedOnFloor = true;
                
                //var defaultScale = ball.baseScale;
                var damp = ball.rb.linearDamping;
                //ball.rb.angularDamping = 25f;
                //DOVirtual.Vector3(defaultScale, Vector3.one * 2f, 0.25f, value => ball.baseScale = value);
                DOVirtual.Float(damp, targetBallDrag, 1f, value => ball.rb.linearDamping = value);
            }
        }
    }

    public void ShakeGroup(Ball removedBall)
    {
        var refPos = removedBall.transform.position;
        var orderedList = new List<Ball>(balls);
        orderedList.Sort((a, b) =>
        {
            var aDist = (a.transform.position - refPos).sqrMagnitude;
            var bDist = (b.transform.position - refPos).sqrMagnitude;
            return aDist.CompareTo(bDist);
        });


        //DOTween.Kill(GetInstanceID() + "ShakeBalls",false);
        for (var i = 0; i < orderedList.Count; i++)
        {
            var b = orderedList[i];
            if (DOTween.IsTweening(b.GetInstanceID() + "ShakeBalls")) continue;
            b.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f)
                .OnComplete((() => b.transform.localScale = Vector3.one * 2f))
                .SetDelay(i * 0.025f)
                .SetId(b.GetInstanceID() + "ShakeBalls");
        }
    }
}