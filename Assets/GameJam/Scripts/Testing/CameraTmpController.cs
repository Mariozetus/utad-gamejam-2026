using UnityEngine;

public class CameraTmpController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -10f);
    [SerializeField] private float smoothTime = 0.15f;

    private Vector3 velocity;
    private Vector3 baseOffset;

    private void Start()
    {
        baseOffset = offset;
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
        transform.LookAt(target);
    }

    public void IncreaseVisionPercent(float percent)
    {
        float multiplier = 1f + (percent / 100f);
        offset = baseOffset * multiplier;
    }
    
    public void ResetVision()
    {
        offset = baseOffset;
    }
    
    
    
}
