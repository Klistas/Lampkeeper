using UnityEngine;

namespace Lampkeeper
{
    /// <summary>
    /// 플레이어 탄. 프리팹 구성: SpriteRenderer + Collider2D(Is Trigger) + Rigidbody2D(Kinematic) + Projectile.
    /// 등불 빛 반경 밖의 적은 맞지 않고 통과한다.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        private Vector2 _velocity;
        private float _damage;
        private float _lifeLeft;

        public void Launch(Vector2 direction, float speed, float damage, float lifetime)
        {
            _velocity = direction.normalized * speed;
            _damage = damage;
            _lifeLeft = lifetime;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Update()
        {
            transform.position += (Vector3)(_velocity * Time.deltaTime);
            _lifeLeft -= Time.deltaTime;
            if (_lifeLeft <= 0f) PooledObject.Despawn(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out EnemyBase enemy) || enemy.IsDead) return;
            if (!enemy.CanBeHit) return;

            enemy.TakeDamage(_damage);
            PooledObject.Despawn(gameObject);
        }

        public void OnSpawned() { }

        public void OnDespawned()
        {
            _velocity = Vector2.zero;
            _lifeLeft = 0f;
        }
    }
}
