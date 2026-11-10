using UnityEngine;

namespace Lampkeeper
{
    /// <summary>가장 단순한 적: 플레이어를 향해 직진한다.</summary>
    public class ChaserEnemy : EnemyBase
    {
        protected override Vector2 GetDesiredVelocity()
        {
            Vector2 toTarget = (Vector2)Target.position - Body.position;
            return toTarget.normalized * Data.moveSpeed;
        }
    }
}
