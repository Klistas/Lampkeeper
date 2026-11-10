namespace Lampkeeper
{
    /// <summary>PlayerData를 복사한 런타임 수치. 업그레이드는 여기에만 적용한다(SO 에셋 원본 보호).</summary>
    public class PlayerStats
    {
        public float MaxHealth { get; private set; }
        public float MoveSpeed { get; private set; }
        public float FireInterval { get; private set; }
        public float Damage { get; private set; }

        public PlayerStats(PlayerData data)
        {
            MaxHealth = data.maxHealth;
            MoveSpeed = data.moveSpeed;
            FireInterval = data.fireInterval;
            Damage = data.projectileDamage;
        }

        public void Apply(UpgradeData upgrade)
        {
            switch (upgrade.type)
            {
                case UpgradeType.MaxHealth: MaxHealth += upgrade.value; break;
                case UpgradeType.MoveSpeed: MoveSpeed += upgrade.value; break;
                case UpgradeType.FireRate:  FireInterval = UnityEngine.Mathf.Max(0.02f, FireInterval * (1f - upgrade.value)); break;
                case UpgradeType.Damage:    Damage += upgrade.value; break;
                case UpgradeType.MaxFuel:   break; // Lantern이 처리
            }
        }
    }
}
