using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private bool _isInteractPressed;
    private bool _isHeld = false;
    private PlayerInputActions _inputActions;
    public Action PausePressed, UnpausePressed, InteractPressed, InteractCanceled, HitPerformed, HitCanceled;
    public Action QPressed, EPressed, RPressed;

    public Action UiEscPressed;
    public Action UiEnterPressed;
    public Action UiSpacePressed;
    public Action UiTabPressed;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            this.gameObject.transform.parent = null;
            DontDestroyOnLoad(gameObject);
            _inputActions = new PlayerInputActions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if(_inputActions == null)
            return;
        _inputActions.Player.Q.performed += OnQPerformed;
        _inputActions.Player.E.performed += OnEPerformed;
        _inputActions.Player.R.performed += OnRPerformed;
        _inputActions.Player.Pause.performed += OnPausePerformed;
        _inputActions.UI.Unpause.performed += OnUnpausePerformed;
        _inputActions.UI.Enter.performed += OnUnpausePerformed;
        _inputActions.UI.Esc.performed += OnUnpausePerformed;
        _inputActions.UI.Space.performed += OnUnpausePerformed;
        _inputActions.UI.Tab.performed += OnUnpausePerformed;
        _inputActions.Player.Interact.started += OnInteractStarted;
        _inputActions.Player.Interact.canceled += OnInteractCanceled;
        _inputActions.Player.Hit.started += OnHitStarted;
        _inputActions.Player.Hit.canceled += OnHitCanceled;
        _inputActions.Player.Hit.performed += OnHitPerformed;

        EnablePlayerInputs();
    }

    private void OnDisable()
    {
        if(_inputActions == null)
            return;

        DisableAllInputs();
        _inputActions.Player.Q.performed -= OnQPerformed;
        _inputActions.Player.E.performed -= OnEPerformed;
        _inputActions.Player.R.performed -= OnRPerformed;
        _inputActions.Player.Pause.performed -= OnPausePerformed;
        _inputActions.UI.Unpause.performed -= OnUnpausePerformed;
        _inputActions.Player.Interact.started -= OnInteractStarted;
        _inputActions.Player.Interact.canceled -= OnInteractCanceled;
        _inputActions.UI.Esc.performed -= OnUiEsc;
        _inputActions.UI.Enter.performed -= OnUiEnter;
        _inputActions.UI.Space.performed -= OnUiSpace;
        _inputActions.UI.Tab.performed -= OnUiTab;
        _inputActions.Player.Hit.started -= OnHitStarted;
        _inputActions.Player.Hit.canceled -= OnHitCanceled;
        _inputActions.Player.Hit.performed -= OnHitPerformed;

    }

    private void OnUnpausePerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Logger.Log("Unpausing game", LogType.System);
        EnablePlayerInputs();
        UnpausePressed?.Invoke();
    }

    private void OnPausePerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Logger.Log("Pausing game", LogType.System);
        EnableUIInputs();
        PausePressed?.Invoke();
    }

    public void EnablePlayerInputs()
    {
        DisableAllInputs();
        _inputActions.Player.Enable();
    }

    public void EnableUIInputs()
    {
        DisableAllInputs();
        _inputActions.UI.Enable();
    }

    public void DisableAllInputs()
    {
        _inputActions.Disable();
    }

    public Vector2 GetMovementInput()
    {
        return _inputActions.Player.Move.ReadValue<Vector2>();
    }
    private void OnInteractStarted(InputAction.CallbackContext ctx)
    {
        _isInteractPressed = true;
        InteractPressed?.Invoke();
    }

    private void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        _isInteractPressed = false;
        InteractCanceled?.Invoke();
    }
    
    private void OnQPerformed(InputAction.CallbackContext ctx) => QPressed?.Invoke();
    private void OnEPerformed(InputAction.CallbackContext ctx) => EPressed?.Invoke();
    private void OnRPerformed(InputAction.CallbackContext ctx) => RPressed?.Invoke();
    
    private void OnUiEsc(InputAction.CallbackContext ctx) => UiEscPressed?.Invoke();
    private void OnUiEnter(InputAction.CallbackContext ctx) => UiEnterPressed?.Invoke();
    
    private void OnUiSpace(InputAction.CallbackContext ctx) => UiSpacePressed?.Invoke();
    private void OnUiTab(InputAction.CallbackContext ctx) => UiTabPressed?.Invoke();
    
    public void EnablePlayerInputActions()
    {
        if (_inputActions == null)
            return;

        DisableAllInputActions();
        _inputActions.Player.Enable();
    }

    public void EnableUiInputActions()
    {
        if (_inputActions == null)
            return;

        DisableAllInputActions();
        _inputActions.UI.Enable();
    }
    
    private void DisableAllInputActions()
    {
        if (_inputActions == null)
            return;

        _inputActions.Player.Disable();
        _inputActions.UI.Disable();
    }

    private bool _isAttacking = false;
    private void OnHitStarted(InputAction.CallbackContext ctx)
    {
        if (!_isAttacking)
        {
            _isAttacking = true;
            if (PlayerController.Instance != null){
                PlayerController.Instance.Attack();
            }
        }
    }
    private void OnHitPerformed(InputAction.CallbackContext context)
    {
        Logger.Log("Hit performed", LogType.System);
        _isHeld = true;
        HitPerformed?.Invoke();
    }

    public void OnHitCanceled(InputAction.CallbackContext ctx)
    {
        _isAttacking = false;
        if(!_isHeld){
            HitCanceled?.Invoke();
            _isHeld = false;
        }
        Logger.Log("canceled", LogType.System);
    }
}
