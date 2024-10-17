using UnityEngine;
using AI.BehaviourTree;

public class WanderStrategy : IStrategy
{
    private readonly Transform _entity;
    private readonly float _speed;
    private readonly float _rotationSpeed;
    private readonly float _maxPitch;

    private readonly Vector2 _randomRangeForDirections = new(0.1f, 0.95f);
    private readonly float _chanceToChangeVerticalDirection = 0.1f;

    private readonly Vector2 _changeIntervalRange = new(2f, 6f);
    private float _changeInterval;
    private float _timeSinceLastChanged;
    private Vector3 _randomDirection;
    private Quaternion _targetRotation;
    private Vector3 _lastPickedDirection;

    private EquallyDistributedWeightedPicker<Vector3> _directionPicker;
    private EquallyDistributedWeightedPicker<Quaternion> _verticalDirectionPicker;


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

    public WanderStrategy(Transform entity, float speed, float rotationSpeed, float maxPitch)
    {
        _entity = entity;
        _speed = speed;
        _rotationSpeed = rotationSpeed;
        _maxPitch = maxPitch;
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

        _targetRotation = Quaternion.Euler(_targetRotation.eulerAngles.x, _targetRotation.eulerAngles.y, 0);
        Vector3 forwardDirection = _entity.forward * (_speed * Time.deltaTime);


        _entity.position += forwardDirection;

        _entity.rotation = Quaternion.Slerp(
            _entity.rotation,
            _targetRotation,
            _rotationSpeed * Time.deltaTime
        );

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
            
            _targetRotation = _verticalDirectionPicker.Pick();

            float randomInterpolationFactor = Random.Range(0.1f, 0.5f);

            _targetRotation = Quaternion.Slerp(
                _entity.rotation,
                _targetRotation,
                randomInterpolationFactor
            );

            Vector3 eulerAngles = _targetRotation.eulerAngles;
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, -_maxPitch, _maxPitch);
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
}
