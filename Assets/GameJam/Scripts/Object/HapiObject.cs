using UnityEngine;

public class HapiObject : OrgansObject
{
    private void OnValidate()
    {
        SetMiniBossType(MiniBossType.Hapi);
    }
}
