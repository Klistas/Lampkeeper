using UnityEngine;

namespace Lampkeeper
{
    public enum UpgradeType
    {
        MaxHealth,      // value만큼 최대 HP 증가(같은 양 회복)
        MoveSpeed,      // value만큼 이동 속도 증가
        FireRate,       // 발사 간격에 (1 - value) 곱. 0.1 = 10% 빨라짐
        Damage,         // value만큼 탄 피해 증가
        MaxFuel         // value만큼 최대 연료 증가(같은 양 충전)
    }

    [CreateAssetMenu(fileName = "Upgrade", menuName = "Lampkeeper/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        public string displayName = "Upgrade";
        [TextArea] public string description;
        public UpgradeType type;
        public float value = 1f;
    }
}
