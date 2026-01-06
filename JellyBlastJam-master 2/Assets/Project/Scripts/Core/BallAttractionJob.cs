using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct BallAttractionJob : IJobParallelFor
{
    public float deltaTime;
    public float attractionForce;
    public float maxAttractionDistance;
    public float minDistanceBetweenBalls;
    public float springStiffness;
    public float damping;

    public float3 center;

    [ReadOnly] public NativeArray<float3> positions;
    [ReadOnly] public NativeArray<float3> velocities;
    public NativeArray<float3> newVelocities;

    public void Execute(int index)
    {
        var pos = positions[index];
        var vel = velocities[index];

        var toCenter = center - pos;
        var distance = math.length(toCenter);

        var attraction = float3.zero;

        if (distance < maxAttractionDistance)
        {
            var dir = math.normalize(toCenter);
            var forceFactor = (maxAttractionDistance - distance) / maxAttractionDistance;
            var springForce = springStiffness * forceFactor;

            attraction = dir * springForce - vel * damping;
        }

        var repulsion = float3.zero;

        for (var j = 0; j < positions.Length; j++)
        {
            if (j == index) continue;

            var otherPos = positions[j];
            var delta = pos - otherPos;
            var dist = math.length(delta);

            if (dist < minDistanceBetweenBalls && dist > 0f)
            {
                var pushDir = delta / dist;
                var pushStrength = (minDistanceBetweenBalls - dist) * attractionForce;
                repulsion += pushDir * pushStrength;
            }
        }

        var totalForce = attraction + repulsion;
        var newVelocity = vel + totalForce * deltaTime;

        newVelocities[index] = newVelocity;
    }
}