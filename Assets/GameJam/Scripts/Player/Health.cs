using UnityEngine;

public class Health : MonoBehaviour
{

    public static event System.Action<Health, float> Damaged;
    public static event System.Action<Health, float> Healed;
    [Header("Liver Passive Regen")]
    [SerializeField] private float liverRegenPercentPerSecond = 0.05f;

    
    [SerializeField] private bool isPlayer = false;
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private bool Enemy = false;
    [SerializeField] private bool UseHealthBar = false;
    
    [SerializeField] private GameObject PlayerHealthBarUI;
    [SerializeField] private GameObject EnemyBossHealthBarUI;
    private GameObject _healthBar;
    
    public float Current => currentHealth;
    public float Max => maxHealth;
    private bool _undeadActive = false;
    private float _undeadEndUnscaled = -1f;

    private OrgansManager _organs;      // player ONLY
    private PlayerController _player;   // player ONLY  

    private void Awake()
    {//TODO: Fix Health Bar
        /*if (Enemy)
        {
            maxHealth = (float)gameObject.GetComponent<EnemyStats>()?.MaxHealth;
            
        }
        else
        {
            _healthBar = PlayerHealthBarUI;
            _healthBar.SetActive(true);
        }
*/
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        

        if (isPlayer)
        {
            _organs = GetComponent<OrgansManager>();
            _player = GetComponent<PlayerController>();
        }
    }

    private void Update()
    {
        if (_undeadActive && Time.unscaledTime >= _undeadEndUnscaled)
        {
            _undeadActive = false;
            _undeadEndUnscaled = -1f;
        }

        if (isPlayer)
            TickLiverRegen();
    }


    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        float before = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        float delta = currentHealth - before;
        if (delta > 0f)
            Healed?.Invoke(this, delta);
    }

    public void HealToFull()
    {
        float before = currentHealth;
        currentHealth = maxHealth;
        float delta = currentHealth - before;
        if (delta > 0f)
            Healed?.Invoke(this, delta);
    }

    public void SetHealthToPercent(float pct01)
    {
        pct01 = Mathf.Clamp01(pct01);
        float target = maxHealth * pct01;
        if (currentHealth < target)
        {
            float before = currentHealth;
            currentHealth = target;
            float delta = currentHealth - before;
            if (delta > 0f)
                Healed?.Invoke(this, delta);
        }
    }

    public void TakeDamage(GameObject source, float baseDamage)
    {
        float dmg = Mathf.Max(0f, baseDamage);

        if (isPlayer)
            dmg = PlayerEvents.ApplyIncomingDamage(source, dmg);

        if (dmg <= 0f)
        {
            if (isPlayer) PlayerEvents.RaiseDamageTaken(source, 0f);
            return;
        }

        if (_undeadActive)
        {
            currentHealth = Mathf.Max(1f, currentHealth - dmg);
            Damaged?.Invoke(this, dmg);
            if (isPlayer) PlayerEvents.RaiseDamageTaken(source, dmg);
            return;
        }

        if (isPlayer && (currentHealth - dmg) <= 0f && _organs != null)
        {
            if (_organs.TryActivateKidneys(out float dur, out float speedMult, out float attackMult, out float healToPct))
            {
                ActivateUndead(dur);

                if (healToPct > 0f) SetHealthToPercent(healToPct);
                if (_player != null && speedMult > 1f) _player.SetMovementSpeedMultiplierTimed(speedMult, dur);

                currentHealth = Mathf.Max(1f, currentHealth);
                if (dmg > 0f) Damaged?.Invoke(this, dmg);
                PlayerEvents.RaiseDamageTaken(source, 0f);
                return;
            }
        }

        currentHealth -= dmg;
        if (dmg > 0f) Damaged?.Invoke(this, dmg);

        if (isPlayer) PlayerEvents.RaiseDamageTaken(source, dmg);

        if (currentHealth <= 0f)
            Die();
    }

    private void ActivateUndead(float durationSeconds)
    {
        _undeadActive = true;
        _undeadEndUnscaled = Time.unscaledTime + Mathf.Max(0f, durationSeconds);
    }

    private void Die()
    {
        currentHealth = 0f;

        if (!isPlayer)
        {
            PlayerEvents.RaiseEnemyKilled(gameObject);
            Destroy(gameObject);
        }
        else
        {
            var pc = GetComponent<PlayerController>();
            if (pc != null) pc.enabled = false;
            Debug.Log("Player died.");
        }
    }
    private void TickLiverRegen()
    {
        if (_organs == null) return;

        if (!_organs.TryGetLiver(out var liverBoss))
            return;

        float triggerBelow = 0f;
        float regenCap = 0f;

        switch (liverBoss)
        {
            case MiniBossType.Horus:
                triggerBelow = 0.50f;
                regenCap = 0.75f;
                break;

            case MiniBossType.Khnum:
                triggerBelow = 0.25f;
                regenCap = 1.00f;
                break;

            case MiniBossType.Hapi:
                triggerBelow = 0.40f;
                regenCap = 0.90f;
                break;
        }

        float hpPct = (maxHealth <= 0f) ? 0f : (currentHealth / maxHealth);

        if (hpPct >= triggerBelow) return;

        float capHealth = maxHealth * regenCap;
        if (currentHealth >= capHealth) return;

        float amount = maxHealth * liverRegenPercentPerSecond * Time.deltaTime;
        float before = currentHealth;
        currentHealth = Mathf.Min(capHealth, currentHealth + amount);

        float delta = currentHealth - before;
        if (delta > 0f)
            Healed?.Invoke(this, delta);
    }

}
