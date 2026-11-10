namespace Lampkeeper
{
    /// <summary>게임 전체 흐름 상태. 전환 규칙은 GameManager.CanTransition 참고.</summary>
    public enum GameState
    {
        Boot,       // 최초 실행 직후
        MainMenu,
        Loading,    // 씬 전환 중
        Playing,
        Paused,
        WaveClear,  // 웨이브 사이 (업그레이드 선택)
        GameOver,
        Victory
    }

    /// <summary>씬 이름 상수. Build Profiles(Build Settings)의 씬 이름과 같아야 한다.</summary>
    public static class SceneNames
    {
        public const string Boot = "Boot";
        public const string MainMenu = "MainMenu";
        public const string Game = "Game";
    }
}
