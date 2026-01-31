using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class InputManager : MonoBehaviour
{
    
    
    public static InputManager Instance { get; private set; }

    private bool _isInteractPressed;
    
    private PlayerInputActions _inputActions;
    public Action PausePressed, UnpausePressed, InteractPressed, InteractCanceled;

    
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
        
        _inputActions.Player.Pause.performed += OnPausePerformed;
        _inputActions.UI.Unpause.performed += OnUnpausePerformed;
        _inputActions.UI.Enter.performed += OnUnpausePerformed;
        _inputActions.UI.Esc.performed += OnUnpausePerformed;
        _inputActions.UI.Space.performed += OnUnpausePerformed;
        _inputActions.UI.Tab.performed += OnUnpausePerformed;
        _inputActions.Player.Interact.started += OnInteractStarted;
        _inputActions.Player.Interact.canceled += OnInteractCanceled;
        EnablePlayerInputs();
    }

    private void OnDisable()
    {
        if(_inputActions == null)
            return;

        DisableAllInputs();
        _inputActions.Player.Pause.performed -= OnPausePerformed;
        _inputActions.UI.Unpause.performed -= OnUnpausePerformed;
        _inputActions.Player.Interact.started -= OnInteractStarted;
        _inputActions.Player.Interact.canceled -= OnInteractCanceled;
        _inputActions.UI.Esc.performed -= OnUiEsc;
        _inputActions.UI.Enter.performed -= OnUiEnter;
        _inputActions.UI.Space.performed -= OnUiSpace;
        _inputActions.UI.Tab.performed -= OnUiTab;
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

}