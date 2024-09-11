using UnityEngine;

namespace BiggestFish.Gameplay
{
    public class MeatObject : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 30.0f; // Speed at which the meat rotates
        
        private bool _consumed = false;
        private float _spawnTime;

        private void Start()
        {
            // Apply random rotation to the meat
            transform.rotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));
            _spawnTime = Time.time;
        }


        private void Update()
        {
            // Rotate the meat slowly
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
        }

        private void DestroyObjectRPC()
        {
            Destroy(gameObject);
        }
    }
}