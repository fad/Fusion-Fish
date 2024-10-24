using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISatietyManager 
{
    public void RecoverySatiety(float SatietyCount);
    public float GetSatiety();
    public float GetMaxSatiety();

}
