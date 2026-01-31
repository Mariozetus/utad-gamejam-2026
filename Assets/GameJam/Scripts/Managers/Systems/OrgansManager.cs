    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public class OrgansManager : MonoBehaviour
    {
        [Header("Fallback Tags")]
        [SerializeField] private string minionTag = "Enemy";
        [SerializeField] private string miniBossTag = "MiniBoss";
        [SerializeField] private string bossTag = "Boss";

        [Header("Horus")]
        [SerializeField] private float horusEyesCameraZoom = 1.5f;
        [SerializeField, Range(0f, 1f)] private float horusBrainDodgeChance = 0.25f;
        [SerializeField] private int horusStomachKillThreshold = 25;
        [SerializeField] private float horusStomachSpeedBuff = 1.2f;
        [SerializeField] private float horusStomachSpeedBuffMax = 2f;
        [SerializeField] private float horusKidneysDuration = 2f;
        [SerializeField] private float horusKidneysSpeedMultiplier = 2f;
        [SerializeField] private int horusLungsExtraDash = 1;

        [Header("Khnum")]
        [SerializeField] private float khnumEyesCameraZoom = 0.85f;
        [SerializeField, Range(0f, 1f)] private float khnumEyesDoubleDamageChance = 0.5f;
        [SerializeField, Range(0f, 1f)] private float khnumBrainMinionInstakillChance = 0.25f;
        [SerializeField, Range(0f, 1f)] private float khnumBrainMiniBossInstakillChance = 0.001f;
        [SerializeField, Range(0f, 1f)] private float khnumStomachHealPercent = 0.25f;
        [SerializeField] private float khnumKidneysDuration = 2f;
        [SerializeField] private float khnumKidneysAttackMultiplier = 2f;

        [Header("Hapi")]
        [SerializeField] private float hapiEyesSlowTimeDuration = 3f;
        [SerializeField, Range(0f, 1f)] private float hapiBrainTauntChance = 0.2f;
        [SerializeField] private float hapiBrainTauntDuration = 2f;
        [SerializeField] private float hapiBrainTauntCooldown = 10f;
        [SerializeField] private int hapiStomachKillThreshold = 50;
        [SerializeField] private float hapiKidneysDuration = 2f;
        [SerializeField, Range(0f, 1f)] private float hapiKidneysHealToPercent = 0.75f;

        public event Action ModifiersChanged;
        public event Action<float> HealAmountRequested;
        public event Action HealToFullRequested;
        public event Action<float, float> SpeedBuffRequested;  
        public event Action ScreenWipeCharged;
        public event Action<GameObject> InstakillRequested;
        public event Action<GameObject, float> TauntRequested;

        private readonly Dictionary<OrgansType, MiniBossType> _active = new();

        public float CameraZoomMultiplier { get; private set; } = 1f;
        public bool VignetteEnabled { get; private set; }
        public float DodgeChance { get; private set; }          // Horus brain
        public float DoubleDamageChance { get; private set; }   // Khnum eyes
        public int ExtraDashCount { get; private set; }         // Horus lungs
        public bool HasSlowTimeAbility { get; private set; }    //· Hapi eyes
        public float SlowTimeDuration => hapiEyesSlowTimeDuration;

        private int _stomachKillCount = 0;
        private float _stackedSpeedMult = 1f;
        private bool _screenWipeReady = false;
        private float _lastTauntUnscaled = -999f;

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
            Equip(organ.OrgansType, organ.MiniBossType);
        }

        public void Equip(OrgansType slot, MiniBossType boss)
        {
            _active[slot] = boss;

            if (slot == OrgansType.Stomach)
            {
                _stomachKillCount = 0;
                _screenWipeReady = false;
                _stackedSpeedMult = 1f;
            }

            Rebuild();
        }

        private void Rebuild()
        {
            CameraZoomMultiplier = 1f;
            VignetteEnabled = false;
            DodgeChance = 0f;
            DoubleDamageChance = 0f;
            ExtraDashCount = 0;
            HasSlowTimeAbility = false;

            // Eyes
            if (_active.TryGetValue(OrgansType.Eyes, out var eyes))
            {
                if (eyes == MiniBossType.Horus) CameraZoomMultiplier = horusEyesCameraZoom;
                if (eyes == MiniBossType.Khnum)
                {
                    CameraZoomMultiplier = khnumEyesCameraZoom;
                    VignetteEnabled = true;
                    DoubleDamageChance = khnumEyesDoubleDamageChance;
                }
                if (eyes == MiniBossType.Hapi) HasSlowTimeAbility = true;
            }

            // Brain
            if (_active.TryGetValue(OrgansType.Brain, out var brain))
            {
                if (brain == MiniBossType.Horus) DodgeChance = horusBrainDodgeChance;
            }

            // Lungs
            if (_active.TryGetValue(OrgansType.Lungs, out var lungs))
            {
                if (lungs == MiniBossType.Horus) ExtraDashCount = horusLungsExtraDash;
            }

            ModifiersChanged?.Invoke();
        }

        private void OnEnemyKilled(GameObject enemy)
        {
            if (!_active.TryGetValue(OrgansType.Stomach, out var stomach))
                return;

            _stomachKillCount++;

            if (stomach == MiniBossType.Horus)
            {
                if (_stomachKillCount >= horusStomachKillThreshold)
                {
                    _stomachKillCount = 0;
                    HealToFullRequested?.Invoke();

                    _stackedSpeedMult = Mathf.Min(_stackedSpeedMult * horusStomachSpeedBuff, horusStomachSpeedBuffMax);
                    SpeedBuffRequested?.Invoke(_stackedSpeedMult, 0f);
                }
            }
            else if (stomach == MiniBossType.Hapi)
            {
                if (_stomachKillCount >= hapiStomachKillThreshold)
                {
                    _stomachKillCount = 0;
                    _screenWipeReady = true;
                    ScreenWipeCharged?.Invoke();
                }
            }
        }

        private void OnDamageDealt(GameObject target, float damage)
        {
            // Khnum stomach lifesteal
            if (_active.TryGetValue(OrgansType.Stomach, out var stomach) && stomach == MiniBossType.Khnum)
            {
                float heal = Mathf.Max(0f, damage) * khnumStomachHealPercent;
                if (heal > 0f) HealAmountRequested?.Invoke(heal);
            }
        }

        private void OnOutgoingDamageModify(GameObject target, ref float damage)
        {
            if (damage <= 0f) return;

            // Khnum eyes: 50% double damage
            if (_active.TryGetValue(OrgansType.Eyes, out var eyes) && eyes == MiniBossType.Khnum)
            {
                if (UnityEngine.Random.value <= DoubleDamageChance) damage *= 2f;
            }

            // Khnum brain: instakill
            if (_active.TryGetValue(OrgansType.Brain, out var brain) && brain == MiniBossType.Khnum && target != null)
            {
                if (IsMinion(target))
                {
                    if (UnityEngine.Random.value <= khnumBrainMinionInstakillChance)
                    {
                        InstakillRequested?.Invoke(target);
                        damage = float.MaxValue;
                    }
                }
                else if (IsMiniBoss(target))
                {
                    if (UnityEngine.Random.value <= khnumBrainMiniBossInstakillChance)
                    {
                        InstakillRequested?.Invoke(target);
                        damage = float.MaxValue;
                    }
                }
            }

            // Hapi brain: taunt
            if (_active.TryGetValue(OrgansType.Brain, out var brain2) && brain2 == MiniBossType.Hapi && target != null)
            {
                float now = Time.unscaledTime;
                if (now >= _lastTauntUnscaled + hapiBrainTauntCooldown)
                {
                    if (UnityEngine.Random.value <= hapiBrainTauntChance)
                    {
                        _lastTauntUnscaled = now;
                        TauntRequested?.Invoke(target, hapiBrainTauntDuration);
                    }
                }
            }
        }

        private void OnIncomingDamageModify(GameObject source, ref float damage)
        {
            if (damage <= 0f) return;

            // Horus brain: 25% dodge
            if (_active.TryGetValue(OrgansType.Brain, out var brain) && brain == MiniBossType.Horus)
            {
                if (UnityEngine.Random.value <= DodgeChance) damage = 0f;
            }

        }

        public bool IsScreenWipeReady() => _screenWipeReady;
        public void ConsumeScreenWipe() => _screenWipeReady = false;

        public bool TryActivateKidneys(out float duration, out float speedMult, out float attackMult, out float healToPercent)
        {
            duration = 0f; speedMult = 1f; attackMult = 1f; healToPercent = 0f;

            if (!_active.TryGetValue(OrgansType.Kidneys, out var kidneys))
                return false;

            if (kidneys == MiniBossType.Horus)
            {
                duration = horusKidneysDuration;
                speedMult = horusKidneysSpeedMultiplier;
                return true;
            }
            if (kidneys == MiniBossType.Khnum)
            {
                duration = khnumKidneysDuration;
                attackMult = khnumKidneysAttackMultiplier;
                return true;
            }
            if (kidneys == MiniBossType.Hapi)
            {
                duration = hapiKidneysDuration;
                healToPercent = hapiKidneysHealToPercent;
                return true;
            }
            return false;
        }
        public bool TryGetLiver(out MiniBossType bossType)
        {
            bossType = default;
            if (!_active.TryGetValue(OrgansType.Liver, out var liverBoss))
                return false;

            bossType = liverBoss;
            return true;
        }


        private bool IsMinion(GameObject go) => go.CompareTag(minionTag);
        private bool IsMiniBoss(GameObject go) => go.CompareTag(miniBossTag);
    }
