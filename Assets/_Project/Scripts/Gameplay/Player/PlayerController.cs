using UnityEngine;

namespace Lampkeeper
{
    /// <summary>
    /// 플레이어 베이스 예시. 프리팹 구성:
    /// SpriteRenderer + CircleCollider2D + Rigidbody2D(Dynamic, Gravity Scale 0, Freeze Rotation Z) + PlayerController
    /// 자식: Lantern (+ 빛 표시용 원 스프라이트)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : Actor
    {
        [SerializeField] private PlayerData data;
        [SerializeField] private Lantern lantern;
        [SerializeField] private Camera viewCamera;
        [SerializeField, Min(0f)] private float muzzleOffset = 0.6f;

        public PlayerStats Stats { get; private set; }
        public Lantern Lantern => lantern;
        public Vector2 AimDirection { get; private set; } = Vector2.right;

        private Rigidbody2D _body;
        private InputReader _input;
        private ComponentPool<Projectile> _projectiles;
        private float _fireCooldown;
        private float _invincibleTimer;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            if (viewCamera == null) viewCamera = Camera.main;

            Stats = new PlayerStats(data);
            ResetHealth(Stats.MaxHealth);

            if (data.projectilePrefab != null)
                _projectiles = new ComponentPool<Projectile>(data.projectilePrefab, data.projectilePrewarm);
            else
                Debug.LogWarning("[PlayerController] PlayerData.projectilePrefab이 비어 있습니다.", this);
        }

        private void Start()
        {
            if (GameManager.Instance != null) _input = GameManager.Instance.Input;
            else Debug.LogWarning("[PlayerController] GameManager가 없습니다. Boot 씬부터 실행하거나 GameManager 프리팹을 씬에 두세요.", this);

            OnHealthChanged(); // HUD 초기값
        }

        private void Update()
        {
            if (_input == null || IsDead) return;

            _fireCooldown -= Time.deltaTime;
            _invincibleTimer -= Time.deltaTime;

            UpdateAim();

            if (_input.FireHeld && _fireCooldown <= 0f) Fire();
        }

        private void FixedUpdate()
        {
            Vector2 move = (_input != null && !IsDead) ? _input.Move : Vector2.zero;
            _body.linearVelocity = move * Stats.MoveSpeed;
        }

        private void UpdateAim()
        {
            if (viewCamera == null) return;
            Vector2 pointerWorld = viewCamera.ScreenToWorldPoint(_input.PointerScreenPosition);
            Vector2 dir = pointerWorld - _body.position;
            if (dir.sqrMagnitude > 0.0001f) AimDirection = dir.normalized;
        }

        private void Fire()
        {
            if (_projectiles == null) return;
            _fireCooldown = Stats.FireInterval;

            Vector2 spawnPos = _body.position + AimDirection * muzzleOffset;
            Projectile p = _projectiles.Get(spawnPos, Quaternion.identity);
            p.Launch(AimDirection, data.projectileSpeed, Stats.Damage, data.projectileLifetime);
        }

        public override void TakeDamage(float amount)
        {
            if (_invincibleTimer > 0f) return;
            base.TakeDamage(amount);
        }

        public void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null) return;
            Stats.Apply(upgrade);
            if (upgrade.type == UpgradeType.MaxHealth) IncreaseMaxHealth(upgrade.value);
            if (upgrade.type == UpgradeType.MaxFuel && lantern != null) lantern.AddMaxFuel(upgrade.value);
        }

        protected override void OnHealthChanged()
        {
            EventBus<PlayerHealthChangedEvent>.Publish(new PlayerHealthChangedEvent(CurrentHealth, MaxHealth));
        }

        protected override void OnDamaged(float amount)
        {
            _invincibleTimer = data.invincibleTime;
            // TODO(학생): 깜빡임, 화면 흔들림, 효과음
        }

        protected override void Die()
        {
            _body.linearVelocity = Vector2.zero;
            EventBus<PlayerDiedEvent>.Publish(new PlayerDiedEvent());
            gameObject.SetActive(false);
        }
    }
}
