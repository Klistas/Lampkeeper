using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lampkeeper
{
    /// <summary>
    /// 게임 상태 머신 + 전역 서비스(SceneLoader, InputReader) 보관.
    /// Boot 씬에 하나만 두고 DontDestroyOnLoad로 유지한다.
    /// 상태는 반드시 ChangeState로만 바꾼다.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SceneLoader), typeof(InputReader))]
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; } = GameState.Boot;
        public SceneLoader Scenes { get; private set; }
        public InputReader Input { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // 테스트용으로 다른 씬에 놓아 둔 중복 인스턴스 제거
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            Scenes = GetComponent<SceneLoader>();
            Input = GetComponent<InputReader>();
        }

        private void OnEnable()
        {
            if (Instance != this) return;
            Input.PausePressed += TogglePause;
            EventBus<WaveClearedEvent>.Subscribe(OnWaveCleared);
            EventBus<PlayerDiedEvent>.Subscribe(OnPlayerDied);
            EventBus<LanternEmptiedEvent>.Subscribe(OnLanternEmptied);
        }

        private void OnDisable()
        {
            if (Instance != this) return;
            Input.PausePressed -= TogglePause;
            EventBus<WaveClearedEvent>.Unsubscribe(OnWaveCleared);
            EventBus<PlayerDiedEvent>.Unsubscribe(OnPlayerDied);
            EventBus<LanternEmptiedEvent>.Unsubscribe(OnLanternEmptied);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            if (Instance != this) return;

            string scene = SceneManager.GetActiveScene().name;
            if (scene == SceneNames.Boot) GoToMainMenu();
            else if (scene == SceneNames.Game) ChangeState(GameState.Playing);   // 에디터에서 Game 씬부터 실행한 경우
            else ChangeState(GameState.MainMenu);
        }

        // ---------- 외부에서 호출하는 API ----------

        public void StartGame()
        {
            if (!ChangeState(GameState.Loading)) return;
            Scenes.Load(SceneNames.Game, () => ChangeState(GameState.Playing));
        }

        public void GoToMainMenu()
        {
            if (!ChangeState(GameState.Loading)) return;
            Scenes.Load(SceneNames.MainMenu, () => ChangeState(GameState.MainMenu));
        }

        public void TogglePause()
        {
            if (State == GameState.Playing) ChangeState(GameState.Paused);
            else if (State == GameState.Paused) ChangeState(GameState.Playing);
        }

        public void ContinueAfterWaveClear()
        {
            if (State == GameState.WaveClear) ChangeState(GameState.Playing);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------- 상태 머신 ----------

        public bool ChangeState(GameState next)
        {
            if (next == State) return false;
            if (!CanTransition(State, next))
            {
                Debug.LogWarning($"[GameManager] 허용되지 않은 상태 전환: {State} -> {next}");
                return false;
            }

            GameState previous = State;
            State = next;
            OnEnterState(next);
            EventBus<GameStateChangedEvent>.Publish(new GameStateChangedEvent(previous, next));
            return true;
        }

        private static bool CanTransition(GameState from, GameState to)
        {
            if (from == GameState.Boot) return true; // 에디터에서 아무 씬부터 시작해도 되도록

            switch (from)
            {
                case GameState.MainMenu:  return to == GameState.Loading;
                case GameState.Loading:   return to == GameState.MainMenu || to == GameState.Playing;
                case GameState.Playing:   return to == GameState.Paused || to == GameState.WaveClear
                                              || to == GameState.GameOver || to == GameState.Victory;
                case GameState.Paused:    return to == GameState.Playing || to == GameState.Loading;
                case GameState.WaveClear: return to == GameState.Playing || to == GameState.Loading;
                case GameState.GameOver:  return to == GameState.Loading;
                case GameState.Victory:   return to == GameState.Loading;
                default:                  return false;
            }
        }

        private void OnEnterState(GameState state)
        {
            bool frozen = state == GameState.Paused || state == GameState.WaveClear
                       || state == GameState.GameOver || state == GameState.Victory;
            Time.timeScale = frozen ? 0f : 1f;
            Input.SetGameplayEnabled(state == GameState.Playing);
        }

        // ---------- 이벤트 처리 ----------

        private void OnWaveCleared(WaveClearedEvent e)
        {
            if (State != GameState.Playing) return;
            ChangeState(e.IsLastWave ? GameState.Victory : GameState.WaveClear);
        }

        private void OnPlayerDied(PlayerDiedEvent e)
        {
            if (State == GameState.Playing) ChangeState(GameState.GameOver);
        }

        private void OnLanternEmptied(LanternEmptiedEvent e)
        {
            if (State == GameState.Playing) ChangeState(GameState.GameOver);
        }
    }
}
