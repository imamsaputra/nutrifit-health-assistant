using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem.UI;
#endif

/// <summary>
/// Welcome screen NutriFit.
/// UI dibuat lewat menu Tools > NutriFit > Create Welcome Screen (muncul di Hierarchy & Scene View).
/// Jika UI belum dibuat, script otomatis membuatnya saat Play.
/// Butuh TextMeshPro (Window > TextMeshPro > Import TMP Essential Resources).
/// </summary>
[DisallowMultipleComponent]
public class WelcomeScreen : MonoBehaviour
{
    [System.Serializable]
    public class Page
    {
        [TextArea(1, 3)] public string title;
        [TextArea(1, 3)] public string subtitle;
        public Sprite illustration;
    }

    [Header("Konten")]
    public string appName = "NutriFit";
    public string tagline = "Health Assistants";
    [Tooltip("Logo lengkap (ikon + tulisan). Jika kosong, dipakai teks appName + tagline.")]
    public Sprite logoSprite;
    [Tooltip("Pakai <accent>kata</accent> untuk mewarnai kata dengan Accent Color.")]
    public Page[] pages =
    {
        new Page
        {
            title = "Selamat Datang\ndi <accent>NutriFit</accent>!",
            subtitle = "Mulai perjalanan hidup sehatmu\nbersama kami.",
        },
    };
    public string startLabel = "Mulai";
    public string skipLabel = "Lewati";

    [Header("Tampilan")]
    public bool showArrow = true;
    public bool showSkip = false;
    [Tooltip("Dots hanya tampil jika halaman lebih dari 1.")]
    public bool showDots = true;
    public bool playIntroAnimation = true;

    [Header("Navigasi")]
    public string nextSceneName = "Login";
    public bool showOnlyOnce = true;
    public bool autoSlide = false;
    public float autoSlideInterval = 4f;
    public float swipeThreshold = 80f;
    public float transitionTime = 0.25f;

    [Header("Warna (klik Refresh Preview di bawah setelah mengubah)")]
    public Color backgroundColor = new Color32(0xFA, 0xFB, 0xF6, 255);
    public Color titleColor = new Color32(0x0F, 0x5C, 0x3D, 255);
    public Color accentColor = new Color32(0x3C, 0xAE, 0x57, 255);
    public Color subtitleColor = new Color32(0x7C, 0x8B, 0x86, 255);
    public Color brandColor = new Color32(0x1E, 0x8E, 0x4A, 255);
    public Color buttonColor = new Color32(0x5D, 0xB0, 0x5C, 255);
    public Color buttonTextColor = Color.white;
    public Color dotActiveColor = new Color32(0x5D, 0xB0, 0x5C, 255);
    public Color dotInactiveColor = new Color32(0xCF, 0xE5, 0xCF, 255);

    [Header("Aset UI (diisi otomatis oleh menu editor)")]
    public Sprite roundedSprite;
    public Sprite arrowSprite;

    [Header("Event (opsional)")]
    public UnityEvent onGetStarted;
    public UnityEvent onSkip;
    public UnityEvent<int> onPageChanged;

    // ---- referensi UI hasil build (tersimpan di scene) ----
    [SerializeField, HideInInspector] GameObject canvasRoot;
    [SerializeField, HideInInspector] CanvasGroup rootGroup;
    [SerializeField, HideInInspector] CanvasGroup contentGroup;
    [SerializeField, HideInInspector] RectTransform contentRT;
    [SerializeField, HideInInspector] Image backgroundImage;
    [SerializeField, HideInInspector] Image logoImage;
    [SerializeField, HideInInspector] TextMeshProUGUI brandText;
    [SerializeField, HideInInspector] TextMeshProUGUI taglineText;
    [SerializeField, HideInInspector] Image illustration;
    [SerializeField, HideInInspector] TextMeshProUGUI titleText;
    [SerializeField, HideInInspector] TextMeshProUGUI subtitleText;
    [SerializeField, HideInInspector] Transform dotsRoot;
    [SerializeField, HideInInspector] Image[] dots;
    [SerializeField, HideInInspector] Button startButton;
    [SerializeField, HideInInspector] RectTransform buttonRow;
    [SerializeField, HideInInspector] TextMeshProUGUI startText;
    [SerializeField, HideInInspector] Image arrowImage;
    [SerializeField, HideInInspector] Button skipButton;
    [SerializeField, HideInInspector] TextMeshProUGUI skipText;
    [SerializeField, HideInInspector] WelcomeSwipeArea swipeArea;

    public const string PrefKey = "NutriFitWelcomeSeen";
    const int UILayer = 5;

    int current;
    bool busy, dragging, finishing;
    float idleTimer;
    static Sprite runtimeRounded, runtimeArrow;

    public bool HasUI => canvasRoot != null;

    // ------------------------------------------------------------------ lifecycle

    void Awake()
    {
        if (showOnlyOnce && PlayerPrefs.GetInt(PrefKey, 0) == 1 && CanLoadNext())
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        EnsureEventSystem();
        if (!HasUI) BuildUI();

        if (dots == null || dots.Length != pages.Length) BuildDots();
        WireUp();
        ApplyStatic();
        ApplyPage(0);

        if (playIntroAnimation) StartCoroutine(Intro());
    }

    void Update()
    {
        if (!autoSlide || busy || dragging || finishing || pages.Length < 2) return;
        idleTimer += Time.unscaledDeltaTime;
        if (idleTimer >= autoSlideInterval)
            GoToPage((current + 1) % pages.Length, 1);
    }

    void WireUp()
    {
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(OnGetStartedClicked);
        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(OnSkipClicked);
        swipeArea.onDragStart = () => dragging = true;
        swipeArea.onSwipe = HandleSwipe;
    }

    // ------------------------------------------------------------------ fungsi tombol & navigasi

    /// <summary>Tombol "Mulai": tandai selesai lalu pindah ke scene berikutnya.</summary>
    public void OnGetStartedClicked()
    {
        onGetStarted?.Invoke();
        Finish();
    }

    /// <summary>Tombol "Lewati" (opsional, aktifkan lewat Show Skip).</summary>
    public void OnSkipClicked()
    {
        onSkip?.Invoke();
        Finish();
    }

    public void NextPage()
    {
        if (current < pages.Length - 1) GoToPage(current + 1, 1);
    }

    public void PreviousPage()
    {
        if (current > 0) GoToPage(current - 1, -1);
    }

    void GoToPage(int index, int direction)
    {
        idleTimer = 0f;
        if (busy || finishing || index == current) return;
        StartCoroutine(PageTransition(index, direction));
    }

    void HandleSwipe(float deltaX)
    {
        dragging = false;
        idleTimer = 0f;
        if (deltaX <= -swipeThreshold) NextPage();
        else if (deltaX >= swipeThreshold) PreviousPage();
    }

    bool CanLoadNext()
    {
        return !string.IsNullOrEmpty(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName);
    }

    void Finish()
    {
        if (finishing) return;

        if (!string.IsNullOrEmpty(nextSceneName) && !CanLoadNext())
        {
            Debug.LogWarning($"[WelcomeScreen] Scene \"{nextSceneName}\" tidak ada di Build Settings (File > Build Settings > Add Open Scenes).");
            return;
        }

        finishing = true;
        PlayerPrefs.SetInt(PrefKey, 1);
        PlayerPrefs.Save();
        StartCoroutine(FadeOutAndLoad());
    }

    [ContextMenu("Reset Welcome Seen")]
    public void ResetSeen()
    {
        PlayerPrefs.DeleteKey(PrefKey);
        PlayerPrefs.Save();
        Debug.Log("[WelcomeScreen] Flag direset.");
    }

    // ------------------------------------------------------------------ animasi

    IEnumerator Intro()
    {
        busy = true;
        const float dur = 0.6f;
        for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
        {
            float k = 1f - Mathf.Pow(1f - t / dur, 3f); // ease-out
            rootGroup.alpha = k;
            contentRT.anchoredPosition = new Vector2(0, -40f * (1f - k));
            yield return null;
        }
        rootGroup.alpha = 1f;
        contentRT.anchoredPosition = Vector2.zero;
        busy = false;
    }

    IEnumerator PageTransition(int index, int dir)
    {
        busy = true;

        for (float t = 0; t < transitionTime; t += Time.unscaledDeltaTime)
        {
            float k = t / transitionTime;
            contentGroup.alpha = 1f - k;
            contentRT.anchoredPosition = new Vector2(-dir * 80f * k, 0);
            yield return null;
        }

        ApplyPage(index);

        for (float t = 0; t < transitionTime; t += Time.unscaledDeltaTime)
        {
            float k = t / transitionTime;
            contentGroup.alpha = k;
            contentRT.anchoredPosition = new Vector2(dir * 80f * (1f - k), 0);
            yield return null;
        }

        contentGroup.alpha = 1f;
        contentRT.anchoredPosition = Vector2.zero;
        busy = false;
    }

    IEnumerator FadeOutAndLoad()
    {
        for (float t = 0; t < 0.3f; t += Time.unscaledDeltaTime)
        {
            rootGroup.alpha = 1f - t / 0.3f;
            yield return null;
        }
        rootGroup.alpha = 0f;

        if (string.IsNullOrEmpty(nextSceneName)) canvasRoot.SetActive(false);
        else SceneManager.LoadScene(nextSceneName);
    }

    // ------------------------------------------------------------------ isi konten (juga dipakai preview di editor)

    /// <summary>Warna, teks tetap, logo, tombol. Dipanggil saat Play dan saat Refresh Preview.</summary>
    public void ApplyStatic()
    {
        if (!HasUI) return;

        backgroundImage.color = backgroundColor;

        // Logo (atau fallback teks)
        bool hasLogo = logoSprite != null;
        logoImage.sprite = logoSprite;
        logoImage.enabled = hasLogo;
        brandText.gameObject.SetActive(!hasLogo);
        taglineText.gameObject.SetActive(!hasLogo);
        brandText.text = appName;
        brandText.color = brandColor;
        taglineText.text = tagline;
        taglineText.color = brandColor;

        titleText.color = titleColor;
        subtitleText.color = subtitleColor;

        // Tombol Mulai
        var btnImg = startButton.targetGraphic as Image;
        if (btnImg) btnImg.color = buttonColor;
        startText.text = startLabel;
        startText.color = buttonTextColor;
        arrowImage.sprite = arrowSprite != null ? arrowSprite : RuntimeArrow();
        arrowImage.color = buttonTextColor;
        arrowImage.gameObject.SetActive(showArrow);

        // Tombol Lewati
        skipText.text = skipLabel;
        skipText.color = subtitleColor;
        skipButton.gameObject.SetActive(showSkip);

        // Dots
        if (dotsRoot) dotsRoot.gameObject.SetActive(showDots && pages != null && pages.Length > 1);

        startText.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonRow);
    }

    /// <summary>Tampilkan halaman tertentu (dipakai slider preview di Inspector).</summary>
    public void PreviewPage(int index)
    {
        if (!HasUI || pages == null || pages.Length == 0) return;
        if (dots == null || dots.Length != pages.Length) BuildDots();
        ApplyStatic();
        ApplyPage(Mathf.Clamp(index, 0, pages.Length - 1));
    }

    string FormatAccent(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        string hex = ColorUtility.ToHtmlStringRGB(accentColor);
        return s.Replace("<accent>", "<color=#" + hex + ">").Replace("</accent>", "</color>");
    }

    void ApplyPage(int index)
    {
        current = index;
        var p = pages[index];
        titleText.text = FormatAccent(p.title);
        subtitleText.text = FormatAccent(p.subtitle);
        illustration.sprite = p.illustration;
        illustration.enabled = p.illustration != null;

        for (int i = 0; i < dots.Length; i++)
        {
            bool active = i == index;
            dots[i].color = active ? dotActiveColor : dotInactiveColor;
            dots[i].rectTransform.sizeDelta = active ? new Vector2(64, 20) : new Vector2(20, 20);
        }

        if (Application.isPlaying) onPageChanged?.Invoke(index);
    }

    // ------------------------------------------------------------------ pembuatan UI

    /// <summary>Membuat (atau membuat ulang) seluruh UI sebagai GameObject biasa.</summary>
    [ContextMenu("Build / Rebuild UI")]
    public void BuildUI()
    {
        if (canvasRoot != null) SafeDestroy(canvasRoot);

        // ---- Canvas
        var canvasGO = NewGO("WelcomeCanvas", transform, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasRoot = canvasGO;
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        rootGroup = canvasGO.GetComponent<CanvasGroup>();

        // ---- Background + area swipe
        backgroundImage = NewImage("Background", canvasGO.transform, backgroundColor);
        Stretch(backgroundImage.rectTransform);
        swipeArea = backgroundImage.gameObject.AddComponent<WelcomeSwipeArea>();

        // ---- Logo (atas)
        logoImage = NewImage("Logo", canvasGO.transform, Color.white);
        Place(logoImage.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -375), new Vector2(560, 460));
        logoImage.preserveAspect = true;
        logoImage.raycastTarget = false;

        // Fallback bila logo belum diisi
        brandText = NewText("BrandFallback", canvasGO.transform, appName, 120, brandColor, FontStyles.Bold);
        Place(brandText.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -340), new Vector2(900, 160));
        taglineText = NewText("TaglineFallback", canvasGO.transform, tagline, 38, brandColor, FontStyles.Normal);
        taglineText.characterSpacing = 8f;
        Place(taglineText.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -450), new Vector2(900, 60));

        // ---- Content (ilustrasi + judul + subjudul) -> dianimasikan
        var contentGO = NewGO("Content", canvasGO.transform, typeof(RectTransform), typeof(CanvasGroup));
        contentRT = (RectTransform)contentGO.transform;
        Stretch(contentRT);
        contentGroup = contentGO.GetComponent<CanvasGroup>();
        contentGroup.blocksRaycasts = false;

        illustration = NewImage("Illustration", contentGO.transform, Color.white);
        Place(illustration.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 92), new Vector2(660, 580));
        illustration.preserveAspect = true;
        illustration.raycastTarget = false;

        titleText = NewText("Title", contentGO.transform, "", 84, titleColor, FontStyles.Bold);
        Place(titleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -265), new Vector2(940, 220));

        subtitleText = NewText("Subtitle", contentGO.transform, "", 44, subtitleColor, FontStyles.Normal);
        Place(subtitleText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -441), new Vector2(940, 130));

        // ---- Dots (muncul jika halaman > 1)
        var dotsGO = NewGO("Dots", canvasGO.transform, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        dotsRoot = dotsGO.transform;
        Place((RectTransform)dotsRoot, new Vector2(0.5f, 0), new Vector2(0, 150), new Vector2(400, 24));
        var dl = dotsGO.GetComponent<HorizontalLayoutGroup>();
        dl.spacing = 14;
        dl.childAlignment = TextAnchor.MiddleCenter;
        dl.childControlWidth = false;
        dl.childControlHeight = false;
        dl.childForceExpandWidth = false;
        dl.childForceExpandHeight = false;
        BuildDots();

        // ---- Tombol Mulai (pill hijau + panah)
        var btnImg = NewImage("StartButton", canvasGO.transform, buttonColor);
        Place(btnImg.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 280), new Vector2(520, 130));
        btnImg.sprite = Rounded();
        btnImg.type = Image.Type.Sliced;
        btnImg.pixelsPerUnitMultiplier = 32f / 65f;
        startButton = btnImg.gameObject.AddComponent<Button>();
        startButton.targetGraphic = btnImg;
        var colors = startButton.colors;
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.highlightedColor = new Color(0.94f, 0.94f, 0.94f, 1f);
        startButton.colors = colors;

        var rowGO = NewGO("Row", btnImg.transform, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        buttonRow = (RectTransform)rowGO.transform;
        Stretch(buttonRow);
        var rl = rowGO.GetComponent<HorizontalLayoutGroup>();
        rl.spacing = 18;
        rl.childAlignment = TextAnchor.MiddleCenter;
        rl.childControlWidth = true;
        rl.childControlHeight = false;
        rl.childForceExpandWidth = false;
        rl.childForceExpandHeight = false;

        startText = NewText("Label", rowGO.transform, startLabel, 46, buttonTextColor, FontStyles.Bold);
        startText.rectTransform.sizeDelta = new Vector2(200, 100);

        arrowImage = NewImage("Arrow", rowGO.transform, buttonTextColor);
        arrowImage.rectTransform.sizeDelta = new Vector2(48, 48);
        arrowImage.preserveAspect = true;
        arrowImage.raycastTarget = false;
        var le = arrowImage.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = 48;
        le.preferredHeight = 48;

        // ---- Tombol Lewati (opsional)
        var skipImg = NewImage("SkipButton", canvasGO.transform, new Color(0, 0, 0, 0));
        Place(skipImg.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 70), new Vector2(320, 80));
        skipButton = skipImg.gameObject.AddComponent<Button>();
        skipButton.targetGraphic = skipImg;
        skipText = NewText("Label", skipImg.transform, skipLabel, 40, subtitleColor, FontStyles.Normal);
        Stretch(skipText.rectTransform);

        ApplyStatic();
        ApplyPage(0);
    }

    void BuildDots()
    {
        if (dotsRoot == null) return;
        for (int i = dotsRoot.childCount - 1; i >= 0; i--)
        {
            var child = dotsRoot.GetChild(i).gameObject;
            child.transform.SetParent(null);
            SafeDestroy(child);
        }

        dots = new Image[pages.Length];
        for (int i = 0; i < pages.Length; i++)
        {
            var d = NewImage("Dot" + i, dotsRoot, dotInactiveColor);
            d.sprite = Rounded();
            d.type = Image.Type.Sliced;
            d.pixelsPerUnitMultiplier = 32f / 10f;
            d.raycastTarget = false;
            dots[i] = d;
        }
    }

    // ------------------------------------------------------------------ helper

    public static void EnsureEventSystem()
    {
#if UNITY_2023_1_OR_NEWER
        var es = FindFirstObjectByType<EventSystem>();
#else
        var es = FindObjectOfType<EventSystem>();
#endif
        if (es != null) return;

        var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        go.AddComponent<InputSystemUIInputModule>();
#else
        go.AddComponent<StandaloneInputModule>();
#endif
    }

    static void SafeDestroy(GameObject go)
    {
        if (Application.isPlaying) Destroy(go);
        else DestroyImmediate(go);
    }

    static GameObject NewGO(string name, Transform parent, params System.Type[] components)
    {
        var go = new GameObject(name, components);
        go.layer = UILayer;
        go.transform.SetParent(parent, false);
        return go;
    }

    static Image NewImage(string name, Transform parent, Color color)
    {
        var go = NewGO(name, parent, typeof(RectTransform), typeof(Image));
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    static TextMeshProUGUI NewText(string name, Transform parent, string text, float size, Color color, FontStyles style)
    {
        var go = NewGO(name, parent, typeof(RectTransform), typeof(TextMeshProUGUI));
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.fontStyle = style;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        return t;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    // ------------------------------------------------------------------ sprite buatan kode

    /// <summary>Texture 64x64 rounded-rectangle (dipakai editor untuk membuat aset PNG).</summary>
    public static Texture2D CreateRoundedTexture()
    {
        const int size = 64, r = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(r - x - 0.5f, 0, x + 0.5f - (size - r));
                float dy = Mathf.Max(r - y - 0.5f, 0, y + 0.5f - (size - r));
                float a = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        }
        tex.Apply();
        return tex;
    }

    /// <summary>Texture ikon panah kanan "→" (putih, background transparan).</summary>
    public static Texture2D CreateArrowTexture()
    {
        const int s = 96;
        const float halfWidth = 5.5f;
        var a = new Vector2(14, 48);
        var b = new Vector2(80, 48);
        var h1 = new Vector2(50, 18);
        var h2 = new Vector2(50, 78);

        var tex = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float d = Mathf.Min(DistToSegment(p, a, b),
                          Mathf.Min(DistToSegment(p, b, h1), DistToSegment(p, b, h2)));
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(halfWidth - d + 0.5f)));
            }
        }
        tex.Apply();
        return tex;
    }

    static float DistToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude);
        return (p - (a + ab * t)).magnitude;
    }

    Sprite Rounded()
    {
        if (roundedSprite != null) return roundedSprite;
        if (runtimeRounded == null)
        {
            var tex = CreateRoundedTexture();
            tex.hideFlags = HideFlags.DontSave;
            runtimeRounded = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100f, 0,
                SpriteMeshType.FullRect, new Vector4(32, 32, 32, 32));
            runtimeRounded.hideFlags = HideFlags.DontSave;
        }
        return runtimeRounded;
    }

    static Sprite RuntimeArrow()
    {
        if (runtimeArrow == null)
        {
            var tex = CreateArrowTexture();
            tex.hideFlags = HideFlags.DontSave;
            runtimeArrow = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            runtimeArrow.hideFlags = HideFlags.DontSave;
        }
        return runtimeArrow;
    }
}
