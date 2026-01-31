using System;
using System.Collections.Generic;
using UnityEngine;


public class OrgansManager : MonoBehaviour
{
    [Header("Tags")]
    [SerializeField] private string minionTag = "Enemy";
    [SerializeField] private string miniBossTag = "MiniBoss";
    [SerializeField] private string bossTag = "Boss";

    [Header("Horus Settings")]
    [SerializeField] private float horusEyesCameraZoom = 1.5f;
    [SerializeField, Range(0f, 1f)] private float horusBrainDodgeChance = 0.25f;
    [SerializeField] private int horusStomachKillThreshold = 25;
    [SerializeField] private float horusStomachSpeedBuff = 1.2f;
    [SerializeField] private float horusStomachSpeedBuffMax = 2f;
    [SerializeField] private float horusKidneysDuration = 2f;
    [SerializeField] private float horusKidneysSpeedMultiplier = 2f;
    [SerializeField] private int horusLungsExtraDash = 1;

    [Header("Khnum Settings")]
    [SerializeField] private float khnumEyesCameraZoom = 0.85f;
    [SerializeField, Range(0f, 1f)] private float khnumEyesDoubleDamageChance = 0.5f;
    [SerializeField, Range(0f, 1f)] private float khnumBrainMinionInstakillChance = 0.25f;
    [SerializeField, Range(0f, 1f)] private float khnumBrainMiniBossInstakillChance = 0.001f;
    [SerializeField, Range(0f, 1f)] private float khnumStomachHealPercent = 0.25f;
    [SerializeField] private float khnumKidneysDuration = 2f;
    [SerializeField] private float khnumKidneysAttackMultiplier = 2f;

    [Header("Hapi Settings")]
    [SerializeField] private float hapiEyesSlowTimeDuration = 3f;
    [SerializeField, Range(0f, 1f)] private float hapiBrainTauntChance = 0.2f;
    [SerializeField] private float hapiBrainTauntDuration = 2f;
    [SerializeField] private float hapiBrainTauntCooldown = 10f;
    [SerializeField] private int hapiStomachKillThreshold = 50;
    [SerializeField] private float hapiKidneysDuration = 2f;
    [SerializeField, Range(0f, 1f)] private float hapiKidneysHealToPercent = 0.75f;

    public event Action ModifiersChanged;

    // Health system listens:
    public event Action<float> HealAmountRequested;          
    public event Action HealToFullRequested;                 
    public event Action<float, float> SpeedBuffRequested;   
    public event Action<float, float> AttackBuffRequested;   

    public event Action<GameObject> InstakillRequested;      
    public event Action<GameObject, float> TauntRequested;   

    // UI system listens:
    public event Action ScreenWipeReady;                     // Hapi stomach

    private readonly Dictionary<OrgansType, MiniBossType> _activeOrgans = new();

    // Stomach
    private int _stomachKillCount;
    private float _currentSpeedMultiplier = 1f;
    private bool _screenWipeReady;

    // Hapi brain cooldown
    private float _lastTauntTimeUnscaled = -999f;

    public float CameraZoomMultiplier { get; private set; } = 1f;
    public bool VignetteEnabled { get; private set; }


    public float DodgeChance { get; private set; }            // Horus brain
    public float DoubleDamageChance { get; private set; }     // Khnum eyes


    public int ExtraDashCount { get; private set; }           // Horus lungs
    public bool HasBullChargeDash { get; private set; }       // Khnum lungs
    public bool HasWindWave { get; private set; }             // Hapi lungs
    public bool HasSlowTimeAbility { get; private set; }      // Hapi eyes
    public float SlowTimeDuration => hapiEyesSlowTimeDuration;

    private void OnEnable()
    {
        OrgansObject.PickedUp += OnOrganPickedUp;

        PlayerEvents.EnemyKilled += OnEnemyKilled;
        PlayerEvents.DamageDealt += OnDamageDealt;

        PlayerEvents.OutgoingDamageModify += OnOutgoingDamageModify;
        PlayerEvents.IncomingDamageModify += OnIncomingDamageModify;
    }

    private void OnDisable()
    {
        OrgansObject.PickedUp -= OnOrganPickedUp;

        PlayerEvents.EnemyKilled -= OnEnemyKilled;
        PlayerEvents.DamageDealt -= OnDamageDealt;

        PlayerEvents.OutgoingDamageModify -= OnOutgoingDamageModify;
        PlayerEvents.IncomingDamageModify -= OnIncomingDamageModify;
    }

    private void OnOrganPickedUp(OrgansObject organ, GameObject interactor)
    {
        if (organ == null) return;
        EquipOrgan(organ.OrgansType, organ.MiniBossType);
    }

    public void EquipOrgan(OrgansType organType, MiniBossType bossType)
    {
        _activeOrgans[organType] = bossType;

        if (organType == OrgansType.Stomach)
        {
            _stomachKillCount = 0;
            _screenWipeReady = false;
            _currentSpeedMultiplier = 1f; 
        }

        RebuildModifiers();
    }


    public void ClearOrgan(OrgansType organType)
    {
        if (_activeOrgans.Remove(organType))
        {
            if (organType == OrgansType.Stomach)
            {
                _stomachKillCount = 0;
                _screenWipeReady = false;
                _currentSpeedMultiplier = 1f;
            }

            RebuildModifiers();
        }
    }

    private void RebuildModifiers()
    {
        CameraZoomMultiplier = 1f;
        VignetteEnabled = false;

        DodgeChance = 0f;
        DoubleDamageChance = 0f;

        ExtraDashCount = 0;
        HasBullChargeDash = false;
        HasWindWave = false;
        HasSlowTimeAbility = false;

        // Eyes
        if (_activeOrgans.TryGetValue(OrgansType.Eyes, out var eyesBoss))
        {
            switch (eyesBoss)
            {
                case MiniBossType.Horus:
                    CameraZoomMultiplier = horusEyesCameraZoom;
                    break;

                case MiniBossType.Khnum:
                    CameraZoomMultiplier = khnumEyesCameraZoom;
                    VignetteEnabled = true;
                    DoubleDamageChance = khnumEyesDoubleDamageChance;
                    break;

                case MiniBossType.Hapi:
                    HasSlowTimeAbility = true;
                    break;
            }
        }

        // Brain
        if (_activeOrgans.TryGetValue(OrgansType.Brain, out var brainBoss))
        {
            if (brainBoss == MiniBossType.Horus)
                DodgeChance = horusBrainDodgeChance;
        }

        // Lungs
        if (_activeOrgans.TryGetValue(OrgansType.Lungs, out var lungsBoss))
        {
            switch (lungsBoss)
            {
                case MiniBossType.Horus:
                    ExtraDashCount = horusLungsExtraDash;
                    break;

                case MiniBossType.Khnum:
                    HasBullChargeDash = true;
                    break;

                case MiniBossType.Hapi:
                    HasWindWave = true;
                    break;
            }
        }

        ModifiersChanged?.Invoke();
    }

    private void OnEnemyKilled(GameObject enemy)
    {
        if (!_activeOrgans.TryGetValue(OrgansType.Stomach, out var stomachBoss))
            return;

        if (stomachBoss == MiniBossType.Horus)
        {
            _stomachKillCount++;
            if (_stomachKillCount >= horusStomachKillThreshold)
            {
                _stomachKillCount = 0;

                HealToFullRequested?.Invoke();

                _currentSpeedMultiplier = Mathf.Min(_currentSpeedMultiplier * horusStomachSpeedBuff, horusStomachSpeedBuffMax);
                SpeedBuffRequested?.Invoke(_currentSpeedMultiplier, 0f);
            }
        }
        else if (stomachBoss == MiniBossType.Hapi)
        {
            _stomachKillCount++;
            if (_stomachKillCount >= hapiStomachKillThreshold)
            {
                _stomachKillCount = 0;
                _screenWipeReady = true;
                ScreenWipeReady?.Invoke();
            }
        }
    }

    private void OnDamageDealt(GameObject target, float damage)
    {
        // Khnum stomach
        if (_activeOrgans.TryGetValue(OrgansType.Stomach, out var stomachBoss) && stomachBoss == MiniBossType.Khnum)
        {
            float heal = Mathf.Max(0f, damage) * khnumStomachHealPercent;
            if (heal > 0f)
                HealAmountRequested?.Invoke(heal);
        }
    }


    private void OnOutgoingDamageModify(GameObject target, ref float damage)
    {
        if (damage <= 0f) return;

        // Khnum eyes
        if (_activeOrgans.TryGetValue(OrgansType.Eyes, out var eyesBoss) &&
            eyesBoss == MiniBossType.Khnum &&
            DoubleDamageChance > 0f &&
            UnityEngine.Random.value <= DoubleDamageChance)
        {
            damage *= 2f;
        }

        // Khnum brain
        if (_activeOrgans.TryGetValue(OrgansType.Brain, out var brainBoss) &&
            brainBoss == MiniBossType.Khnum &&
            target != null)
        {
            if (target.CompareTag(minionTag))
            {
                if (UnityEngine.Random.value <= khnumBrainMinionInstakillChance)
                {
                    InstakillRequested?.Invoke(target);
                    damage = float.MaxValue;
                }
            }
            else if (target.CompareTag(miniBossTag))
            {
                if (UnityEngine.Random.value <= khnumBrainMiniBossInstakillChance)
                {
                    InstakillRequested?.Invoke(target);
                    damage = float.MaxValue;
                }
            }
        }

        // Hapi brain
        if (_activeOrgans.TryGetValue(OrgansType.Brain, out var brainBoss2) &&
            brainBoss2 == MiniBossType.Hapi &&
            target != null)
        {
            float now = Time.unscaledTime;
            if (now >= _lastTauntTimeUnscaled + hapiBrainTauntCooldown &&
                UnityEngine.Random.value <= hapiBrainTauntChance)
            {
                _lastTauntTimeUnscaled = now;
                TauntRequested?.Invoke(target, hapiBrainTauntDuration);
            }
        }
    }

    private void OnIncomingDamageModify(GameObject source, ref float damage)
    {
        if (damage <= 0f) return;

        // Horus brain
        if (_activeOrgans.TryGetValue(OrgansType.Brain, out var brainBoss) &&
            brainBoss == MiniBossType.Horus &&
            DodgeChance > 0f &&
            UnityEngine.Random.value <= DodgeChance)
        {
            damage = 0f;
        }

    }


    public bool IsScreenWipeReady() => _screenWipeReady;

    public void ConsumeScreenWipe()
    {
        _screenWipeReady = false;
    }

 
    public bool TryActivateKidneys(
        out float duration,
        out float speedMultiplier,
        out float attackMultiplier,
        out float healToPercent)
    {
        duration = 0f;
        speedMultiplier = 1f;
        attackMultiplier = 1f;
        healToPercent = 0f;

        if (!_activeOrgans.TryGetValue(OrgansType.Kidneys, out var kidneysBoss))
            return false;

        if (kidneysBoss == MiniBossType.Horus)
        {
            duration = horusKidneysDuration;
            speedMultiplier = horusKidneysSpeedMultiplier;
            return true;
        }

        if (kidneysBoss == MiniBossType.Khnum)
        {
            duration = khnumKidneysDuration;
            attackMultiplier = khnumKidneysAttackMultiplier;
            return true;
        }

        if (kidneysBoss == MiniBossType.Hapi)
        {
            duration = hapiKidneysDuration;
            healToPercent = hapiKidneysHealToPercent;
            return true;
        }

        return false;
    }
}
