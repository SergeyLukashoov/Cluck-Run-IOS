using System;
using DG.Tweening;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("References")]
    [SerializeField] private FloatingJoystick _joystick;

    [SerializeField] private GameObject playerImage;

    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;
    
    [SerializeField] private EndlessBackground endlessBackground;
    [SerializeField] private ObjectSpawner spawner;
    [SerializeField] private UIManager uiManager;

    public int collectedObjects = 0;
    public int collectedObjectsForAchiev = 0;
    public int growUpTimes = 0;

    private void Awake()
    {
        _rigidbody = GetComponentInChildren<Rigidbody>();
        Time.timeScale = 1f;
    }

    private void Start()
    {
        collectedObjectsForAchiev = PlayerPrefs.GetInt("CollectedEggs", 0);
        growUpTimes = PlayerPrefs.GetInt("GrowUp200Times", 0);
    }

    private void Update()
    {
        if (_joystick.isActiveAndEnabled)
        {
            UpdateMovementDirection();
        }
        else
        {
            _moveDirection = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        MoveCharacter();
    }

    private void UpdateMovementDirection()
    {
        _moveDirection = new Vector3(_joystick.Horizontal, 0f, _joystick.Vertical);
    }

    private void MoveCharacter()
    {
        if (_moveDirection.magnitude > 0.1f)
        {
            Vector3 movement = _moveDirection.normalized * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(_rigidbody.position + movement);
        }
    }
    
    public void GrowPlayer(float amount)
    {
        if (collectedObjects == 5)
        {
            Camera.main.DOOrthoSize(10, 1f).SetRelative();
            endlessBackground.renderDistance += 3;
            spawner.spawnRadius += 50;
            if (!uiManager._popUpWindow.activeSelf)
            {
                uiManager.OpenPopUpWindow();
            }
            collectedObjects = 0;
        }
        
        playerImage.transform.localScale += Vector3.one * amount;
        growUpTimes++;
        PlayerPrefs.SetInt("GrowUp200Times", growUpTimes);
        collectedObjects++;
        collectedObjectsForAchiev++;
        PlayerPrefs.SetInt("CollectedEggs", collectedObjectsForAchiev);
        GetComponentInChildren<ContinuousRotation>().OnAbsorbed();
    }

    public void AddPointsFromCollectedObject(float size)
    {
        uiManager.AddPoints(size);
    }
}