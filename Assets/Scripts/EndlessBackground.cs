using UnityEngine;
using System.Collections.Generic;

public class EndlessBackground : MonoBehaviour
{
    [SerializeField] private GameObject backgroundPrefab; // Префаб вашего фона
    [SerializeField] public int renderDistance = 2; // На сколько чанков вперед/назад генерировать
    [SerializeField] private Vector2 chunkSize = new Vector2(10f, 10f); // Размер одного чанка фона

    private Transform cameraTransform;
    private Dictionary<Vector2Int, GameObject> spawnedChunks = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int lastCameraChunkPos;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraChunkPos = GetChunkPosition(cameraTransform.position);
        GenerateChunksAroundCamera();
    }

    private void Update()
    {
        Vector2Int currentChunkPos = GetChunkPosition(cameraTransform.position);
        
        if (currentChunkPos != lastCameraChunkPos)
        {
            GenerateChunksAroundCamera();
            lastCameraChunkPos = currentChunkPos;
        }
    }

    private Vector2Int GetChunkPosition(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x / chunkSize.x);
        int z = Mathf.RoundToInt(worldPosition.z / chunkSize.y);
        return new Vector2Int(x, z);
    }

    private void GenerateChunksAroundCamera()
    {
        Vector2Int centerChunk = GetChunkPosition(cameraTransform.position);
        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();

        // Генерация новых чанков вокруг камеры
        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int y = -renderDistance; y <= renderDistance; y++)
            {
                Vector2Int chunkPos = new Vector2Int(centerChunk.x + x, centerChunk.y + y);
                chunksToKeep.Add(chunkPos);

                if (!spawnedChunks.ContainsKey(chunkPos))
                {
                    Vector3 spawnPosition = new Vector3(
                        chunkPos.x * chunkSize.x,
                        0,
                        chunkPos.y * chunkSize.y
                    );

                    GameObject newChunk = Instantiate(
                        backgroundPrefab,
                        spawnPosition,
                        Quaternion.Euler(90, 0, 0), // Поворот на 90 по X
                        transform
                    );

                    spawnedChunks.Add(chunkPos, newChunk);
                }
            }
        }

        // Удаление чанков, которые слишком далеко
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in spawnedChunks)
        {
            if (!chunksToKeep.Contains(chunk.Key))
            {
                Destroy(chunk.Value);
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunkPos in chunksToRemove)
        {
            spawnedChunks.Remove(chunkPos);
        }
    }
}