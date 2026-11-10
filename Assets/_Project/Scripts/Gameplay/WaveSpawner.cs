using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lampkeeper
{
    /// <summary>
    /// WaveData 배열을 순서대로 실행한다. Playing 상태로 들어올 때(Loading/WaveClear에서) 다음 웨이브를 시작하고,
    /// 스폰이 끝나고 살아 있는 적이 0이 되면 WaveClearedEvent를 보낸다.
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private WaveData[] waves;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private PlayerController player;

        public int CurrentWaveIndex => _waveIndex;
        public int TotalWaves => waves != null ? waves.Length : 0;

        private readonly Dictionary<EnemyData, ComponentPool<EnemyBase>> _pools = new Dictionary<EnemyData, ComponentPool<EnemyBase>>();
        private int _waveIndex = -1;
        private int _alive;
        private bool _spawning;
        private bool _waveActive;

        private void OnEnable()
        {
            EventBus<GameStateChangedEvent>.Subscribe(OnGameStateChanged);
            EventBus<EnemyKilledEvent>.Subscribe(OnEnemyKilled);
        }

        private void OnDisable()
        {
            EventBus<GameStateChangedEvent>.Unsubscribe(OnGameStateChanged);
            EventBus<EnemyKilledEvent>.Unsubscribe(OnEnemyKilled);
        }

        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            if (e.Current != GameState.Playing) return;
            bool freshStart = e.Previous == GameState.Loading || e.Previous == GameState.Boot;
            if (freshStart || e.Previous == GameState.WaveClear) StartNextWave();
            // Paused -> Playing 은 진행 중인 웨이브를 그대로 이어간다.
        }

        private void StartNextWave()
        {
            if (_waveActive) return;
            if (_waveIndex + 1 >= TotalWaves)
            {
                Debug.LogWarning("[WaveSpawner] 남은 웨이브가 없습니다. waves 배열을 확인하세요.", this);
                return;
            }

            _waveIndex++;
            StartCoroutine(RunWave(waves[_waveIndex]));
        }

        private IEnumerator RunWave(WaveData wave)
        {
            _waveActive = true;
            _spawning = true;
            EventBus<WaveStartedEvent>.Publish(new WaveStartedEvent(_waveIndex, TotalWaves));

            yield return new WaitForSeconds(wave.startDelay); // timeScale 0(일시정지)이면 멈춘다

            foreach (WaveData.SpawnGroup group in wave.groups)
            {
                if (group.enemy == null || group.enemy.prefab == null)
                {
                    Debug.LogWarning($"[WaveSpawner] {wave.name}: enemy 또는 prefab이 비어 있는 그룹을 건너뜁니다.", wave);
                    continue;
                }

                for (int i = 0; i < group.count; i++)
                {
                    Spawn(group.enemy);
                    yield return new WaitForSeconds(group.spawnInterval);
                }
            }

            _spawning = false;
            CheckWaveCleared();
        }

        private void Spawn(EnemyData data)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogError("[WaveSpawner] spawnPoints가 비어 있습니다.", this);
                return;
            }

            if (!_pools.TryGetValue(data, out ComponentPool<EnemyBase> pool))
            {
                pool = new ComponentPool<EnemyBase>(data.prefab, data.prewarm);
                _pools.Add(data, pool);
            }

            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            EnemyBase enemy = pool.Get(point.position, Quaternion.identity);
            enemy.Init(data, player != null ? player.transform : null, player != null ? player.Lantern : null);
            _alive++;
        }

        private void OnEnemyKilled(EnemyKilledEvent e)
        {
            _alive = Mathf.Max(0, _alive - 1);
            CheckWaveCleared();
        }

        private void CheckWaveCleared()
        {
            if (!_waveActive || _spawning || _alive > 0) return;
            _waveActive = false;
            EventBus<WaveClearedEvent>.Publish(new WaveClearedEvent(_waveIndex, _waveIndex >= TotalWaves - 1));
        }
    }
}
