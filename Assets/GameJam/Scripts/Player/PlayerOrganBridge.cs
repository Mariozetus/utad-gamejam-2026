using UnityEngine;

public class PlayerOrganBridge : MonoBehaviour
{
    [SerializeField] private OrgansManager organs;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Health playerHealth;
    [SerializeField] private TimeSlowController timeSlow;

    private void Awake()
    {
        if (organs == null) organs = GetComponent<OrgansManager>();
        if (playerController == null) playerController = GetComponent<PlayerController>();
        if (playerHealth == null) playerHealth = GetComponent<Health>();
        if (timeSlow == null) timeSlow = GetComponent<TimeSlowController>();
    }

    private void OnEnable()
    {
        if (organs == null) return;

        organs.HealToFullRequested += () => playerHealth?.HealToFull();
        organs.HealAmountRequested += (a) => playerHealth?.Heal(a);

        organs.SpeedBuffRequested += (mult, dur) =>
        {
            if (playerController == null) return;
            if (dur > 0f) playerController.SetMovementSpeedMultiplierTimed(mult, dur);
            else playerController.SetMovementSpeedMultiplier(mult);
        };

        organs.InstakillRequested += (t) =>
        {
            if (t == null) return;
            var hp = t.GetComponentInParent<Health>();
            if (hp != null) hp.TakeDamage(gameObject, 999999f);
        };

        organs.ScreenWipeCharged += () =>
        {
            Debug.Log("y test screen wipe");
        };

        organs.TauntRequested += (t, d) =>
        {
            Debug.Log($"Taunt {t.name} {d}s");
        };
    }

    private void OnDisable()
    {
        if (organs == null) return;

        organs.HealToFullRequested -= () => playerHealth?.HealToFull();
    }

    private void Update()
    {
        if (organs == null) return;

        if (organs.HasSlowTimeAbility && Input.GetKeyDown(KeyCode.T))
        {
            if (timeSlow != null) timeSlow.TriggerSlowTime(organs.SlowTimeDuration);
        }
        //TODO : Remove test key ADD input system
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (!organs.IsScreenWipeReady()) return;
            organs.ConsumeScreenWipe();

            var minions = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var e in minions)
            {
                var hp = e.GetComponentInParent<Health>();
                if (hp != null) hp.TakeDamage(gameObject, 999999f);
            }
        }
    }
}
