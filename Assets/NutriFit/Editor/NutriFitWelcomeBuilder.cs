#if UNITY_EDITOR
using NutriFit;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Menu: NutriFit > Build Welcome Page
/// Membangun seluruh UI Welcome Page NutriFit di scene yang sedang terbuka.
/// </summary>
public static class NutriFitWelcomeBuilder
{
    private const string SpriteDir = "Assets/NutriFit/Resources/NutriFit/";

    // Warna (diambil dari desain)
    private const string ColBackground = "#F7FBF5";
    private const string ColBlob = "#DDEED8";
    private const string ColDark = "#0B4D3B";
    private const string ColLight = "#6AB04C";
    private const string ColTagline = "#14503F";
    private const string ColDesc = "#6B7280";
    private const string ColButton = "#2F8F5B";
    private const string ColDotActive = "#1FA67A";
    private const string ColDotIdle = "#DDE3DD";

    [MenuItem("NutriFit/Build Welcome Page")]
    public static void Build()
    {
        Sprite rounded = LoadSprite("rounded.png", new Vector4(63, 63, 63, 63));
        Sprite circle = LoadSprite("circle.png", Vector4.zero);
        Sprite logo = LoadSprite("logo_leaf.png", Vector4.zero);
        Sprite illus = LoadSprite("illustration.png", Vector4.zero);
        Sprite arrow = LoadSprite("arrow.png", Vector4.zero);

        if (rounded == null || circle == null || logo == null || illus == null || arrow == null)
        {
            EditorUtility.DisplayDialog("NutriFit",
                "Sprite tidak ditemukan di " + SpriteDir + "\nPastikan folder Assets/NutriFit ikut ter-copy utuh.", "OK");
            return;
        }

        Font font = GetDefaultFont();

        // Hapus hasil build sebelumnya agar tidak dobel
        GameObject old = GameObject.Find("NutriFit_Canvas");
        if (old != null) Undo.DestroyObjectImmediate(old);

        // ---------- Canvas ----------
        var canvasGO = new GameObject("NutriFit_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasGO, "Build NutriFit Welcome Page");
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // ---------- Background ----------
        var bgRoot = NewRect("Background", canvasGO.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        bgRoot.offsetMin = Vector2.zero; bgRoot.offsetMax = Vector2.zero;
        var bg = bgRoot.gameObject.AddComponent<Image>();
        bg.color = Hex(ColBackground);
        bg.raycastTarget = false;

        // Blob kiri atas & kanan bawah
        NewImage("Blob_TopLeft", bgRoot, circle, Hex(ColBlob), new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector2(-120, -40), new Vector2(760, 760));
        NewImage("Blob_BottomRight", bgRoot, circle, Hex(ColBlob), new Vector2(1, 0), new Vector2(0.5f, 0.5f), new Vector2(260, -180), new Vector2(1000, 1000));

        // Daun dekoratif kiri bawah
        var leafDeco = NewImage("Leaf_Deco", bgRoot, logo, new Color(1, 1, 1, 0.55f), new Vector2(0, 0), new Vector2(0.5f, 0.5f), new Vector2(110, 110), new Vector2(300, 238));
        leafDeco.rectTransform.localRotation = Quaternion.Euler(0, 0, 18);

        // ---------- Content (di-fade in) ----------
        var content = NewRect("Content", canvasGO.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        content.offsetMin = Vector2.zero; content.offsetMax = Vector2.zero;
        var group = content.gameObject.AddComponent<CanvasGroup>();

        // Logo daun
        NewImage("Logo_Leaf", content, logo, Color.white, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -170), new Vector2(190, 151));

        // Wordmark "NutriFit"
        var word = NewText("Wordmark", content, font, "<color=" + ColDark + ">Nutri</color><color=" + ColLight + ">Fit</color>",
            150, FontStyle.Bold, Color.white, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -340), new Vector2(900, 200));

        // Tagline
        NewText("Tagline", content, font, "Hidup Sehat, Mulai dari Sekarang",
            50, FontStyle.Italic, Hex(ColTagline), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -560), new Vector2(960, 90));

        // Deskripsi
        NewText("Description", content, font, "Dapatkan pola makan yang tepat,\nraih tubuh yang lebih sehat dan bertenaga.",
            38, FontStyle.Normal, Hex(ColDesc), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -665), new Vector2(960, 130));

        // Ilustrasi
        var illusImg = NewImage("Illustration", content, illus, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -200), new Vector2(860, 773));

        // Tombol "Mulai Sekarang"
        var btnImg = NewImage("Button_Mulai", content, rounded, Hex(ColButton), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(0, 190), new Vector2(840, 150), Image.Type.Sliced);
        btnImg.raycastTarget = true;
        var btn = btnImg.gameObject.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        NewText("Label", btnImg.rectTransform, font, "Mulai Sekarang", 54, FontStyle.Normal, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-45, 0), new Vector2(520, 100));
        NewImage("Arrow", btnImg.rectTransform, arrow, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(235, 0), new Vector2(66, 44));

        // Indikator halaman (3 titik)
        var dots = NewRect("PageDots", content, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(0, 85), new Vector2(200, 40));
        NewImage("Dot_1", dots, circle, Hex(ColDotActive), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-50, 0), new Vector2(28, 28));
        NewImage("Dot_2", dots, circle, Hex(ColDotIdle), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(24, 24));
        NewImage("Dot_3", dots, circle, Hex(ColDotIdle), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(50, 0), new Vector2(24, 24));

        // ---------- Controller ----------
        var controller = canvasGO.AddComponent<WelcomeController>();
        controller.contentGroup = group;
        controller.illustration = illusImg.rectTransform;
        UnityEventTools.AddPersistentListener(btn.onClick, controller.OnStartClicked);

        // ---------- EventSystem (agar tombol bisa diklik) ----------
        if (FindEventSystem() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
            Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
        }

        // Atur kamera agar tampilan Game view portrait
        Selection.activeGameObject = canvasGO;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[NutriFit] Welcome Page berhasil dibuat. Atur Game view ke 1080x1920 (Portrait), lalu tekan Play.");
    }

    // ---------------- Helpers ----------------

    private static Object FindEventSystem()
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<EventSystem>();
#else
        return Object.FindObjectOfType<EventSystem>();
#endif
    }

    private static Font GetDefaultFont()
    {
#if UNITY_2022_2_OR_NEWER
        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
    }

    private static Color Hex(string hex)
    {
        Color c;
        return ColorUtility.TryParseHtmlString(hex, out c) ? c : Color.magenta;
    }

    private static Sprite LoadSprite(string file, Vector4 border)
    {
        string path = SpriteDir + file;
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return null;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = border;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static RectTransform NewRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 pivot, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    private static Image NewImage(string name, Transform parent, Sprite sprite, Color color, Vector2 anchor,
        Vector2 pivot, Vector2 pos, Vector2 size, Image.Type type = Image.Type.Simple)
    {
        var rt = NewRect(name, parent, anchor, anchor, pivot, pos, size);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        img.type = type;
        img.raycastTarget = false;
        return img;
    }

    private static Text NewText(string name, Transform parent, Font font, string text, int size, FontStyle style,
        Color color, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 boxSize)
    {
        var rt = NewRect(name, parent, anchor, anchor, pivot, pos, boxSize);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.fontStyle = style;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.supportRichText = true;
        t.raycastTarget = false;
        return t;
    }
}
#endif
