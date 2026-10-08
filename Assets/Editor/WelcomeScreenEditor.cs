#if UNITY_EDITOR
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Letakkan di folder Assets/Editor (nama folder harus persis "Editor").
/// Menu: Tools > NutriFit > Create Welcome Screen  -> UI langsung muncul di Hierarchy & Scene View.
/// </summary>
public static class WelcomeScreenMenu
{
    const string AssetDir = "Assets/NutriFit/UI";
    const string RoundedPath = AssetDir + "/RoundedRect.png";
    const string ArrowPath = AssetDir + "/ArrowIcon.png";
    public const string LogoName = "NutriFit_Logo";
    public const string IllustrationName = "NutriFit_Illustration";

    static WelcomeScreen FindWS()
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<WelcomeScreen>();
#else
        return Object.FindObjectOfType<WelcomeScreen>();
#endif
    }

    [MenuItem("Tools/NutriFit/Create Welcome Screen")]
    public static void Create()
    {
        if (!CheckTMP()) return;

        var existing = FindWS();
        if (existing != null)
        {
            Selection.activeObject = existing.gameObject;
            if (EditorUtility.DisplayDialog("Welcome Screen",
                    "Scene ini sudah punya WelcomeScreen. Buat ulang UI-nya?", "Buat ulang", "Batal"))
                Rebuild(existing);
            return;
        }

        var go = new GameObject("WelcomeScreen");
        var ws = go.AddComponent<WelcomeScreen>();
        Undo.RegisterCreatedObjectUndo(go, "Create Welcome Screen");
        Rebuild(ws);
        Selection.activeObject = go;
    }

    [MenuItem("Tools/NutriFit/Reset Welcome Seen")]
    static void ResetSeen()
    {
        PlayerPrefs.DeleteKey(WelcomeScreen.PrefKey);
        PlayerPrefs.Save();
        Debug.Log("[WelcomeScreen] Flag direset. Welcome akan muncul lagi saat Play.");
    }

    public static void Rebuild(WelcomeScreen ws)
    {
        if (!CheckTMP()) return;

        ws.roundedSprite = GetOrCreateSprite(RoundedPath, WelcomeScreen.CreateRoundedTexture(), new Vector4(32, 32, 32, 32));
        ws.arrowSprite = GetOrCreateSprite(ArrowPath, WelcomeScreen.CreateArrowTexture(), Vector4.zero);
        AutoAssignSprites(ws);

        ws.BuildUI();
        WelcomeScreen.EnsureEventSystem();

        EditorUtility.SetDirty(ws);
        EditorSceneManager.MarkSceneDirty(ws.gameObject.scene);

        var canvas = ws.transform.Find("WelcomeCanvas");
        if (canvas != null)
        {
            Selection.activeObject = canvas.gameObject;
            var sv = SceneView.lastActiveSceneView;
            if (sv != null)
            {
                sv.in2DMode = true;
                sv.FrameSelected();
            }
        }
        Debug.Log("[WelcomeScreen] UI dibuat. Lihat di Hierarchy > WelcomeScreen > WelcomeCanvas. " +
                  "Atur Game View ke rasio portrait (mis. 1080x1920) untuk melihat tampilan akhir.");
    }

    /// <summary>Cari NutriFit_Logo & NutriFit_Illustration di project, jadikan Sprite, lalu isi ke komponen jika masih kosong.</summary>
    public static void AutoAssignSprites(WelcomeScreen ws)
    {
        if (ws.logoSprite == null)
            ws.logoSprite = FindSpriteByName(LogoName);

        if (ws.pages != null && ws.pages.Length > 0 && ws.pages[0].illustration == null)
            ws.pages[0].illustration = FindSpriteByName(IllustrationName);

        EditorUtility.SetDirty(ws);
    }

    static Sprite FindSpriteByName(string name)
    {
        foreach (var guid in AssetDatabase.FindAssets(name + " t:Texture2D"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(path) != name) continue;

            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                ApplySpriteSettings(importer);
                importer.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) return sprite;
        }
        return null;
    }

    internal static void ApplySpriteSettings(TextureImporter importer)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
    }

    static bool CheckTMP()
    {
        if (TMP_Settings.instance != null) return true;
        EditorUtility.DisplayDialog("TextMeshPro belum di-import",
            "Buka Window > TextMeshPro > Import TMP Essential Resources, lalu jalankan menu ini lagi.", "OK");
        return false;
    }

    static Sprite GetOrCreateSprite(string path, Texture2D tex, Vector4 border)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null)
        {
            Object.DestroyImmediate(tex);
            return existing;
        }

        Directory.CreateDirectory(AssetDir);
        File.WriteAllBytes(path, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        ApplySpriteSettings(importer);
        importer.spriteBorder = border;
        importer.spritePixelsPerUnit = 100;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}

/// <summary>PNG bernama NutriFit_Logo / NutriFit_Illustration otomatis diimpor sebagai Sprite (2D and UI).</summary>
public class NutriFitTexturePostprocessor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        var name = Path.GetFileNameWithoutExtension(assetPath);
        if (name != WelcomeScreenMenu.LogoName && name != WelcomeScreenMenu.IllustrationName) return;
        WelcomeScreenMenu.ApplySpriteSettings((TextureImporter)assetImporter);
    }
}

[CustomEditor(typeof(WelcomeScreen))]
public class WelcomeScreenInspector : Editor
{
    int previewPage;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var ws = (WelcomeScreen)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Alat", EditorStyles.boldLabel);

        // --- UI ---
        if (!ws.HasUI)
            EditorGUILayout.HelpBox("UI belum dibuat. Klik tombol di bawah (atau UI dibuat otomatis saat Play).", MessageType.Warning);

        if (GUILayout.Button(ws.HasUI ? "Build / Rebuild UI" : "Build UI"))
            WelcomeScreenMenu.Rebuild(ws);

        if (GUILayout.Button("Auto-assign sprite NutriFit_Logo & NutriFit_Illustration"))
        {
            WelcomeScreenMenu.AutoAssignSprites(ws);
            if (ws.HasUI) ws.PreviewPage(previewPage);
            EditorSceneManager.MarkSceneDirty(ws.gameObject.scene);
        }

        // --- Preview ---
        if (ws.HasUI && ws.pages != null && ws.pages.Length > 0)
        {
            previewPage = Mathf.Clamp(previewPage, 0, ws.pages.Length - 1);
            EditorGUI.BeginChangeCheck();
            previewPage = EditorGUILayout.IntSlider("Preview halaman", previewPage, 0, ws.pages.Length - 1);
            bool refresh = GUILayout.Button("Refresh Preview (setelah ganti teks / warna / sprite)");
            if (EditorGUI.EndChangeCheck() || refresh)
            {
                ws.PreviewPage(previewPage);
                EditorSceneManager.MarkSceneDirty(ws.gameObject.scene);
            }
        }

        // --- Pengecekan ---
        EditorGUILayout.Space(6);
        if (ws.logoSprite == null)
            EditorGUILayout.HelpBox("Logo Sprite belum diisi. Sementara dipakai teks appName + tagline.", MessageType.Info);

        if (ws.pages == null || ws.pages.Length == 0)
            EditorGUILayout.HelpBox("Pages kosong. Tambahkan minimal 1 halaman.", MessageType.Error);
        else if (ws.pages.Any(p => p.illustration == null))
            EditorGUILayout.HelpBox("Ada halaman yang belum punya Illustration.", MessageType.Warning);

        if (!string.IsNullOrEmpty(ws.nextSceneName))
        {
            bool inBuild = EditorBuildSettings.scenes.Any(s =>
                Path.GetFileNameWithoutExtension(s.path) == ws.nextSceneName);

            if (!inBuild)
            {
                EditorGUILayout.HelpBox(
                    $"Scene \"{ws.nextSceneName}\" belum ada di Build Settings, tombol tidak akan pindah scene.",
                    MessageType.Warning);

                if (GUILayout.Button("Cari & tambahkan scene ke Build Settings"))
                    AddSceneToBuild(ws.nextSceneName);
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Next Scene Name kosong: setelah selesai, welcome hanya disembunyikan.", MessageType.Info);
        }

        EditorGUILayout.Space(4);
        if (GUILayout.Button("Reset \"Welcome Seen\" (muncul lagi saat Play)"))
        {
            PlayerPrefs.DeleteKey(WelcomeScreen.PrefKey);
            PlayerPrefs.Save();
            Debug.Log("[WelcomeScreen] Flag direset.");
        }
    }

    static void AddSceneToBuild(string sceneName)
    {
        var path = AssetDatabase.FindAssets("t:Scene " + sceneName)
            .Select(AssetDatabase.GUIDToAssetPath)
            .FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == sceneName);

        if (string.IsNullOrEmpty(path))
        {
            EditorUtility.DisplayDialog("Scene tidak ditemukan",
                $"Tidak ada file scene bernama \"{sceneName}\" di project.", "OK");
            return;
        }

        var list = EditorBuildSettings.scenes.ToList();
        list.Add(new EditorBuildSettingsScene(path, true));
        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log($"[WelcomeScreen] Scene ditambahkan ke Build Settings: {path}");
    }
}
#endif
