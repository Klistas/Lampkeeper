using UnityEngine;

namespace Lampkeeper
{
    /// <summary>연료 조각. 프리팹 구성: SpriteRenderer + Collider2D(Is Trigger) + FuelPickup.</summary>
    [RequireComponent(typeof(Collider2D))]
    public class FuelPickup : MonoBehaviour, IPoolable
    {
        private float _amount;

        public void Init(float amount) => _amount = amount;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController player) || player.IsDead) return;
            if (player.Lantern != null) player.Lantern.AddFuel(_amount);
            PooledObject.Despawn(gameObject);
        }

        public void OnSpawned() { }
        public void OnDespawned() => _amount = 0f;
    }
}
