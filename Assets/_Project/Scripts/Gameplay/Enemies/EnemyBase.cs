using UnityEngine;

namespace Lampkeeper
{
    /// <summary>
    /// 적 베이스. 새 적은 이 클래스를 상속하고 GetDesiredVelocity만 구현하면 된다.
    /// 프리팹 구성: SpriteRenderer + CircleCollider2D(트리거 아님) + Rigidbody2D(Dynamic, Gravity Scale 0, Freeze Rotation Z) + 파생 클래스
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : Actor, IPoolable
    {
        public EnemyData Data { get; private set; }
        protected Transform Target { get; private set; }
        protected Lantern Lantern { get; private set; }
        protected Rigidbody2D Body { get; private set; }

        /// <summary>등불 빛 반경 안에 있을 때만 탄에 맞는다.</summary>
        public bool CanBeHit => Lantern == null || Lantern.Contains(Body.position);

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
        }

        /// <summary>풀에서 꺼낸 직후 스포너가 호출한다.</summary>
        public virtual void Init(EnemyData data, Transform target, Lantern lantern)
        {
            Data = data;
            Target = target;
            Lantern = lantern;
            ResetHealth(data.maxHealth);
        }

        protected virtual void FixedUpdate()
        {
            if (IsDead || Data == null) return;
            bool hasTarget = Target != null && Target.gameObject.activeInHierarchy;
            Body.linearVelocity = hasTarget ? GetDesiredVelocity() : Vector2.zero;
        }

        /// <summary>이번 물리 프레임에 원하는 속도(유닛/초).</summary>
        protected abstract Vector2 GetDesiredVelocity();

        protected virtual void OnCollisionStay2D(Collision2D collision)
        {
            if (IsDead || Data == null) return;
            if (collision.collider.TryGetComponent(out PlayerController player))
                player.TakeDamage(Data.contactDamage); // 연속 피해는 플레이어 무적 시간이 막는다
        }

        protected override void Die()
        {
            EventBus<EnemyKilledEvent>.Publish(new EnemyKilledEvent(Body.position, Data));
            PooledObject.Despawn(gameObject);
        }

        public virtual void OnSpawned() { }

        public virtual void OnDespawned()
        {
            Body.linearVelocity = Vector2.zero;
            Data = null;
            Target = null;
            Lantern = null;
        }
    }
}
