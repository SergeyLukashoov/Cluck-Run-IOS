using System;
using UnityEngine;
using DG.Tweening;

public class CollectableObject : MonoBehaviour
{
    private ObjectSpawner spawner;
    private Transform player;
    private bool isCollected = false;

    private void Start()
    {
        spawner = GameObject.FindObjectOfType<ObjectSpawner>();
    }

    public void Init(ObjectSpawner spawnerRef, Transform playerRef)
    {
        spawner = spawnerRef;
        player = playerRef;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<ContinuousRotation>(out ContinuousRotation player))
        {
            if (isCollected) return;
            
                // Если объект больше игрока, его нельзя поглотить
                float playerSize = player.transform.localScale.x;
                float objectSize = transform.localScale.x;
                if (objectSize > playerSize * 2.2f)
                {
                    return;
                }

                isCollected = true;
            
                // Анимация уменьшения и уничтожения
                transform.DOScale(Vector3.zero, 0.5f)
                    .SetEase(Ease.InBack);
    
                transform.DOMove(player.transform.position, 0.5f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        player.GetComponentInParent<PlayerController>().AddPointsFromCollectedObject(objectSize);
                        player.GetComponentInParent<PlayerController>().GrowPlayer(0.1f);
                        spawner.RemoveObject(gameObject);
                    });
        }
    }
}