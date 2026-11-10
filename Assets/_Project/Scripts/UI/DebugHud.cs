using UnityEngine;

namespace Lampkeeper
{
    /// <summary>
    /// 임시 HUD (OnGUI). 이벤트 버스로 값을 받아 표시한다.
    /// 과제: UGUI(Canvas) 또는 UI Toolkit으로 교체하고 이 파일은 지운다.
    /// </summary>
    public class DebugHud : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [Tooltip("웨이브 사이에 고를 업그레이드 후보. 비어 있으면 Continue 버튼만 나온다.")]
        [SerializeField] private UpgradeData[] upgradeChoices;

        private float _hp, _hpMax, _fuel, _fuelMax;
        private int _wave, _waveTotal;

        private void OnEnable()
        {
            EventBus<PlayerHealthChangedEvent>.Subscribe(OnHealth);
            EventBus<LanternFuelChangedEvent>.Subscribe(OnFuel);
            EventBus<WaveStartedEvent>.Subscribe(OnWave);
        }

        private void OnDisable()
        {
            EventBus<PlayerHealthChangedEvent>.Unsubscribe(OnHealth);
            EventBus<LanternFuelChangedEvent>.Unsubscribe(OnFuel);
            EventBus<WaveStartedEvent>.Unsubscribe(OnWave);
        }

        private void OnHealth(PlayerHealthChangedEvent e) { _hp = e.Current; _hpMax = e.Max; }
        private void OnFuel(LanternFuelChangedEvent e) { _fuel = e.Current; _fuelMax = e.Max; }
        private void OnWave(WaveStartedEvent e) { _wave = e.WaveIndex + 1; _waveTotal = e.TotalWaves; }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 260, 400), GUI.skin.box);
            GUILayout.Label($"HP   {_hp:0}/{_hpMax:0}");
            GUILayout.Label($"Fuel {_fuel:0}/{_fuelMax:0}");
            GUILayout.Label($"Wave {_wave}/{_waveTotal}");

            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                GUILayout.Label($"State: {gm.State}");
                DrawStateButtons(gm);
            }
            GUILayout.EndArea();
        }

        private void DrawStateButtons(GameManager gm)
        {
            switch (gm.State)
            {
                case GameState.Paused:
                    if (GUILayout.Button("Resume")) gm.TogglePause();
                    if (GUILayout.Button("Main Menu")) gm.GoToMainMenu();
                    break;

                case GameState.WaveClear:
                    if (upgradeChoices != null && upgradeChoices.Length > 0 && player != null)
                    {
                        // TODO(학생): 무작위 3개 뽑기
                        for (int i = 0; i < upgradeChoices.Length && i < 3; i++)
                        {
                            UpgradeData u = upgradeChoices[i];
                            if (u == null) continue;
                            if (GUILayout.Button($"{u.displayName}\n{u.description}"))
                            {
                                player.ApplyUpgrade(u);
                                gm.ContinueAfterWaveClear();
                            }
                        }
                    }
                    else if (GUILayout.Button("Continue")) gm.ContinueAfterWaveClear();
                    break;

                case GameState.GameOver:
                case GameState.Victory:
                    GUILayout.Label(gm.State == GameState.Victory ? "You survived!" : "The light went out.");
                    if (GUILayout.Button("Retry")) gm.StartGame();
                    if (GUILayout.Button("Main Menu")) gm.GoToMainMenu();
                    break;
            }
        }
    }
}
