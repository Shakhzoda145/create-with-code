#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Bygger "Prototype 5" enligt lektion 5.1–5.4. Meny: Tools > Unit 5 > Bygg Prototype 5-scenen
public static class Unit5SceneBuilder
{
    const string Root = "Assets/Samples/ExtralityLab@DSV/0.2.2/CwC-Starting-Assets/Unit 5/";
    const string ScenePath = Root + "Scenes/Prototype 5.unity";
    const string Out = "Assets/Unit5/Generated";

    [MenuItem("Tools/Unit 5/Bygg Prototype 5-scenen")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (GameObject.Find("Game Manager") != null)
        {
            EditorUtility.DisplayDialog("Unit 5", "Scenen har redan en Game Manager. Inget ändrades.", "OK");
            return;
        }

        CwcEditorHelpers.AddTag("DestroyZone");
        CwcEditorHelpers.AddTag("Bad");
        if (!AssetDatabase.IsValidFolder(Out)) AssetDatabase.CreateFolder("Assets/Unit5", "Generated");

        // Sensorn under spelplanen (5.1)
        GameObject.Find("Sensor").tag = "DestroyZone";

        // Fyra mål-prefabs (5.1 + 5.2): tre bra, ett dåligt
        var targets = new System.Collections.Generic.List<GameObject>
        {
            MakeTarget("Good 1", "Obstacles/Crate_01.prefab", "Particles/Explosion_Green.prefab", 5, false),
            MakeTarget("Good 2", "Obstacles/Barrel_01.prefab", "Particles/Explosion_Yellow.prefab", 5, false),
            MakeTarget("Good 3", "Props/Other/Crate_Question_01.prefab", "Particles/Explosion_Blue.prefab", 10, false),
            MakeTarget("Bad 1", "Props/Bad/SM_Prop_Bomb_01.prefab", "Particles/Explosion_Red.prefab", -5, true),
        };

        // Game Manager (5.1)
        var gm = new GameObject("Game Manager").AddComponent<GameManager>();
        gm.targets = targets;

        // Canvas + EventSystem (5.2)
        var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        var canvas = canvasGo.transform;
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(Root + "Fonts/Margarine/Margarine-Regular SDF.asset");

        // Score Text uppe till vänster (5.2)
        gm.scoreText = MakeText("Score Text", canvas, "Score: 0", 48, font, new Vector2(0, 1), new Vector2(30, -20), TextAlignmentOptions.TopLeft);

        // Game Over Text (5.3), dold från start
        gm.gameOverText = MakeText("Game Over Text", canvas, "Game Over", 120, font, new Vector2(0.5f, 0.5f), new Vector2(0, 60), TextAlignmentOptions.Center);
        gm.gameOverText.color = new Color(1f, 0.3f, 0.3f);
        gm.gameOverText.gameObject.SetActive(false);

        // Restart Button (5.3), dold från start
        var restart = MakeButton("Restart Button", canvas, "Restart", font, new Vector2(0, -80));
        UnityEventTools.AddPersistentListener(restart.onClick, gm.RestartGame);
        restart.gameObject.SetActive(false);
        gm.restartButton = restart;

        // Title Screen med titel och tre svårighetsknappar (5.4)
        var title = new GameObject("Title Screen", typeof(RectTransform));
        title.transform.SetParent(canvas, false);
        var trt = title.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one; trt.offsetMin = trt.offsetMax = Vector2.zero;
        var titleText = MakeText("Title Text", title.transform, "Clicky Mouse", 140, font, new Vector2(0.5f, 0.5f), new Vector2(0, 200), TextAlignmentOptions.Center);
        titleText.color = new Color(0.3f, 0.8f, 1f);
        MakeDifficulty("Easy Button", title.transform, "Easy", font, new Vector2(0, 0), 1);
        MakeDifficulty("Medium Button", title.transform, "Medium", font, new Vector2(0, -130), 2);
        MakeDifficulty("Hard Button", title.transform, "Hard", font, new Vector2(0, -260), 3);
        gm.titleScreen = title;

        CwcEditorHelpers.AddSceneToBuild(ScenePath);
        EditorUtility.SetDirty(gm);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Unit 5", "Klart! Prototype 5 är byggd. Välj svårighetsgrad och klicka på målen.", "OK");
    }

    static GameObject MakeTarget(string name, string modelPath, string fxPath, int points, bool bad)
    {
        var src = AssetDatabase.LoadAssetAtPath<GameObject>(Root + modelPath);
        var go = (GameObject)PrefabUtility.InstantiatePrefab(src);
        PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        go.name = name;
        if (bad) go.tag = "Bad";
        go.AddComponent<Rigidbody>();
        go.AddComponent<BoxCollider>();
        var t = go.AddComponent<Target>();
        t.pointValue = points;
        t.explosionParticle = AssetDatabase.LoadAssetAtPath<GameObject>(Root + fxPath).GetComponent<ParticleSystem>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, Out + "/" + name + ".prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    static TextMeshProUGUI MakeText(string name, Transform parent, string text, float size, TMP_FontAsset font,
                                    Vector2 anchor, Vector2 pos, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = align;
        if (font != null) tmp.font = font;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(1200, 200);
        return tmp;
    }

    static Button MakeButton(string name, Transform parent, string label, TMP_FontAsset font, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        img.type = Image.Type.Sliced;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(400, 100);
        var txt = MakeText("Text (TMP)", go.transform, label, 56, font, new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center);
        txt.color = Color.black;
        txt.rectTransform.sizeDelta = rt.sizeDelta;
        return go.GetComponent<Button>();
    }

    static void MakeDifficulty(string name, Transform parent, string label, TMP_FontAsset font, Vector2 pos, int difficulty)
    {
        var b = MakeButton(name, parent, label, font, pos);
        b.gameObject.AddComponent<DifficultyButton>().difficulty = difficulty;
    }
}
#endif
