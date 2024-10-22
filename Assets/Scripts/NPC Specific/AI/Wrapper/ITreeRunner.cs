using UnityEngine;

public interface ITreeRunner
{
    void AdjustAreaCheck((bool isInside, Vector3 direction) areaCheck);
}
