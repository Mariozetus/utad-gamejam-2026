using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private PlayerInputActions _inputActions;

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

        _inputActions.Enable();
    }

    private void OnDisable()
    {
        if(_inputActions == null)
            return;

        _inputActions.Disable();
    }
}
