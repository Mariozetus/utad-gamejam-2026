using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float range = 3f;
    [SerializeField] private float baseDamage = 10f;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))// left click test TODO: replace with proper attack input   
            TryHit();
    }

    private void TryHit()
    {
        if (cam == null) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            var hp = hit.collider.GetComponentInParent<Health>();
            if (hp == null) return;

            float dmg = PlayerEvents.ApplyOutgoingDamage(hp.gameObject, baseDamage);
            hp.TakeDamage(gameObject, dmg);
            PlayerEvents.RaiseDamageDealt(hp.gameObject, dmg);
        
        /*        enemy to player
                var hp = player.GetComponentInParent<Health>();
                if (hp == null) return;
                hp.TakeDamage(enemy, baseDamage);
        */
        }
    }
}