using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NutriFit
{
    /// <summary>
    /// Cara tercepat tanpa menu: taruh script ini di GameObject kosong, lalu tekan Play.
    /// UI Welcome Page NutriFit dibuat otomatis saat game berjalan.
    /// </summary>
    public class NutriFitWelcomeRuntime : MonoBehaviour
    {
        [Tooltip("Nama scene tujuan saat tombol ditekan (opsional).")]
        public string nextSceneName = "";

        private const string ColBackground = "#F7FBF5";
        private const string ColBlob = "#DDEED8";
        private const string ColDark = "#0B4D3B";
        private const string ColLight = "#6AB04C";
        private const string ColTagline = "#14503F";
        private const string ColDesc = "#6B7280";
        private const string ColButton = "#2F8F5B";
        private const string ColDotActive = "#1FA67A";
        private const string ColDotIdle = "#DDE3DD";

        private void Awake()
        {
            Sprite rounded = GetSprite("rounded", new Vector4(63, 63, 63, 63));
            Sprite circle = GetSprite("circle", Vector4.zero);
            Sprite logo = GetSprite("logo_leaf", Vector4.zero);
            Sprite illus = GetSprite("illustration", Vector4.zero);
            Sprite arrow = GetSprite("arrow", Vector4.zero);
            Font font = GetFont();

            // Canvas
            var canvasGO = new GameObject("NutriFit_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Background
            var bgRoot = NewRect("Background", canvasGO.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            bgRoot.offsetMin = Vector2.zero; bgRoot.offsetMax = Vector2.zero;
            var bg = bgRoot.gameObject.AddComponent<Image>();
            bg.color = Hex(ColBackground);
            bg.raycastTarget = false;

            NewImage("Blob_TopLeft", bgRoot, circle, Hex(ColBlob), new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector2(-120, -40), new Vector2(760, 760));
            NewImage("Blob_BottomRight", bgRoot, circle, Hex(ColBlob), new Vector2(1, 0), new Vector2(0.5f, 0.5f), new Vector2(260, -180), new Vector2(1000, 1000));
            var leafDeco = NewImage("Leaf_Deco", bgRoot, logo, new Color(1, 1, 1, 0.55f), new Vector2(0, 0), new Vector2(0.5f, 0.5f), new Vector2(110, 110), new Vector2(300, 238));
            leafDeco.rectTransform.localRotation = Quaternion.Euler(0, 0, 18);

            // Content
            var content = NewRect("Content", canvasGO.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            content.offsetMin = Vector2.zero; content.offsetMax = Vector2.zero;
            var group = content.gameObject.AddComponent<CanvasGroup>();

            NewImage("Logo_Leaf", content, logo, Color.white, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -170), new Vector2(190, 151));
            NewText("Wordmark", content, font, "<color=" + ColDark + ">Nutri</color><color=" + ColLight + ">Fit</color>",
                150, FontStyle.Bold, Color.white, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -340), new Vector2(900, 200));
            NewText("Tagline", content, font, "Hidup Sehat, Mulai dari Sekarang",
                50, FontStyle.Italic, Hex(ColTagline), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -560), new Vector2(960, 90));
            NewText("Description", content, font, "Dapatkan pola makan yang tepat,\nraih tubuh yang lebih sehat dan bertenaga.",
                38, FontStyle.Normal, Hex(ColDesc), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -665), new Vector2(960, 130));

            var illusImg = NewImage("Illustration", content, illus, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -200), new Vector2(860, 773));

            var btnImg = NewImage("Button_Mulai", content, rounded, Hex(ColButton), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(0, 190), new Vector2(840, 150), Image.Type.Sliced);
            btnImg.raycastTarget = true;
            var btn = btnImg.gameObject.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            NewText("Label", btnImg.rectTransform, font, "Mulai Sekarang", 54, FontStyle.Normal, Color.white,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-45, 0), new Vector2(520, 100));
            NewImage("Arrow", btnImg.rectTransform, arrow, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(235, 0), new Vector2(66, 44));

            var dots = NewRect("PageDots", content, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0.5f), new Vector2(0, 85), new Vector2(200, 40));
            NewImage("Dot_1", dots, circle, Hex(ColDotActive), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-50, 0), new Vector2(28, 28));
            NewImage("Dot_2", dots, circle, Hex(ColDotIdle), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(24, 24));
            NewImage("Dot_3", dots, circle, Hex(ColDotIdle), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(50, 0), new Vector2(24, 24));

            // Controller (fade-in, animasi, tombol)
            var controller = canvasGO.AddComponent<WelcomeController>();
            controller.nextSceneName = nextSceneName;
            controller.contentGroup = group;
            controller.illustration = illusImg.rectTransform;
            btn.onClick.AddListener(controller.OnStartClicked);

            // EventSystem
#if UNITY_2023_1_OR_NEWER
            bool hasEs = FindFirstObjectByType<EventSystem>() != null;
#else
            bool hasEs = FindObjectOfType<EventSystem>() != null;
#endif
            if (!hasEs)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
                es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                es.AddComponent<StandaloneInputModule>();
#endif
            }
        }

        // ---------- Helpers ----------

        private static Sprite GetSprite(string name, Vector4 border)
        {
            string path = "NutriFit/" + name;
            Sprite s = Resources.Load<Sprite>(path);
            if (s != null) return s;

            Texture2D tex = Resources.Load<Texture2D>(path);
            if (tex == null)
            {
                Debug.LogError("[NutriFit] Gambar tidak ditemukan: Resources/" + path + ". Pastikan folder Assets/NutriFit/Resources/NutriFit ada.");
                return null;
            }
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        private static Font GetFont()
        {
            Font f = null;
#if UNITY_2022_2_OR_NEWER
            f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            f = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
            if (f == null) f = Font.CreateDynamicFontFromOSFont("Arial", 16);
            return f;
        }

        private static Color Hex(string hex)
        {
            Color c;
            return ColorUtility.TryParseHtmlString(hex, out c) ? c : Color.magenta;
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
}
