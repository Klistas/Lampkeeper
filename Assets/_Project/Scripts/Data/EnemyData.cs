using UnityEngine;

namespace Lampkeeper
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Lampkeeper/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public string displayName = "Shadow";
        public EnemyBase prefab;
        [Min(0)] public int prewarm = 10;

        [Header("Stats")]
        [Min(1f)] public float maxHealth = 3f;
        [Min(0f)] public float moveSpeed = 2f;
        [Min(0f)] public float contactDamage = 1f;

        [Header("Drop")]
        [Range(0f, 1f)] public float fuelDropChance = 0.4f;
        [Min(0f)] public float fuelDropAmount = 10f;
    }
}
