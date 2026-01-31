using System;
using UnityEngine;

public class OrgansObject : ObjectBase
{
    [SerializeField] private OrgansType organsType;
    [SerializeField] private MiniBossType miniBossType;
    [SerializeField] private bool destroyOnPickup = true;

    public OrgansType OrgansType => organsType;
    public MiniBossType MiniBossType => miniBossType;

    public static event Action<OrgansObject, GameObject> PickedUp;

    protected override void OnInteract(GameObject interactor)
    {
        PickedUp?.Invoke(this, interactor);
        if (destroyOnPickup)
            Destroy(gameObject);
    }

    protected void SetMiniBossType(MiniBossType type)
    {
        miniBossType = type;
    }
}
