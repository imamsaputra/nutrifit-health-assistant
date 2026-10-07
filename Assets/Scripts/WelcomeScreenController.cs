using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NutriFitTools
{
    public class WelcomeScreenController : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button loginButton;

        [SerializeField] private string nextSceneName = "Onboarding";
        [SerializeField] private string loginSceneName = "Login";

        private void Awake()
        {
            if (startButton != null)
                startButton.onClick.AddListener(OnStartClicked);

            if (loginButton != null)
                loginButton.onClick.AddListener(OnLoginClicked);
        }

        private void OnDestroy()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(OnStartClicked);

            if (loginButton != null)
                loginButton.onClick.RemoveListener(OnLoginClicked);
        }

        public void OnStartClicked()
        {
            LoadScene(nextSceneName);
        }

        public void OnLoginClicked()
        {
            LoadScene(loginSceneName);
        }

        private void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("NutriFit: nama scene belum diisi.");
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogWarning(
                    "NutriFit: scene '" + sceneName +
                    "' belum ditambahkan ke Build Settings / Build Profiles.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
