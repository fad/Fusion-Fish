using UnityEngine;
using AI.BehaviourTree;

public class WanderStrategy : IStrategy
{
    #region Fields provided by FishData

    private readonly Transform _entity;
    private readonly float _speed;
    private readonly float _rotationSpeed;
    private readonly float _maxPitch;
    private readonly LayerMask _obstacleAvoidanceLayerMask;
    private readonly float _obstacleAvoidanceDistance;

    #endregion

    private readonly Vector2 _randomRangeForDirections = new(0.1f, 0.95f);
    private readonly float _chanceToChangeVerticalDirection = 0.1f;

    private readonly Vector2 _changeIntervalRange = new(2f, 6f);
    private readonly float _smoothTime = .4f;
    private float _changeInterval;
    private float _timeSinceLastChanged;
    private Vector3 _randomDirection;
    private Quaternion _targetRotation;
    private Vector3 _lastPickedDirection;

    private EquallyDistributedWeightedPicker<Vector3> _directionPicker;
    private EquallyDistributedWeightedPicker<Quaternion> _verticalDirectionPicker;

    private float _currentXRotationSpeed;
    private float _currentYRotationSpeed;
    private float _currentZRotationSpeed;

    public class Builder
    {
        public Transform entity;
        public float speed;
        public float rotationSpeed;
        public float maxPitch;
        public LayerMask obstacleAvoidanceLayerMask;
        public float obstacleAvoidanceDistance;

        public Builder(Transform entity)
        {
            this.entity = entity;
        }

        public Builder WithSpeed(float speed)
        {
            this.speed = speed;
            return this;
        }

        public Builder WithRotationSpeed(float rotationSpeed)
        {
            this.rotationSpeed = rotationSpeed;
            return this;
        }

        public Builder WithMaxPitch(float maxPitch)
        {
            this.maxPitch = maxPitch;
            return this;
        }

        public Builder WithObstacleAvoidanceLayerMask(LayerMask obstacleAvoidanceLayerMask)
        {
            this.obstacleAvoidanceLayerMask = obstacleAvoidanceLayerMask;
            return this;
        }

        public Builder WithObstacleAvoidanceDistance(float obstacleAvoidanceDistance)
        {
            this.obstacleAvoidanceDistance = obstacleAvoidanceDistance;
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

    private WanderStrategy(Builder builder)
    {
        _entity = builder.entity;
        _speed = builder.speed;
        _rotationSpeed = builder.rotationSpeed;
        _maxPitch = builder.maxPitch;
        _obstacleAvoidanceLayerMask = builder.obstacleAvoidanceLayerMask;
        _obstacleAvoidanceDistance = builder.obstacleAvoidanceDistance;
    }

    /// <summary>
    /// Constantly moves the entity.
    /// </summary>
    /// <returns><see cref="Status.Running"/> since the entity will constantly move.</returns>
    public Status Process()
    {
        _timeSinceLastChanged += Time.deltaTime;

        if (_timeSinceLastChanged >= _changeInterval)
        {
            ChangeDirection();
            ChangeVerticalDirection();
            _changeInterval = Random.Range(_changeIntervalRange.x, _changeIntervalRange.y);
            _timeSinceLastChanged = 0f;
        }

        AvoidObstacles();

        _targetRotation = Quaternion.Euler(_targetRotation.eulerAngles.x, _targetRotation.eulerAngles.y, 0);
        Vector3 forwardDirection = _entity.forward * (_speed * Time.deltaTime);


        _entity.position += forwardDirection;

        float xAngle = Mathf.SmoothDampAngle(_entity.eulerAngles.x, _targetRotation.eulerAngles.x,
            ref _currentXRotationSpeed, _smoothTime) + _rotationSpeed * Time.deltaTime;

        float yAngle = Mathf.SmoothDampAngle(_entity.eulerAngles.y, _targetRotation.eulerAngles.y,
            ref _currentYRotationSpeed, _smoothTime) + _rotationSpeed * Time.deltaTime;

        float zAngle = Mathf.SmoothDampAngle(_entity.eulerAngles.z, _targetRotation.eulerAngles.z,
            ref _currentZRotationSpeed, _smoothTime) + _rotationSpeed * Time.deltaTime;


        _entity.rotation = Quaternion.Euler(xAngle, yAngle, zAngle);

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
        _targetRotation = Quaternion.LookRotation(_randomDirection);

        float randomInterpolationFactor = Random.Range(_randomRangeForDirections.x, _randomRangeForDirections.y);

        _targetRotation = Quaternion.Slerp(
            _entity.rotation,
            _targetRotation,
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

            _targetRotation = verticalDirection;

            float randomInterpolationFactor = Random.Range(0.1f, 0.5f);

            _targetRotation = Quaternion.Slerp(
                _entity.rotation,
                _targetRotation,
                randomInterpolationFactor
            );

            Vector3 eulerAngles = _targetRotation.eulerAngles;
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, _maxPitch, 360 - _maxPitch);
            _targetRotation = Quaternion.Euler(eulerAngles);
            return;
        }

        FlattenOutPitch();
    }

    /// <summary>
    /// Resets the pitch (x-axis rotation) of the target rotation to zero.
    /// </summary>
    private void FlattenOutPitch()
    {
        Vector3 targetEuler = _targetRotation.eulerAngles;
        targetEuler.x = 0;
        _targetRotation = Quaternion.Euler(targetEuler);
    }

    private void AvoidObstacles()
    {
        if (Physics.Raycast(_entity.position, _entity.forward, out RaycastHit hit, _obstacleAvoidanceDistance,
                _obstacleAvoidanceLayerMask))
        {
            _targetRotation = Quaternion.LookRotation(Vector3.Reflect(_entity.forward, hit.normal));
        }
    }
}
