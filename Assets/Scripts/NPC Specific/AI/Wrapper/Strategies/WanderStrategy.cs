using UnityEngine;
using AI.BehaviourTree;

public class WanderStrategy : IStrategy
{
    private readonly Transform _entity;
    private readonly float _speed;
    private readonly float _rotationSpeed;
    private readonly float _changeInterval;
    private readonly float _maxPitch;


    private float _timeSinceLastChanged;
    private Vector3 _randomDirection;
    private Quaternion _targetRotation;
    private Vector3 _lastPickedDirection;

    private EquallyDistributedWeightedPicker<Vector3> _directionPicker;

    private Vector3[] Directions => new[]
    {
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right,
        Vector3.back,
        Vector3.forward
    };

    public WanderStrategy(Transform entity, float speed, float rotationSpeed, float changeInterval, float maxPitch)
    {
        _entity = entity;
        _speed = speed;
        _rotationSpeed = rotationSpeed;
        _changeInterval = changeInterval;
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
            _randomDirection = GetRandomDirection();
            _targetRotation = Quaternion.LookRotation(_randomDirection);

            float randomInterpolationFactor = Random.Range(0.1f, 0.95f);

            _targetRotation = Quaternion.Slerp(
                _entity.rotation,
                _targetRotation,
                randomInterpolationFactor
            );

            Vector3 eulerAngles = _targetRotation.eulerAngles;
            eulerAngles.x = Mathf.Clamp(eulerAngles.x, -_maxPitch, _maxPitch);
            _targetRotation = Quaternion.Euler(eulerAngles);

            _timeSinceLastChanged = 0f;
        }


        Vector3 forwardDirection = _entity.forward * (_speed * Time.deltaTime);


        _entity.position += forwardDirection;

        _entity.rotation = Quaternion.Lerp(
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
}
