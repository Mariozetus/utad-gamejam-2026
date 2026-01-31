using UnityEngine;

public class HorusObject : OrgansObject
{
    private void OnValidate()
    {
        SetMiniBossType(MiniBossType.Horus);
    }
}
