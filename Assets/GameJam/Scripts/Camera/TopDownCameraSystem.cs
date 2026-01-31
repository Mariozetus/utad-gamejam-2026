using UnityEngine;

public class TopDownCameraSystem : MonoBehaviour
{
    [Header("TARGET SETTINGS")]
    [SerializeField] private Transform player_target;

    [Header("OFFSET SETTINGS")]
    [SerializeField] private Vector3 offSet = new Vector3(0f, 20f, -20f);

    [Header("ANGLES SETTINGS")]
    [SerializeField] private float angleX = 50f;
    [SerializeField] private float angleY = 50f;

    [Header("ORGANS")]
    [SerializeField] private OrgansManager organsManager; 

    [Header("Smoothing")]
    [SerializeField] private bool smoothFollow = true;
    [SerializeField] private float followLerp = 12f;

    private Vector3 _baseOffset;
    private bool _triedFindPlayer;

    private void Awake()
    {
        _baseOffset = offSet;
    }

    private void Start()
    {
        TryBindPlayerOnce();
        ApplyRotation();
    }

    private void LateUpdate()
    {
        if (player_target == null)
        {
            TryBindPlayerOnce();
            if (player_target == null) return;
        }

        ApplyRotation();
        FollowPlayer();
    }

    private void TryBindPlayerOnce()
    {
        if (_triedFindPlayer) return;
        _triedFindPlayer = true;

        if (player_target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player_target = player.transform;
            }
        }

        if (player_target != null && organsManager == null)
        {
            organsManager = player_target.GetComponent<OrgansManager>();
        }
    }

    private void ApplyRotation()
    {
        transform.rotation = Quaternion.Euler(angleX, angleY, 0f);
    }

    private void FollowPlayer()
    {
        float zoomMult = 1f;
        if (organsManager != null)
            zoomMult = Mathf.Clamp(organsManager.CameraZoomMultiplier, 0.5f, 3f);

        Vector3 offsetNow = _baseOffset * zoomMult;

        Vector3 desiredPos = player_target.position + offsetNow + player_target.forward;

        if (smoothFollow)
        {
            float t = 1f - Mathf.Exp(-followLerp * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, desiredPos, t);
        }
        else
        {
            transform.position = desiredPos;
        }
    }

    public void SetTarget(Transform target)
    {
        player_target = target;
        organsManager = target != null ? target.GetComponent<OrgansManager>() : null;
        _triedFindPlayer = true;
    }
}
