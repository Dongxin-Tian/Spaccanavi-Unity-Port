using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spaccanavi
{
    public sealed class TransitionScene : MonoBehaviour
    {
        public static string TargetSceneName { get; set; } = string.Empty;

        private void Start()
            => SceneManager.LoadScene(TargetSceneName);
    }
}
