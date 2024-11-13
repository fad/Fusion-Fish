using UnityEngine;

public interface ITreeRunner
{
    /// <summary>
    /// Sends over important information about the area check to the tree runner.
    /// </summary>
    /// <param name="areaCheck">Information about whether the fish got inside the area and the direction from the fish to the area.</param>
    void AdjustAreaCheck((bool isInside, Vector3 direction) areaCheck);

    /// <summary>
    /// Sends over important information about the current detected fish to the behaviour.
    /// </summary>
    /// <param name="targetData">Data needed for successfully traversing into hunt or fleeing behaviour.</param>
    void AdjustHuntOrFleeTarget((Transform targetTransform, IEntity targetBehaviour) targetData);
}
