using UnityEngine;


public class ProximityPrompt : MonoBehaviour
{
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private bool faceCamera = true;

    private Transform _target;
    private Camera _cam;

    public void Bind(Transform target, Camera cam = null)
    {
        _target = target;
        _cam = cam != null ? cam : Camera.main;
    }

    public void Show(Transform target, Camera cam = null)
    {
        Bind(target, cam);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        _target = null;
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        transform.position = _target.position + worldOffset;

        if (faceCamera && _cam != null)
        {
            Vector3 dir = transform.position - _cam.transform.position;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
