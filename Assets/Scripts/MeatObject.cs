using Fusion;
using UnityEngine;


public class MeatObject : NetworkBehaviour
{ 
    [SerializeField] private float rotationSpeed = 30.0f; // Speed at which the meat rotates
    
    private void Start()
    {
        // Apply random rotation to the meat
        transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
    }
    
    private void Update()
    {
        // Rotate the meat slowly
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}