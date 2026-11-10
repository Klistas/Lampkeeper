using UnityEngine;

namespace Lampkeeper
{
    /// <summary>임시 메인 메뉴 (OnGUI). MainMenu 씬의 빈 오브젝트에 붙인다. 과제: UGUI 버튼으로 교체.</summary>
    public class MainMenuView : MonoBehaviour
    {
        private void OnGUI()
        {
            GameManager gm = GameManager.Instance;
            if (gm == null)
            {
                GUILayout.Label("GameManager not found. Start from the Boot scene.");
                return;
            }

            float w = 200f, h = 120f;
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h), GUI.skin.box);
            GUILayout.Label("LAMPKEEPER");
            GUI.enabled = gm.State == GameState.MainMenu;
            if (GUILayout.Button("Start")) gm.StartGame();
            if (GUILayout.Button("Quit")) gm.QuitGame();
            GUI.enabled = true;
            GUILayout.EndArea();
        }
    }
}
