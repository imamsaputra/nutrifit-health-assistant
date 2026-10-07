#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace NutriFitTools
{
    public static class WelcomeScreenTools
    {
        private static Color Green => Hex("#168A4B");
        private static Color LightGreen => Hex("#E7F4E5");
        private static Color DarkGreen => Hex("#174D34");
        private static Color Body => Hex("#4B5563");

        [MenuItem("Tools/NutriFit/Create Welcome Screen", priority = 1)]
        public static void CreateWelcomeScreen()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Keluar dari Play Mode terlebih dahulu.");
                return;
            }

            EnsureEventSystem();

            // Bersihkan objek welcome lama agar tidak terjadi duplikasi.
            GameObject old = GameObject.Find("NutriFit_WelcomeCanvas");
            if (old != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "NutriFit Welcome",
                    "Welcome Screen sudah ada di scene. Hapus dan buat ulang?",
                    "Buat Ulang", "Batal"))
                    return;

                Undo.DestroyObjectImmediate(old);
            }

            Canvas canvas = CreateCanvas();
            canvas.name = "NutriFit_WelcomeCanvas";

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(432, 932);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject root = UI("WelcomeRoot", canvas.transform);
            Stretch(root.GetComponent<RectTransform>());
            Image background = root.AddComponent<Image>();
            background.color = Hex("#F8FBF7");

            // Dekorasi background.
            AddPanel("TopLeftShape", root.transform,
                new Vector2(0.5f, 0.5f), new Vector2(340, 340),
                new Vector2(-205, 395), LightGreen);
            AddPanel("BottomRightShape", root.transform,
                new Vector2(0.5f, 0.5f), new Vector2(370, 370),
                new Vector2(205, -405), LightGreen);

            GameObject content = UI("Content", root.transform);
            RectTransform cr = content.GetComponent<RectTransform>();
            cr.anchorMin = new Vector2(0.06f, 0.025f);
            cr.anchorMax = new Vector2(0.94f, 0.975f);
            cr.offsetMin = cr.offsetMax = Vector2.zero;

            // Placeholder logo.
            AddPanel("Logo_Placeholder", content.transform,
                new Vector2(0.5f, 0.855f), new Vector2(180, 110),
                Vector2.zero, Hex("#D5EACB"));

            AddText("Brand", content.transform, "NutriFit", 42, FontStyle.Bold,
                DarkGreen, new Vector2(0.5f, 0.755f), new Vector2(380, 60));

            AddText("Tagline", content.transform, "Health Assistants", 21, FontStyle.Normal,
                DarkGreen, new Vector2(0.5f, 0.705f), new Vector2(380, 38));

            // Placeholder ilustrasi.
            AddPanel("Illustration_Placeholder", content.transform,
                new Vector2(0.5f, 0.49f), new Vector2(350, 320),
                Vector2.zero, Hex("#E8F5E2"));

            AddText("Headline", content.transform,
                "Mulai Hidup Sehat,\nMulai dari Sekarang.",
                29, FontStyle.Bold, DarkGreen,
                new Vector2(0.5f, 0.285f), new Vector2(405, 95));

            AddText("Description", content.transform,
                "Atur pola makan, pantau kebiasaan sehat,\ndan capai tujuan kesehatanmu\nbersama NutriFit.",
                17, FontStyle.Normal, Body,
                new Vector2(0.5f, 0.198f), new Vector2(405, 85));

            Button start = AddButton("StartButton", content.transform,
                "MULAI SEKARANG     →", Green,
                new Vector2(0.5f, 0.108f), new Vector2(345, 65), Color.white);

            Button login = AddButton("LoginButton", content.transform,
                "Sudah punya akun?  Masuk", Color.clear,
                new Vector2(0.5f, 0.035f), new Vector2(320, 45), Green);

            GameObject controller = new GameObject("WelcomeScreenController");
            controller.transform.SetParent(root.transform, false);
            WelcomeScreenController script = controller.AddComponent<WelcomeScreenController>();

            SerializedObject so = new SerializedObject(script);
            so.FindProperty("startButton").objectReferenceValue = start;
            so.FindProperty("loginButton").objectReferenceValue = login;
            so.FindProperty("nextSceneName").stringValue = "Onboarding";
            so.FindProperty("loginSceneName").stringValue = "Login";
            so.ApplyModifiedPropertiesWithoutUndo();

            Selection.activeGameObject = controller;
            EditorGUIUtility.PingObject(controller);

            Debug.Log("NutriFit Welcome Screen berhasil dibuat melalui Tools > NutriFit > Create Welcome Screen.");
        }

        [MenuItem("Tools/NutriFit/Open Welcome Scene Folder")]
        public static void OpenWelcomeFolder()
        {
            string folder = "Assets";
            Object obj = AssetDatabase.LoadAssetAtPath<Object>(folder);
            Selection.activeObject = obj;
            EditorGUIUtility.PingObject(obj);
        }

        private static Canvas CreateCanvas()
        {
            GameObject go = new GameObject(
                "NutriFit_WelcomeCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            return canvas;
        }

        private static GameObject UI(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Stretch(RectTransform r)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
        }

        private static Image AddPanel(string name, Transform parent, Vector2 anchor,
            Vector2 size, Vector2 position, Color color)
        {
            GameObject go = UI(name, parent);
            RectTransform r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.sizeDelta = size;
            r.anchoredPosition = position;

            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text AddText(string name, Transform parent, string value,
            int fontSize, FontStyle style, Color color,
            Vector2 anchor, Vector2 size)
        {
            GameObject go = UI(name, parent);
            RectTransform r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.sizeDelta = size;

            Text text = go.AddComponent<Text>();
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Button AddButton(string name, Transform parent, string label,
            Color background, Vector2 anchor, Vector2 size, Color textColor)
        {
            GameObject go = UI(name, parent);
            RectTransform r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.sizeDelta = size;

            Image image = go.AddComponent<Image>();
            image.color = background;

            Button button = go.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = background;
            colors.highlightedColor = background == Color.clear
                ? Color.clear : Color.Lerp(background, Color.white, 0.12f);
            colors.pressedColor = background == Color.clear
                ? Color.clear : Color.Lerp(background, Color.black, 0.10f);
            colors.selectedColor = background;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            AddText("Label", go.transform, label, 18, FontStyle.Bold,
                textColor, new Vector2(0.5f, 0.5f), size - new Vector2(15, 8));

            return button;
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem != null) return;

            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(go, "Create NutriFit EventSystem");
        }

        private static Color Hex(string html)
        {
            if (ColorUtility.TryParseHtmlString(html, out Color color))
                return color;

            return Color.white;
        }
    }
}
#endif
