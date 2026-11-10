using UnityEngine;

namespace Lampkeeper
{
    /// <summary>플레이어 기본 수치. 런타임에 이 에셋 값을 직접 바꾸지 말 것(에디터에서는 플레이 후에도 바뀐 값이 남는다). 변경은 PlayerStats에.</summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Lampkeeper/Player Data")]
    public class PlayerData : ScriptableObject
    {
        [Header("Body")]
        [Min(1f)] public float maxHealth = 5f;
        [Min(0f)] public float moveSpeed = 5f;
        [Min(0f)] public float invincibleTime = 0.8f;

        [Header("Weapon")]
        public Projectile projectilePrefab;
        [Min(0)] public int projectilePrewarm = 30;
        [Min(0.02f)] public float fireInterval = 0.2f;
        [Min(0f)] public float projectileSpeed = 14f;
        [Min(0f)] public float projectileDamage = 1f;
        [Min(0.1f)] public float projectileLifetime = 1.5f;

        [Header("Lantern")]
        [Min(1f)] public float maxFuel = 100f;
        [Min(0f)] public float fuelDrainPerSecond = 2f;
        [Min(0.1f)] public float minLightRadius = 1.5f;
        [Min(0.1f)] public float maxLightRadius = 6f;
    }
}
