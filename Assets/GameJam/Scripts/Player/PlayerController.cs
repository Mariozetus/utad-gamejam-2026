using UnityEngine;
using System.Collections;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("SPEED SETTINGS")]
    [Range(0f, 20f)] [SerializeField] private float normalMovementSpeed = 5f;

    [Header("ROTATION SETTINGS")]
    [Range(0f, 20f)] [SerializeField] private float rotationSpeed = 5f;
    //[SerializeField] Animator fadeAnimation;
    private Vector2 _currentInput;
    private Vector3 _velocity;
    private CharacterController _characterController;
    private Camera _camera;
    private Animator _animator;
    private float _currentSpeed;
    private bool _isRunning = false;
    private bool _wasWalking = false;
    private static readonly int Speed = Animator.StringToHash("Speed");

    // Pause screen.
    public bool screenPaused;

    [Header("ORGANS (Runtime Multipliers)")]
    [SerializeField] private bool useUnscaledRotationLerp = false; 
    private float _movementSpeedMultiplier = 1f;  
    private float _movementSpeedMultiplierEndUnscaled = -1f; 

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _camera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

        _currentInput = InputManager.Instance.GetMovementInput();
        _currentSpeed = normalMovementSpeed;
    }

    private void Update()
    {
        TickOrgansSpeedBuff();
        UpdateMovement();
        ApplyTotalVelocity();
    }

    private void UpdateMovement(){
    
    // PREFABS.
    //bool isMoving = _currentInput.magnitude > 0.1f;
    //bool playerIsWalking = !_isRunning && isMoving;
    //bool playerIsRunning = _isRunning && isMoving;

    /////// SONIDOS -> MARIO ////////////
    /*
    // Walk and Run sounds.
    if (SoundManager.Instance != null && SoundManager.Instance.playerSounds != null)
    {
        if (playerIsRunning && (!_wasRunning || !SoundManager.Instance.playerSounds.isPlaying))
        {
            SoundManager.Instance.playerSounds.Stop();
            SoundManager.Instance.PlayFx(AudioFX.RunSound, SoundManager.Instance.playerSounds);
        }
        else if (playerIsWalking && (!_wasWalking || !SoundManager.Instance.playerSounds.isPlaying))
        {
            SoundManager.Instance.playerSounds.Stop();
            SoundManager.Instance.PlayFx(AudioFX.WalkSound, SoundManager.Instance.playerSounds);
        }
        else if (!playerIsRunning && !playerIsWalking && SoundManager.Instance.playerSounds.isPlaying)
        {
            SoundManager.Instance.playerSounds.Stop();
        }
        _wasRunning = playerIsRunning;
        _wasWalking = playerIsWalking;
    }
    */

        if (screenPaused){
            return;
        }
        _currentInput = InputManager.Instance.GetMovementInput().normalized;

        // Direcction.
        Vector3 forward = new Vector3(1, 0, 1).normalized;
        Vector3 right = new Vector3(1, 0, -1).normalized;
        Vector3 desiredMove = right * _currentInput.x + forward * _currentInput.y;
        desiredMove.y = 0f;

        // Rotation.
        if (desiredMove.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredMove);

            float dt = useUnscaledRotationLerp ? Time.unscaledDeltaTime : Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * dt);
            //transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        
        float finalSpeed = _currentSpeed * _movementSpeedMultiplier;
        _velocity = desiredMove * finalSpeed;
        //_velocity = desiredMove * _currentSpeed;

        if (_animator != null)
        {
            _animator.SetFloat(Speed, _currentInput.magnitude);
        }
    }
    
    public void setPause(bool p)
    {
        screenPaused = p;
    }

    private void ApplyTotalVelocity()
    {
        _characterController.SimpleMove(_velocity);
    }

    public Transform GetTransform(out bool playerOnSight)
    {
        playerOnSight = true;
        return transform;
    }


    public void SetMovementSpeedMultiplier(float multiplier)
     {
         _movementSpeedMultiplier = Mathf.Max(0f, multiplier);
         _movementSpeedMultiplierEndUnscaled = -1f;
     }
    
     public void SetMovementSpeedMultiplierTimed(float multiplier, float durationSeconds)
     {
         _movementSpeedMultiplier = Mathf.Max(0f, multiplier);
         _movementSpeedMultiplierEndUnscaled = Time.unscaledTime + Mathf.Max(0f, durationSeconds);
     }

     public void ResetMovementSpeedMultiplier()
   {
         _movementSpeedMultiplier = 1f;
        _movementSpeedMultiplierEndUnscaled = -1f;
   }

     private void TickOrgansSpeedBuff()
    {
         if (_movementSpeedMultiplierEndUnscaled < 0f) return;
         if (Time.unscaledTime >= _movementSpeedMultiplierEndUnscaled)
        {
            _movementSpeedMultiplier = 1f;
             _movementSpeedMultiplierEndUnscaled = -1f;
         }
    }
}
