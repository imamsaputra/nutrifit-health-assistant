using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NutriFit
{
    /// <summary>
    /// Mengatur perilaku Welcome Page: fade-in saat mulai, ilustrasi yang
    /// bergerak naik-turun pelan, dan aksi tombol "Mulai Sekarang".
    /// </summary>
    public class WelcomeController : MonoBehaviour
    {
        [Header("Scene tujuan saat tombol ditekan")]
        [Tooltip("Isi dengan nama scene berikutnya (harus sudah ada di Build Settings).")]
        public string nextSceneName = "";

        [Header("Referensi (diisi otomatis oleh builder)")]
        public CanvasGroup contentGroup;
        public RectTransform illustration;

        [Header("Animasi")]
        public float fadeDuration = 0.8f;
        public float bobAmount = 12f;
        public float bobSpeed = 1.5f;

        private Vector2 _basePos;

        private void Start()
        {
            if (illustration != null) _basePos = illustration.anchoredPosition;
            if (contentGroup != null)
            {
                contentGroup.alpha = 0f;
                StartCoroutine(FadeIn());
            }
        }

        private IEnumerator FadeIn()
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, fadeDuration);
                contentGroup.alpha = Mathf.SmoothStep(0f, 1f, t);
                yield return null;
            }
            contentGroup.alpha = 1f;
        }

        private void Update()
        {
            if (illustration != null)
                illustration.anchoredPosition = _basePos + Vector2.up * (Mathf.Sin(Time.time * bobSpeed) * bobAmount);
        }

        /// <summary>Dipanggil oleh tombol "Mulai Sekarang".</summary>
        public void OnStartClicked()
        {
            if (!string.IsNullOrEmpty(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
                SceneManager.LoadScene(nextSceneName);
            else
                Debug.Log("[NutriFit] Tombol 'Mulai Sekarang' ditekan. Isi 'Next Scene Name' di WelcomeController untuk pindah scene.");
        }
    }
}
