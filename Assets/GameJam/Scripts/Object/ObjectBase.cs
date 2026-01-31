using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    [Header("Proximity Prompt")]
    [SerializeField] private Transform promptAnchor;
    
    [Header("Auto Trigger")]
    [SerializeField] private bool autoEnsureTrigger = true;
    [SerializeField] private float autoTriggerRadius = 1.5f;
    [SerializeField] private string autoTriggerChildName = "_InteractTrigger";

    [Header("SFX")]
    [SerializeField] private bool playSfx = false;
    [SerializeField] private string sfxOpen = "Item_Pickup";

    private Transform _autoTriggerTf;
    private SphereCollider _autoTriggerCol;
    private InteractableTriggerForwarder _autoForwarder;

    public void Interact(GameObject interactor)
    {
        OnInteract(interactor);
    }
    
    
    private void Awake()
    {
        if (autoEnsureTrigger)
            EnsureAutoTriggerChild();
    }

    
    private void EnsureAutoTriggerChild()
    {
        _autoTriggerTf = transform.Find(autoTriggerChildName);
        if (_autoTriggerTf == null)
        {
            var child = new GameObject(autoTriggerChildName);
            child.transform.SetParent(transform, false);
            _autoTriggerTf = child.transform;
        }

        _autoTriggerCol = _autoTriggerTf.GetComponent<SphereCollider>();
        if (_autoTriggerCol == null)
            _autoTriggerCol = _autoTriggerTf.gameObject.AddComponent<SphereCollider>();

        _autoTriggerCol.isTrigger = true;
        _autoTriggerCol.radius = Mathf.Max(0.01f, autoTriggerRadius);

        _autoForwarder = _autoTriggerTf.GetComponent<InteractableTriggerForwarder>();
        if (_autoForwarder == null)
            _autoForwarder = _autoTriggerTf.gameObject.AddComponent<InteractableTriggerForwarder>();

        _autoForwarder.SetOwner(this);
    }
    
    public void NotifyTriggerEnter(Collider other)
    {
        HandleTriggerEnter(other);
    }

    public void NotifyTriggerExit(Collider other)
    {
        HandleTriggerExit(other);
    }

    public Transform GetPromptAnchor()
    {
        return promptAnchor != null ? promptAnchor : transform;
    }
    
    private void HandleTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (!other.CompareTag("Player")) return;
        ProximityDetector detector = other.GetComponentInChildren<ProximityDetector>();
        if (detector != null)
            detector.Register(this);
    }

    private void HandleTriggerExit(Collider other)
    {
        if (other == null) return;
        if (!other.CompareTag("Player")) return;

        ProximityDetector detector = other.GetComponentInChildren<ProximityDetector>();
        if (detector != null)
            detector.Unregister(this);
    }

    protected virtual void OnInteract(GameObject interactor)
    {
    }
    
}
