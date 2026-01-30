using System;
using UnityEngine;


public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private PlayerInputActions _inputActions;
    public Action PausePressed, UnpausePressed;

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
        EnablePlayerInputs();
    }

    private void OnDisable()
    {
        if(_inputActions == null)
            return;

        DisableAllInputs();
        _inputActions.Player.Pause.performed -= OnPausePerformed;
        _inputActions.UI.Unpause.performed -= OnUnpausePerformed;
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
}
