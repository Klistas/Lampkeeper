using UnityEngine;

namespace Lampkeeper
{
    public readonly struct GameStateChangedEvent : IGameEvent
    {
        public readonly GameState Previous;
        public readonly GameState Current;
        public GameStateChangedEvent(GameState previous, GameState current) { Previous = previous; Current = current; }
    }

    public readonly struct WaveStartedEvent : IGameEvent
    {
        public readonly int WaveIndex;   // 0부터 시작
        public readonly int TotalWaves;
        public WaveStartedEvent(int waveIndex, int totalWaves) { WaveIndex = waveIndex; TotalWaves = totalWaves; }
    }

    public readonly struct WaveClearedEvent : IGameEvent
    {
        public readonly int WaveIndex;
        public readonly bool IsLastWave;
        public WaveClearedEvent(int waveIndex, bool isLastWave) { WaveIndex = waveIndex; IsLastWave = isLastWave; }
    }

    public readonly struct EnemyKilledEvent : IGameEvent
    {
        public readonly Vector2 Position;
        public readonly EnemyData Data;
        public EnemyKilledEvent(Vector2 position, EnemyData data) { Position = position; Data = data; }
    }

    public readonly struct PlayerHealthChangedEvent : IGameEvent
    {
        public readonly float Current;
        public readonly float Max;
        public PlayerHealthChangedEvent(float current, float max) { Current = current; Max = max; }
    }

    public readonly struct PlayerDiedEvent : IGameEvent { }

    public readonly struct LanternFuelChangedEvent : IGameEvent
    {
        public readonly float Current;
        public readonly float Max;
        public LanternFuelChangedEvent(float current, float max) { Current = current; Max = max; }
    }

    public readonly struct LanternEmptiedEvent : IGameEvent { }
}
