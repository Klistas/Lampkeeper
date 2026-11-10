using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lampkeeper
{
    /// <summary>비동기 씬 로더. GameManager와 같은 오브젝트에 붙는다.</summary>
    public class SceneLoader : MonoBehaviour
    {
        public bool IsLoading { get; private set; }

        /// <summary>0~1. 로딩 화면을 만들 때 사용.</summary>
        public float Progress { get; private set; }

        public void Load(string sceneName, Action onLoaded = null)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"[SceneLoader] 이미 로딩 중이라 '{sceneName}' 요청을 무시합니다.");
                return;
            }
            StartCoroutine(LoadRoutine(sceneName, onLoaded));
        }

        private IEnumerator LoadRoutine(string sceneName, Action onLoaded)
        {
            IsLoading = true;
            Progress = 0f;

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null)
            {
                Debug.LogError($"[SceneLoader] 씬 '{sceneName}'을 불러올 수 없습니다. Build Profiles의 씬 목록에 추가했는지 확인하세요.");
                IsLoading = false;
                yield break;
            }

            while (!op.isDone)
            {
                Progress = op.progress;
                yield return null;
            }

            Progress = 1f;
            IsLoading = false;
            onLoaded?.Invoke();
        }
    }
}
