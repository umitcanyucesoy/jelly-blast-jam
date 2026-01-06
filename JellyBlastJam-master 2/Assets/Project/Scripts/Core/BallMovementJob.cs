using Project.Scripts.Managers.Core;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct BallMovementJob : IJobParallelFor
{
    public float DeltaTime;
    public float AttractionForce;
    public float MaxAttractionDistance;
    public float MinDistanceBetweenBalls;
    public float DragMultiplier;
    public float3 CenterPosition;

    [ReadOnly] public NativeArray<BallData> CurrentBalls;
    public NativeArray<BallData> NextBalls;

    public void Execute(int i)
    {
        var ballData = CurrentBalls[i];

        var toCenter = CenterPosition - ballData.Position;
        var distance = math.length(toCenter);

        var acceleration = float3.zero;

        if (distance < MaxAttractionDistance)
        {
            var dir = math.normalize(toCenter);
            var forceFactor = math.saturate((MaxAttractionDistance - distance) / MaxAttractionDistance);
            acceleration += dir * AttractionForce * forceFactor;
        }

        for (var j = 0; j < CurrentBalls.Length; j++)
        {
            if (i == j) continue;
            var otherPos = CurrentBalls[j].Position;
            var between = ballData.Position - otherPos;
            var dist = math.length(between);

            if (dist < MinDistanceBetweenBalls && dist > 0.001f)
            {
                var pushDir = math.normalize(between);
                var pushStrength = (MinDistanceBetweenBalls - dist) * AttractionForce;
                acceleration += pushDir * pushStrength;
            }
        }

        ballData.Velocity *= 1f - (DragMultiplier * DeltaTime);

        ballData.Velocity += acceleration * DeltaTime;
        ballData.Position += ballData.Velocity * DeltaTime;

        NextBalls[i] = ballData;
    }
}