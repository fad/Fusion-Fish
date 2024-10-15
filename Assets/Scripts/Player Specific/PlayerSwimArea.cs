using UnityEngine;

public class PlayerSwimArea : MonoBehaviour
{
    public float swimLength;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(swimLength, 0, swimLength));
    }
}
