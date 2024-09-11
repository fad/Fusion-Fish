using UnityEngine;

namespace BiggestFish.Gameplay
{
    public class SpawnGibsOnDestroy : MonoBehaviour
    {
        [SerializeField] private GameObject _gibPrefab;
        [SerializeField] private float _gibScale = 1f;
        [SerializeField] private int _gibSpawnCount = 3;
        [SerializeField] private int _nutritionalValue = 1;

        private void OnDestroy()
        {
            if (_gibPrefab != null && _gibSpawnCount > 0)
                SpawnMeatObjects();
        }

        private void SpawnMeatObjects()
        {
            if (_gibPrefab != null)
                for (var i = 0; i < _gibSpawnCount; i++)
                {
                    var randomOffset = Random.insideUnitSphere * 0.5f; // Adjust the offset as needed
                    var spawnPosition = transform.position + randomOffset;
                    var meat = Instantiate(_gibPrefab, spawnPosition, Quaternion.identity);

                    meat.transform.localScale = new Vector3(_gibScale, _gibScale, _gibScale);
                    //meat.GetComponent<Food>().NutritionalValue = _nutritionalValue;
                }
        }
    }
}