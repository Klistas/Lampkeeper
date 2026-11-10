using UnityEngine;

namespace Lampkeeper
{
    /// <summary>EnemyKilledEvent를 받아 연료를 떨어뜨린다. 이벤트 버스 구독 예시. Game 씬의 아무 오브젝트에 붙인다.</summary>
    public class FuelDropper : MonoBehaviour
    {
        [SerializeField] private FuelPickup pickupPrefab;
        [SerializeField, Min(0)] private int prewarm = 10;

        private ComponentPool<FuelPickup> _pool;

        private void Awake()
        {
            if (pickupPrefab != null) _pool = new ComponentPool<FuelPickup>(pickupPrefab, prewarm);
            else Debug.LogWarning("[FuelDropper] pickupPrefab이 비어 있습니다.", this);
        }

        private void OnEnable() => EventBus<EnemyKilledEvent>.Subscribe(OnEnemyKilled);
        private void OnDisable() => EventBus<EnemyKilledEvent>.Unsubscribe(OnEnemyKilled);

        private void OnEnemyKilled(EnemyKilledEvent e)
        {
            if (_pool == null || e.Data == null) return;
            if (Random.value > e.Data.fuelDropChance) return;

            FuelPickup pickup = _pool.Get(e.Position, Quaternion.identity);
            pickup.Init(e.Data.fuelDropAmount);
        }
    }
}
