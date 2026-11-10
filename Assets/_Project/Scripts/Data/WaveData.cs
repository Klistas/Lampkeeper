using System;
using UnityEngine;

namespace Lampkeeper
{
    [CreateAssetMenu(fileName = "Wave_01", menuName = "Lampkeeper/Wave Data")]
    public class WaveData : ScriptableObject
    {
        [Min(0f)] public float startDelay = 2f;
        public SpawnGroup[] groups = Array.Empty<SpawnGroup>();

        /// <summary>그룹은 위에서부터 순서대로 스폰된다.</summary>
        [Serializable]
        public struct SpawnGroup
        {
            public EnemyData enemy;
            [Min(1)] public int count;
            [Min(0f)] public float spawnInterval;
        }
    }
}
