#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using NutriFit;

namespace NutriFit.EditorTools
{
    public static class NutriFitWelcomeBuilder
    {
        private static readonly Color Green = Hex("#168A4B");
        private static readonly Color LightGreen = Hex("#E7F4E5");
        private static readonly Color DarkGreen = Hex("#174D34");
        private static readonly Color Body = Hex("#4B5563");

        [MenuItem("NutriFit/Create Welcome Screen")]
        public static void CreateWelcomeScreen()
        {
            EnsureEventSystem();

            Canvas canvas = NewCanvas("NutriFit_WelcomeCanvas");
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(432, 932);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject root = UI("WelcomeRoot", canvas.transform);
            Stretch(root.GetComponent<RectTransform>());
            Image bg = root.AddComponent<Image>();
            bg.color = Hex("#F8FBF7");

            // Decorative soft shapes.
            Circle("TopLeftDecoration", root.transform, new Vector2(-205, 395), new Vector2(330, 330), LightGreen);
            Circle("BottomRightDecoration", root.transform, new Vector2(205, -405), new Vector2(370, 370), LightGreen);

            GameObject content = UI("Content", root.transform);
            RectTransform c = content.GetComponent<RectTransform>();
            c.anchorMin = new Vector2(0.07f, 0.025f);
            c.anchorMax = new Vector2(0.93f, 0.975f);
            c.offsetMin = c.offsetMax = Vector2.zero;

            Image logo = Placeholder("Logo_Placeholder", content.transform,
                new Vector2(0.5f, 0.855f), new Vector2(180, 115), Hex("#D5EACB"));

            Text("Brand", content.transform, "NutriFit", 43, FontStyles.Bold, DarkGreen,
                new Vector2(0.5f, 0.755f), new Vector2(380, 65));
            Text("Tagline", content.transform, "Health Assistants", 22, FontStyles.Normal, DarkGreen,
                new Vector2(0.5f, 0.705f), new Vector2(380, 40));

            Image illustration = Placeholder("Illustration_Placeholder", content.transform,
                new Vector2(0.5f, 0.485f), new Vector2(350, 330), Hex("#E8F5E2"));

            Text("Headline", content.transform,
                "Mulai Hidup Sehat,\nMulai dari Sekarang.", 30, FontStyles.Bold, DarkGreen,
                new Vector2(0.5f, 0.285f), new Vector2(400, 100));

            Text("Description", content.transform,
                "Atur pola makan, pantau kebiasaan sehat,\ndan capai tujuan kesehatanmu\nbersama NutriFit.",
                17, FontStyles.Normal, Body,
                new Vector2(0.5f, 0.195f), new Vector2(405, 90));

            Button start = MakeButton("StartButton", content.transform,
                "MULAI SEKARANG     →", Green,
                new Vector2(0.5f, 0.105f), new Vector2(345, 66), Color.white);

            Button login = MakeButton("LoginButton", content.transform,
                "Sudah punya akun?  Masuk", Color.clear,
                new Vector2(0.5f, 0.035f), new Vector2(320, 45), Green);

            GameObject controllerGO = new GameObject("WelcomeScreenController");
            controllerGO.transform.SetParent(root.transform, false);
            WelcomeScreenController controller = controllerGO.AddComponent<WelcomeScreenController>();

            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("startButton").objectReferenceValue = start;
            so.FindProperty("loginButton").objectReferenceValue = login;
            so.FindProperty("nextSceneName").stringValue = "Onboarding";
            so.FindProperty("loginSceneName").stringValue = "Login";
            so.ApplyModifiedPropertiesWithoutUndo();

            string path = EditorUtility.SaveFilePanelInProject(
                "Save NutriFit Welcome Scene", "Welcome", "unity",
                "Pilih lokasi untuk scene Welcome.");

            if (!string.IsNullOrEmpty(path))
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), path);

            Selection.activeGameObject = controllerGO;
            Debug.Log("NutriFit Welcome Screen dibuat. Ganti Logo_Placeholder dan Illustration_Placeholder dengan PNG asli.");
        }

        private static Canvas NewCanvas(string name)
        {
            GameObject go = new GameObject(name);
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<CanvasScaler>();
            go.AddComponent<GraphicRaycaster>();
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

        private static Image Placeholder(string name, Transform parent, Vector2 anchor, Vector2 size, Color color)
        {
            GameObject go = UI(name, parent);
            RectTransform r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.sizeDelta = size;
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void Circle(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            GameObject go = UI(name, parent);
            RectTransform r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.sizeDelta = size;
            r.anchoredPosition = pos;
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
        }

        private static TMP_Text Text(string name, Transform parent, string value, float size,
            FontStyles style, Color color, Vector2 anchor, Vector2 dimensions)
        {
            GameObject go = UI(name, parent);
            RectTransform r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.sizeDelta = dimensions;

            TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
            t.text = value;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.enableWordWrapping = true;
            t.raycastTarget = false;
            return t;
        }

        private static Button MakeButton(string name, Transform parent, string label, Color bg,
            Vector2 anchor, Vector2 size, Color textColor)
        {
            GameObject go = UI(name, parent);
            RectTransform r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.sizeDelta = size;

            Image image = go.AddComponent<Image>();
            image.color = bg;

            Button button = go.AddComponent<Button>();
            ColorBlock cb = button.colors;
            cb.normalColor = bg;
            cb.highlightedColor = bg == Color.clear ? Color.clear : Color.Lerp(bg, Color.white, 0.12f);
            cb.pressedColor = bg == Color.clear ? Color.clear : Color.Lerp(bg, Color.black, 0.10f);
            cb.selectedColor = bg;
            cb.fadeDuration = 0.08f;
            button.colors = cb;

            Text("Label", go.transform, label, 18, FontStyles.Bold, textColor,
                new Vector2(0.5f, 0.5f), size - new Vector2(15, 8));

            return button;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject go = new GameObject("EventSystem");
                go.AddComponent<EventSystem>();
                go.AddComponent<StandaloneInputModule>();
            }
        }

        private static Color Hex(string html)
        {
            ColorUtility.TryParseHtmlString(html, out Color color);
            return color;
        }
    }
}
#endif
