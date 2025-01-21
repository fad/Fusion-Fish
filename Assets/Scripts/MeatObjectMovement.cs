using Fusion;
using StylizedWater2;
using UnityEngine;


public class MeatObjectMovement : NetworkBehaviour
{ 
    [SerializeField] private float rotationSpeed = 30.0f; // Speed at which the meat rotates
    private float _waterLevelY;
    
    private WaterGrid _waterGrid;
    
    private void Start()
    {
        _waterGrid = FindObjectOfType<WaterGrid>();

        if (_waterGrid)
        {
            _waterLevelY = _waterGrid.transform.position.y - 0.5f;
        }
        

        // Apply random rotation to the meat
        transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
    }
    
    private void Update()
    {
        if (_waterGrid && transform.position.y > _waterLevelY)
        {
            transform.position -= new Vector3(0,0.05f,0);
        }

        // Rotate the meat slowly
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}