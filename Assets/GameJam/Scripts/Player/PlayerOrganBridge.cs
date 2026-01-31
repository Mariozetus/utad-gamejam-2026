using UnityEngine;

public class PlayerOrganBridge : MonoBehaviour
{
    [SerializeField] private OrgansManager organs;
    [SerializeField] private CombatStats stats;
    [SerializeField] private Health playerHealth;
    [SerializeField] private TimeSlowController timeSlow;

    private const string SRC_STOMACH_SPEED = "Organs:StomachSpeed";
    private const string SRC_KIDNEYS_SPEED = "Organs:KidneysSpeed";
    private const string SRC_KIDNEYS_ATTACK = "Organs:KidneysAttack";

    private void Awake()
    {
        if (organs == null) organs = GetComponent<OrgansManager>();
        if (stats == null) stats = GetComponent<CombatStats>();
        if (playerHealth == null) playerHealth = GetComponent<Health>();
        if (timeSlow == null) timeSlow = GetComponent<TimeSlowController>();
    }

    private void OnEnable()
    {
        if (organs == null) return;

        organs.HealToFullRequested += OnHealToFullRequested;
        organs.HealAmountRequested += OnHealAmountRequested;
        organs.SpeedBuffRequested += OnSpeedBuffRequested;
        organs.InstakillRequested += OnInstakillRequested;
        organs.ScreenWipeCharged += OnScreenWipeCharged;
        organs.TauntRequested += OnTauntRequested;
    }

    private void OnDisable()
    {
        if (organs == null) return;

        organs.HealToFullRequested -= OnHealToFullRequested;
        organs.HealAmountRequested -= OnHealAmountRequested;
        organs.SpeedBuffRequested -= OnSpeedBuffRequested;
        organs.InstakillRequested -= OnInstakillRequested;
        organs.ScreenWipeCharged -= OnScreenWipeCharged;
        organs.TauntRequested -= OnTauntRequested;
    }

    private void OnHealToFullRequested() => playerHealth?.HealToFull();
    private void OnHealAmountRequested(float a) => playerHealth?.Heal(a);

    private void OnSpeedBuffRequested(float mult, float dur)
    {
        if (stats == null) return;
        stats.AddMoveSpeedMul(SRC_STOMACH_SPEED, mult, dur);
    }

    private void OnInstakillRequested(GameObject t)
    {
        if (t == null) return;
        var hp = t.GetComponentInParent<Health>();
        if (hp != null) hp.TakeDamage(gameObject, 999999f);
    }

    private void OnScreenWipeCharged()
    {
        Debug.Log("Screen wipe charged");
    }

    private void OnTauntRequested(GameObject t, float d)
    {
        Debug.Log($"Taunt {t.name} {d}s");
    }

    private void Update()
    {
        if (organs == null) return;
        
        if (organs.HasSlowTimeAbility && Input.GetKeyDown(KeyCode.T))
        {
            if (timeSlow != null) timeSlow.TriggerSlowTime(organs.SlowTimeDuration);
        }

        // ✅ Kidneys test trigger (TODO: replace with Input System)
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (stats == null) return;

            if (organs.TryActivateKidneys(out float dur, out float speedMult, out float attackMult, out float healToPercent))
            {
                if (speedMult > 1f) stats.AddMoveSpeedMul(SRC_KIDNEYS_SPEED, speedMult, dur);
                if (attackMult > 1f) stats.AddAttackMul(SRC_KIDNEYS_ATTACK, attackMult, dur);
            }
        }

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
