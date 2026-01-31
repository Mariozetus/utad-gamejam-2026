using UnityEngine;

[DisallowMultipleComponent]
public class InteractableTriggerForwarder : MonoBehaviour
{
    [SerializeField] private ObjectBase owner;

    public void SetOwner(ObjectBase o) => owner = o;

    private void Awake()
    {
        if (owner == null)
            owner = GetComponentInParent<ObjectBase>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (owner == null) return;
        owner.NotifyTriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (owner == null) return;
        owner.NotifyTriggerExit(other);
    }
}
