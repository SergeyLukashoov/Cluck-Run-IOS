using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<GameObject> objectPrefabs; // Префабы объектов для спавна
    public float spawnRadius = 50f; // Радиус спавна вокруг игрока
    public int maxObjects = 10; // Максимальное количество объектов на сцене
    public Vector2 sizeRange = new Vector2(0.1f, 0.5f); // Диапазон размеров объектов
    public float playerSizeCheckMultiplier = 1.2f; // Если объект больше игрока в N раз, его нельзя поглотить
    public float sizeIncrementAfterCollection = 10f; // На сколько может быть больше игрока при спавне после подбора
    public float spawnRadiusMultiplier = 5f; // Множитель для расчета минимального расстояния между объектами

    [Header("Player Reference")]
    public Transform player; // Ссылка на игрока
    public float playerGrowthAmount = 0.1f; // На сколько увеличивается игрок при поглощении

    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Start()
    {
        // Начальный спавн объектов
        for (int i = 0; i < maxObjects; i++)
        {
            SpawnObject();
        }
    }

    private void Update()
    {
        // Если объектов меньше максимума, спавним новый
        if (spawnedObjects.Count < maxObjects)
        {
            SpawnObject();
        }
    }

    private void SpawnObject(bool afterCollection = false)
    {
        if (objectPrefabs.Count == 0 || player == null) return;

        // Выбираем случайный префаб
        GameObject randomPrefab = objectPrefabs[Random.Range(0, objectPrefabs.Count)];
        
        // Пытаемся найти валидную позицию для спавна
        Vector3 spawnPosition;
        bool positionFound = false;
        int attempts = 0;
        int maxAttempts = 30;
        float randomSize = 0f;

        do
        {
            // Случайная позиция в радиусе вокруг игрока
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            spawnPosition = player.position + new Vector3(randomCircle.x, 0, randomCircle.y);

            // Определяем размер объекта
            if (afterCollection)
            {
                float playerSize = player.localScale.x;
                randomSize = Random.Range(playerSize, playerSize + sizeIncrementAfterCollection);
            }
            else
            {
                float playerSize = player.localScale.x;
                
                if (Random.value < 0.7f)
                {
                    randomSize = Random.Range(sizeRange.x, playerSize * playerSizeCheckMultiplier);
                }
                else
                {
                    randomSize = Random.Range(playerSize * playerSizeCheckMultiplier, sizeRange.y);
                }
            }

            // Проверяем, нет ли других объектов слишком близко
            positionFound = true;
            foreach (var obj in spawnedObjects)
            {
                if (obj == null) continue;
                
                float minDistance = (obj.transform.localScale.x + randomSize) * spawnRadiusMultiplier;
                if (Vector3.Distance(spawnPosition, obj.transform.position) < minDistance)
                {
                    positionFound = false;
                    break;
                }
            }

            attempts++;
        } while (!positionFound && attempts < maxAttempts);

        // Если не нашли подходящую позицию после всех попыток, выходим
        if (!positionFound) return;

        // Создаем объект
        GameObject newObj = Instantiate(randomPrefab, spawnPosition, Quaternion.Euler(90f, 0f, 0f));
        newObj.transform.localScale = Vector3.one * randomSize;
        spawnedObjects.Add(newObj);

        // Добавляем компонент для обработки столкновений
        if (!newObj.GetComponent<CollectableObject>())
        {
            var collectable = newObj.AddComponent<CollectableObject>();
            collectable.Init(this, player);
        }
    }

    public void RemoveObject(GameObject obj)
    {
        if (spawnedObjects.Contains(obj))
        {
            spawnedObjects.Remove(obj);
            Destroy(obj);
            
            // Спавним новый объект после подбора с учетом нового размера игрока
            SpawnObject(true);
        }
    }
}