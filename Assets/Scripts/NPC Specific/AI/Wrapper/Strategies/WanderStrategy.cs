using System;
using UnityEngine;
using AI.BehaviourTree;
using Random = UnityEngine.Random;

public class WanderStrategy : MoveStrategy
{
    private readonly Vector2 _randomRangeForDirections = new(0.1f, 0.95f);
    private readonly float _chanceToChangeVerticalDirection = 0.1f;

    private readonly Vector2 _changeIntervalRange = new(2f, 6f);
    private readonly float _smoothTime = .4f;
    private float _changeInterval;
    private float _timeSinceLastChanged;
    private Vector3 _randomDirection;
    private Vector3 _lastPickedDirection;

    private EquallyDistributedWeightedPicker<Vector3> _directionPicker;
    private EquallyDistributedWeightedPicker<Quaternion> _verticalDirectionPicker;

    private float _currentXRotationSpeed;
    private float _currentYRotationSpeed;
    private float _currentZRotationSpeed;

    public class Builder
    {
        public Transform Entity;
        public float Speed;
        public float RotationSpeed;
        public float MaxPitch;
        public LayerMask ObstacleAvoidanceLayerMask;
        public float ObstacleAvoidanceDistance;
        public Func<(bool, Vector3)> ForbiddenAreaCheck;
        public bool UseForward;
        public Action<float> SpeedChangeCallback;

        public Builder(Transform entity)
        {
            Entity = entity;
        }

        public Builder WithSpeed(float speed)
        {
            Speed = speed;
            return this;
        }

        public Builder WithRotationSpeed(float rotationSpeed)
        {
            RotationSpeed = rotationSpeed;
            return this;
        }

        public Builder WithMaxPitch(float maxPitch)
        {
            MaxPitch = maxPitch;
            return this;
        }

        public Builder WithObstacleAvoidanceLayerMask(LayerMask obstacleAvoidanceLayerMask)
        {
            ObstacleAvoidanceLayerMask = obstacleAvoidanceLayerMask;
            return this;
        }

        public Builder WithObstacleAvoidanceDistance(float obstacleAvoidanceDistance)
        {
            ObstacleAvoidanceDistance = obstacleAvoidanceDistance;
            return this;
        }

        public Builder WithForbiddenAreaCheck(Func<(bool, Vector3)> forbiddenAreaCheck)
        {
            ForbiddenAreaCheck = forbiddenAreaCheck;
            return this;
        }

        public Builder WithUseForward(bool useForward)
        {
            UseForward = useForward;
            return this;
        }
        
        public Builder WithSpeedChangeCallback(Action<float> speedChangeCallback)
        {
            SpeedChangeCallback = speedChangeCallback;
            return this;
        }

        public WanderStrategy Build()
        {
            return new WanderStrategy(this);
        }
    }


    private Vector3[] Directions => new[]
    {
        Vector3.left,
        Vector3.right,
        Vector3.back,
        Vector3.forward
    };

    private Quaternion[] VerticalDirections => new[]
    {
        Quaternion.Euler(90, 0, 0), // Downwards
        Quaternion.Euler(270, 0, 0) // Upwards
    };

    private WanderStrategy(Builder builder) : base(builder.Entity, builder.RotationSpeed, builder.MaxPitch,
        builder.ObstacleAvoidanceLayerMask, builder.ObstacleAvoidanceDistance, builder.ForbiddenAreaCheck,
        builder.UseForward, builder.SpeedChangeCallback)
    {
        Speed = builder.Speed;
        SpeedChangeCallback?.Invoke(Speed);

        ForwardModifier = (short)(builder.UseForward ? 1 : -1);
    }

    /// <summary>
    /// Constantly moves the entity.
    /// </summary>
    /// <returns><see cref="Status.Running"/> since the entity will constantly move.</returns>
    public override Status Process()
    {
        _timeSinceLastChanged += Time.deltaTime;
        AvoidForbiddenArea();

        if (_timeSinceLastChanged >= _changeInterval)
        {
            ChangeDirection();
            ChangeVerticalDirection();
            _changeInterval = Random.Range(_changeIntervalRange.x, _changeIntervalRange.y);

            _timeSinceLastChanged = 0f;
        }

        AvoidObstacles();

        TargetRotation = Quaternion.Euler(TargetRotation.eulerAngles.x, TargetRotation.eulerAngles.y, 0);
        Vector3 forwardDirection = Entity.forward * (ForwardModifier * (Speed * Time.deltaTime));


        Entity.position += forwardDirection;

        float xAngle = Mathf.SmoothDampAngle(Entity.eulerAngles.x, TargetRotation.eulerAngles.x,
            ref _currentXRotationSpeed, _smoothTime) + RotationSpeed * Time.deltaTime;

        float yAngle = Mathf.SmoothDampAngle(Entity.eulerAngles.y, TargetRotation.eulerAngles.y,
            ref _currentYRotationSpeed, _smoothTime) + RotationSpeed * Time.deltaTime;

        float zAngle = Mathf.SmoothDampAngle(Entity.eulerAngles.z, TargetRotation.eulerAngles.z,
            ref _currentZRotationSpeed, _smoothTime) + RotationSpeed * Time.deltaTime;


        Entity.rotation = Quaternion.Euler(xAngle, yAngle, zAngle);

        return Status.Running;
    }

    /// <summary>
    /// Generates a random direction from a set of predefined directions.
    /// </summary>
    /// <returns>
    /// A <see cref="Vector3"/> representing one of the predefined directions.
    /// </returns>
    private Vector3 GetRandomDirection()
    {
        _directionPicker ??= EquallyDistributedWeightedPicker<Vector3> // Lazy initialization
            .Create()
            .WithEqualWeight(8)
            .WithLessLikelyWeight(1)
            .WithItems(Directions)
            .Build();

        return _directionPicker.Pick();
    }


    /// <summary>
    /// Changes the direction of the entity by picking a random direction from a set of predefined directions.
    /// The target rotation is then interpolated towards this new direction using a random interpolation factor.
    /// </summary>
    private void ChangeDirection()
    {
        _randomDirection = GetRandomDirection();
        TargetRotation = Quaternion.LookRotation(_randomDirection);

        float randomInterpolationFactor = Random.Range(_randomRangeForDirections.x, _randomRangeForDirections.y);

        TargetRotation = Quaternion.Slerp(
            Entity.rotation,
            TargetRotation,
            randomInterpolationFactor
        );
    }

    /// <summary>
    /// Changes the vertical direction of the entity with a small chance.
    /// If the vertical direction is changed, it picks a random vertical direction
    /// and interpolates the target rotation towards it.
    /// If not, it flattens the pitch of the target rotation.
    /// </summary>
    private void ChangeVerticalDirection()
    {
        if (Random.value <= _chanceToChangeVerticalDirection)
        {
            _verticalDirectionPicker ??= EquallyDistributedWeightedPicker<Quaternion>
                .Create()
                .WithEqualWeight(10)
                .WithLessLikelyWeight(4)
                .WithItems(VerticalDirections)
                .Build();

            Quaternion verticalDirection = _verticalDirectionPicker.Pick();

            TargetRotation = verticalDirection;

            float randomInterpolationFactor = Random.Range(0.1f, 0.5f);

            TargetRotation = Quaternion.Slerp(
                Entity.rotation,
                TargetRotation,
                randomInterpolationFactor
            );

            Vector3 eulerAngles = TargetRotation.eulerAngles;
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, MaxPitch, 360 - MaxPitch);
            TargetRotation = Quaternion.Euler(eulerAngles);
            return;
        }

        FlattenOutPitch();
    }

    /// <summary>
    /// Resets the pitch (x-axis rotation) of the target rotation to zero.
    /// </summary>
    private void FlattenOutPitch()
    {
        Vector3 targetEuler = TargetRotation.eulerAngles;
        targetEuler.x = 0;
        TargetRotation = Quaternion.Euler(targetEuler);
    }

    /// <summary>
    /// Checks if the entity is inside a forbidden area and adjusts the target rotation to move away from it.
    /// If the entity is inside the forbidden area, the target rotation is inverted to move in the opposite direction.
    /// The change interval is set to the maximum value to avoid frequent direction changes.
    /// </summary>
    protected override void AvoidForbiddenArea()
    {
        (bool isInside, Vector3 direction) result = ForbiddenAreaCheck();

        if (result.isInside)
        {
            TargetRotation = Quaternion.LookRotation(-result.direction, Entity.up);

            // Interval is set to max value to avoid changing direction too often
            _changeInterval = _changeIntervalRange.y;
            _timeSinceLastChanged = 0f;
        }
    }
}
