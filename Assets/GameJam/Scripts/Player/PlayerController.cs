using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("SPEED SETTINGS")]
    [Range(0f, 20f)] [SerializeField] private float normalMovementSpeed = 5f;

    [Header("ROTATION SETTINGS")]
    [Range(0f, 20f)] [SerializeField] private float rotationSpeed = 5f;

    private Vector2 _currentInput;
    private Vector3 _velocity;
    private CharacterController _characterController;
    private Camera _camera;
    private Animator _animator;
    private float _currentSpeed;
    private static readonly int Speed = Animator.StringToHash("Speed");

    public bool screenPaused;

    [Header("Rotation Lerp")]
    [SerializeField] private bool useUnscaledRotationLerp = false;

    [Header("STATS")]
    [SerializeField] private CombatStats stats;

    private void Awake()
    {
        Instance = this;
        if (stats == null) stats = GetComponent<CombatStats>();

        GameManager.Instance.RegisterPlayer(this);
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _camera = Camera.main;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

        _currentInput = InputManager.Instance.GetMovementInput();
        _currentSpeed = normalMovementSpeed;

        // Feed base move speed into stats
        if (stats != null) stats.SetBaseMoveSpeed(_currentSpeed);

        InputManager.Instance.HitCanceled += Attack;
        InputManager.Instance.HitPerformed += RangedAttack;
    }

    private void Update()
    {
        UpdateMovement();
        ApplyTotalVelocity();
    }
    private void UpdateMovement()
    {
        if (screenPaused) return;

        _currentInput = InputManager.Instance.GetMovementInput().normalized;

        Vector3 forward = new Vector3(1, 0, 1).normalized;
        Vector3 right = new Vector3(1, 0, -1).normalized;
        Vector3 desiredMove = right * _currentInput.x + forward * _currentInput.y;
        desiredMove.y = 0f;

        if (desiredMove.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(desiredMove);
            float dt = useUnscaledRotationLerp ? Time.unscaledDeltaTime : Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * dt);
        }

        float finalSpeed = (stats != null) ? stats.MoveSpeed.Value : _currentSpeed;
        _velocity = desiredMove * finalSpeed;

        if (_animator != null)
            _animator.SetFloat(Speed, _currentInput.magnitude);
    }

    public void setPause(bool p) => screenPaused = p;

    private void ApplyTotalVelocity()
    {
        _characterController.SimpleMove(_velocity);
    }

    public Transform GetTransform(out bool playerOnSight)
    {
        playerOnSight = true;
        return transform;
    }

    public void Attack()
    {

        Logger.Log("Normal Attack");
    }

    public void RangedAttack()
    {
        Logger.Log("Range");
    }
}
