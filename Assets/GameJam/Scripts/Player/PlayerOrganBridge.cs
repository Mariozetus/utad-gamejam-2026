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

        if (InputManager.Instance != null)
        {
            InputManager.Instance.QPressed += OnQPressed;
            InputManager.Instance.EPressed += OnEPressed;
            InputManager.Instance.RPressed += OnRPressed;
        }
    }

    private void OnDisable()
    {
        if (organs != null)
        {
            organs.HealToFullRequested -= OnHealToFullRequested;
            organs.HealAmountRequested -= OnHealAmountRequested;
            organs.SpeedBuffRequested -= OnSpeedBuffRequested;
            organs.InstakillRequested -= OnInstakillRequested;
            organs.ScreenWipeCharged -= OnScreenWipeCharged;
            organs.TauntRequested -= OnTauntRequested;
        }

        if (InputManager.Instance != null)
        {
            InputManager.Instance.QPressed -= OnQPressed;
            InputManager.Instance.EPressed -= OnEPressed;
            InputManager.Instance.RPressed -= OnRPressed;
        }
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


    private void OnRPressed()
    {
        if (organs == null) return;

        if (organs.TryUseEyesActive(out float slowDur))
        {
            if (timeSlow != null)
                timeSlow.TriggerSlowTime(slowDur);
        }
    }

    private void OnEPressed()
    {
        if (organs == null) return;

        organs.TryUseLungsActive();
    }

    private void OnQPressed()
    {
        if (organs == null) return;

        organs.TryUseStomachActive(gameObject);
    }
}
