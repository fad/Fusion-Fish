using UnityEngine;

public interface ITreeRunner
{
    /// <summary>
    /// The FishType of this object.
    /// </summary>
    FishData FishType { get; }
    
    /// <summary>
    /// Sends over important information about the area check to the tree runner.
    /// </summary>
    /// <param name="areaCheck">Information about whether the fish got inside the area and the direction from the fish to the area.</param>
    void AdjustAreaCheck((bool isInside, Vector3 direction) areaCheck);
}
