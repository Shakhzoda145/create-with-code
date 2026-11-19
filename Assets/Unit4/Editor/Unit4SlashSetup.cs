#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Hjälpmenyer för slash-tillägget till Unit 4. Varje menyval motsvarar ett steg.
public static class Unit4SlashSetup
{
    const string ScenePath = "Assets/Samples/ExtralityLab@DSV/0.2.2/CwC-Starting-Assets/Unit 4/Scenes/Prototype 4.unity";
    const string Out = "Assets/Unit4/Generated";

    static UnityEngine.SceneManagement.Scene OpenScene()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        return EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    static void Save(UnityEngine.SceneManagement.Scene scene)
    {
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
    }

    // Steg 1: sex väggar runt den sexkantiga ön
    [MenuItem("Tools/Unit 4 Slash/1 Lägg till väggar")]
    public static void AddWalls()
    {
        var scene = OpenScene();
        if (GameObject.Find("Walls") != null) { Debug.Log("Slash: väggarna finns redan."); return; }

        var island = GameObject.Find("Island").GetComponent<Renderer>();
        float top = island.bounds.max.y;          // öns yta
        float apothem = island.bounds.extents.z - 0.2f; // avstånd från mitten till en sida
        float side = island.bounds.extents.x;     // sidans längd i en regelbunden sexhörning

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Wall_Mat" };
        mat.SetColor("_BaseColor", new Color(0.55f, 0.75f, 0.95f));
        AssetDatabase.CreateAsset(mat, Out + "/Wall_Mat.mat");

        var walls = new GameObject("Walls");
        for (int i = 0; i < 6; i++)
        {
            float normalAngle = 30f + 60f * i;     // riktningen ut från mitten mot sidan
            float rad = normalAngle * Mathf.Deg2Rad;
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall " + (i + 1);
            wall.transform.SetParent(walls.transform);
            wall.transform.position = new Vector3(Mathf.Cos(rad) * apothem, top + 0.75f, Mathf.Sin(rad) * apothem);
            wall.transform.rotation = Quaternion.Euler(0, 90f - normalAngle, 0);
            wall.transform.localScale = new Vector3(side + 0.5f, 1.5f, 0.5f);
            wall.GetComponent<Renderer>().sharedMaterial = mat;
        }
        Save(scene);
        Debug.Log("Slash: sex väggar tillagda.");
    }
    // Steg 4: svärd som sveper + ljud vid varje slash
    [MenuItem("Tools/Unit 4 Slash/2 Lägg till svärd och ljud")]
    public static void AddSword()
    {
        var scene = OpenScene();
        var player = GameObject.Find("Player");
        var old = player.transform.Find("Sword Pivot");
        if (old != null) Object.DestroyImmediate(old.gameObject); // bygg om svärdet om det redan finns

        var steel = Mat("Sword_Steel", "Universal Render Pipeline/Lit");
        steel.SetColor("_BaseColor", new Color(0.85f, 0.88f, 0.92f));
        steel.SetFloat("_Metallic", 0.3f);
        steel.SetFloat("_Smoothness", 0.8f);
        EditorUtility.SetDirty(steel);
        var wood = Mat("Sword_Handle", "Universal Render Pipeline/Lit");
        wood.SetColor("_BaseColor", new Color(0.4f, 0.22f, 0.1f));
        var trailMat = Mat("Sword_Trail", "Sprites/Default");

        // Pivot i spelarens mitt; skala bort spelarens 1.5 så att svärdet får "riktiga" mått
        var pivot = new GameObject("Sword Pivot");
        pivot.transform.SetParent(player.transform, false);
        pivot.transform.localScale = Vector3.one / player.transform.localScale.x;

        Part("Handle", pivot, new Vector3(0, 0.3f, 0.45f), new Vector3(0.14f, 0.14f, 0.5f), wood);
        Part("Guard", pivot, new Vector3(0, 0.3f, 0.72f), new Vector3(0.6f, 0.1f, 0.12f), wood);
        Part("Blade", pivot, new Vector3(0, 0.3f, 1.6f), new Vector3(0.3f, 0.06f, 1.7f), steel);

        var tip = new GameObject("Tip");
        tip.transform.SetParent(pivot.transform, false);
        tip.transform.localPosition = new Vector3(0, 0.3f, 2.3f);
        var trail = tip.AddComponent<TrailRenderer>();
        trail.sharedMaterial = trailMat;
        trail.time = 0.3f;
        trail.widthMultiplier = 0.8f;
        trail.widthCurve = AnimationCurve.Linear(0, 1, 1, 0);
        var g = new Gradient();
        g.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(new Color(0.6f, 0.85f, 1f), 1) },
                  new[] { new GradientAlphaKey(0.9f, 0), new GradientAlphaKey(0f, 1) });
        trail.colorGradient = g;
        trail.emitting = false;

        var swing = pivot.AddComponent<SwordSwing>();
        swing.trail = trail;

        var audio = player.GetComponent<AudioSource>();
        if (audio == null) audio = player.AddComponent<AudioSource>();
        audio.playOnAwake = false;

        var ctrl = player.GetComponent<SumoPlayerController>();
        ctrl.sword = swing;
        ctrl.slashSound = AssetDatabase.LoadAssetAtPath<AudioClip>(Out + "/Swoosh.wav");
        EditorUtility.SetDirty(ctrl);

        Save(scene);
        Debug.Log("Slash: svärd och ljud tillagda.");
    }

    // Steg 5: byt bollen mot en karaktär från Unit 2
    [MenuItem("Tools/Unit 4 Slash/3 Byt bollen mot karaktär")]
    public static void AddCharacter()
    {
        var scene = OpenScene();
        var player = GameObject.Find("Player");
        var old = player.transform.Find("Character");
        if (old != null) Object.DestroyImmediate(old.gameObject); // bygg om om den redan finns

        const string prefabPath = "Assets/Samples/ExtralityLab@DSV/0.2.2/CwC-Starting-Assets/Unit 2/Course Library/Humans/SimpleFarmer_Man_01_Brown.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var character = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        character.name = "Character";

        // Ta bort ev. colliders så att bara spelarens osynliga klot krockar
        foreach (var c in character.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
        var anim = character.GetComponent<Animator>();
        if (anim != null) anim.applyRootMotion = false; // animationen får inte flytta karaktären

        character.transform.SetParent(player.transform, false);
        character.transform.localRotation = Quaternion.identity;

        // Skala så att karaktären blir ca 3 enheter lång i världen
        float targetHeight = 3.0f;
        float height = GetBounds(character).size.y;
        float worldScale = targetHeight / height;
        character.transform.localScale = Vector3.one * worldScale / player.transform.lossyScale.y;

        // Ställ fötterna på botten av spelarens klot
        float sphereBottom = player.GetComponent<Collider>().bounds.min.y;
        float feet = GetBounds(character).min.y;
        character.transform.position += Vector3.up * (sphereBottom - feet);

        // Koppla animationerna till spelarens styrning
        var ctrl = player.GetComponent<SumoPlayerController>();
        ctrl.characterAnim = anim;
        EditorUtility.SetDirty(ctrl);

        // Flytta upp svärdet till ungefär handhöjd
        var pivot = player.transform.Find("Sword Pivot");
        if (pivot != null)
        {
            float handY = sphereBottom + 0.45f * targetHeight - 0.3f; // 0.3 = klingans höjd i pivoten
            pivot.position = new Vector3(pivot.position.x, handY, pivot.position.z);
        }

        // Göm bollen (collidern och fysiken finns kvar)
        player.GetComponent<MeshRenderer>().enabled = false;

        Save(scene);
        Debug.Log("Slash: karaktär tillagd, höjd " + height + " -> " + targetHeight);
    }

    // Steg 6: poäng, liv, rundor och game over
    [MenuItem("Tools/Unit 4 Slash/4 Lägg till poäng, liv och rundor")]
    public static void AddGameLoop()
    {
        var scene = OpenScene();
        var oldCanvas = GameObject.Find("Slash Canvas");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas);
        var oldGm = GameObject.Find("Slash Game Manager");
        if (oldGm != null) Object.DestroyImmediate(oldGm);

        var canvasGo = new GameObject("Slash Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        var gm = new GameObject("Slash Game Manager").AddComponent<SlashGameManager>();
        gm.scoreText = Text(canvasGo, "Score Text", new Vector2(0, 1), new Vector2(30, -20), TextAlignmentOptions.TopLeft, 48);
        gm.roundText = Text(canvasGo, "Round Text", new Vector2(0.5f, 1), new Vector2(0, -20), TextAlignmentOptions.Top, 48);
        gm.livesText = Text(canvasGo, "Lives Text", new Vector2(1, 1), new Vector2(-30, -20), TextAlignmentOptions.TopRight, 48);
        gm.gameOverText = Text(canvasGo, "Game Over Text", new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center, 80);
        gm.gameOverText.color = new Color(1f, 0.35f, 0.3f);
        gm.gameOverText.rectTransform.sizeDelta = new Vector2(1400, 500);

        // Scenen måste finnas i Build Settings för att R (starta om) ska fungera
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(sc => sc.path == ScenePath))
        {
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        Save(scene);
        Debug.Log("Slash: poäng, liv och rundor tillagda.");
    }

    static TextMeshProUGUI Text(GameObject canvas, string name, Vector2 anchor, Vector2 pos, TextAlignmentOptions align, float size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.font = TMP_Settings.defaultFontAsset;
        t.fontSize = size;
        t.alignment = align;
        t.color = Color.white;
        t.text = name;
        var rt = t.rectTransform;
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(500, 80);
        return t;
    }

    // Steg 7: japansk stil – gräsplan, bonsaiträd, torii-port, Fuji och solnedgång
    [MenuItem("Tools/Unit 4 Slash/5 Japansk stil")]
    public static void AddJapanTheme()
    {
        var scene = OpenScene();
        var oldRoot = GameObject.Find("Japan Theme");
        if (oldRoot != null) Object.DestroyImmediate(oldRoot); // bygg om om den redan finns
        var root = new GameObject("Japan Theme");

        var islandGo = GameObject.Find("Island");
        var island = islandGo.GetComponent<Renderer>();
        float top = island.bounds.max.y;
        float radius = island.bounds.extents.x; // avstånd från mitten till ett hörn

        // Material
        var grass = Lit("JP_Grass", new Color(0.36f, 0.62f, 0.25f), 0.1f);
        var stone = Lit("JP_Stone", new Color(0.42f, 0.41f, 0.40f), 0.2f);
        var wood = Lit("JP_Wood", new Color(0.32f, 0.19f, 0.10f), 0.3f);
        var bamboo = Lit("JP_Bamboo", new Color(0.62f, 0.66f, 0.30f), 0.4f);
        var red = Lit("JP_ToriiRed", new Color(0.80f, 0.13f, 0.08f), 0.3f);
        var black = Lit("JP_Black", new Color(0.08f, 0.08f, 0.08f), 0.3f);
        var leaves = Lit("JP_Leaves", new Color(0.18f, 0.42f, 0.20f), 0.1f);
        var blossom = Lit("JP_Blossom", new Color(1.0f, 0.72f, 0.82f), 0.1f);
        var pot = Lit("JP_Pot", new Color(0.20f, 0.30f, 0.50f), 0.7f);
        var mountain = Lit("JP_Mountain", new Color(0.55f, 0.55f, 0.72f), 0.0f);
        var snow = Lit("JP_Snow", new Color(0.95f, 0.95f, 1.0f), 0.2f);
        var sun = Mat("JP_Sun", "Universal Render Pipeline/Unlit");
        sun.SetColor("_BaseColor", new Color(0.95f, 0.25f, 0.2f));
        EditorUtility.SetDirty(sun);

        // 1. Ön: sten på sidorna och en sexkantig gräsmatta ovanpå
        var mats = island.sharedMaterials;
        for (int i = 0; i < mats.Length; i++) mats[i] = stone;
        island.sharedMaterials = mats;
        var lawn = new GameObject("Grass");
        lawn.transform.SetParent(root.transform);
        lawn.transform.position = new Vector3(0, top + 0.02f, 0);
        lawn.AddComponent<MeshFilter>().sharedMesh = HexMesh(radius);
        lawn.AddComponent<MeshRenderer>().sharedMaterial = grass;

        // 2. Väggarna blir trästaket med bambustolpar i hörnen
        var walls = GameObject.Find("Walls");
        if (walls != null)
            foreach (var r in walls.GetComponentsInChildren<Renderer>()) r.sharedMaterial = wood;
        for (int i = 0; i < 6; i++)
        {
            float a = 60f * i * Mathf.Deg2Rad;
            var post = Prim(PrimitiveType.Cylinder, "Bamboo Post", root, bamboo);
            post.transform.position = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * (radius - 0.5f) + Vector3.up * (top + 1.1f);
            post.transform.localScale = new Vector3(0.6f, 1.1f, 0.6f);
        }

        // 3. Bonsaiträd på svävande stenar runt ön
        var rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Samples/ExtralityLab@DSV/0.2.2/CwC-Starting-Assets/Unit 1/Obstacles/SM_Rock_Boulder_01.prefab");
        float[] angles = { 20, 75, 140, 200, 255, 320 };
        for (int i = 0; i < angles.Length; i++)
        {
            float a = angles[i] * Mathf.Deg2Rad;
            float dist = radius + 10f + (i % 2) * 5f;
            Vector3 basePos = new Vector3(Mathf.Cos(a) * dist, top - 2f - (i % 3) * 1.5f, Mathf.Sin(a) * dist);
            if (rockPrefab != null) Rock(rockPrefab, root, basePos, 6f);
            bool cherry = i % 3 == 1;
            Bonsai(root, basePos, 1.3f + 0.25f * (i % 2), cherry ? blossom : leaves, wood, pot, i);
        }

        // 4. Torii-port bakom spelplanen (i riktningen kameran tittar från början)
        Vector3 toriiPos = new Vector3(0, top - 4f, radius + 9f);
        if (rockPrefab != null) Rock(rockPrefab, root, toriiPos, 7f);
        Torii(root, toriiPos, red, black);

        // 5. Bakgrund: Fuji, bergskedja och en röd sol
        var cone = ConeMesh();
        var fuji = Shape("Mount Fuji", root, cone, mountain, new Vector3(25, -50, 110), new Vector3(130, 70, 130));
        Shape("Fuji Snow", fuji, cone, snow, new Vector3(0, 0.62f, 0), new Vector3(0.4f, 0.38f, 0.4f), true);
        for (int i = 0; i < 12; i++)
        {
            float a = (i * 30f + 15f) * Mathf.Deg2Rad;
            if (Mathf.Abs(Mathf.DeltaAngle(i * 30f + 15f, 77f)) < 25f) continue; // lämna plats åt Fuji
            float d = 120f + (i % 3) * 25f;
            float h = 40f + (i * 37 % 5) * 8f;
            Shape("Mountain", root, cone, mountain, new Vector3(Mathf.Cos(a) * d, -50, Mathf.Sin(a) * d), new Vector3(90, h, 90));
        }
        // Moln-tussar under ön ger djup
        var puff = Lit("JP_CloudPuff", new Color(1f, 0.95f, 0.95f), 0f);
        for (int i = 0; i < 16; i++)
        {
            float a = (i * 137f) * Mathf.Deg2Rad;
            float d = 25f + (i * 53 % 40);
            Vector3 c = new Vector3(Mathf.Cos(a) * d, -28f - (i % 4) * 4f, Mathf.Sin(a) * d);
            for (int k = 0; k < 3; k++)
            {
                var ball = Prim(PrimitiveType.Sphere, "Cloud", root, puff);
                ball.transform.position = c + new Vector3((k - 1) * 4f, (k == 1 ? 1.5f : 0f), (k % 2) * 2f);
                ball.transform.localScale = new Vector3(9f, 4f, 7f) * (k == 1 ? 1.2f : 0.9f);
            }
        }
        var sunGo = Prim(PrimitiveType.Sphere, "Rising Sun", root, sun);
        sunGo.transform.position = new Vector3(-70, 15, 140);
        sunGo.transform.localScale = Vector3.one * 35f;

        // Ett hav av moln långt under ön
        var cloudMat = Mat("JP_Clouds", "Universal Render Pipeline/Unlit");
        cloudMat.SetColor("_BaseColor", new Color(0.93f, 0.74f, 0.78f));
        EditorUtility.SetDirty(cloudMat);
        var clouds = Prim(PrimitiveType.Plane, "Sea of Clouds", root, cloudMat);
        clouds.transform.position = new Vector3(0, -45f, 0);
        clouds.transform.localScale = new Vector3(120, 1, 120);

        // 6. Fallande körsbärsblad
        Petals(root, top);

        // 7. Himmel, dimma och ljus i solnedgångsfärger
        var sky = Mat("JP_Sky", "Skybox/Procedural");
        sky.SetFloat("_SunDisk", 0);
        sky.SetColor("_SkyTint", new Color(1.0f, 0.55f, 0.5f));
        sky.SetColor("_GroundColor", new Color(1.0f, 0.8f, 0.75f));
        sky.SetFloat("_AtmosphereThickness", 1.6f);
        sky.SetFloat("_Exposure", 1.3f);
        EditorUtility.SetDirty(sky);
        RenderSettings.skybox = sky;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.98f, 0.78f, 0.75f);
        RenderSettings.fogStartDistance = 50f;
        RenderSettings.fogEndDistance = 260f;

        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.farClipPlane = 1000f;
            cam.transform.localRotation = Quaternion.Euler(25, 0, 0); // lite flackare så att horisonten och Fuji syns
        }
        var light = GameObject.Find("Directional Light");
        if (light != null) light.GetComponent<Light>().color = new Color(1f, 0.86f, 0.72f);
        foreach (var name in new[] { "SkyDome", "FX_Mist" })
        {
            var go = GameObject.Find(name);
            if (go != null) go.SetActive(false);   // kursens gamla himmel göms
        }

        Save(scene);
        Debug.Log("Slash: japansk stil tillagd.");
    }

    // Steg 8: bakgrundsmusik som loopar
    [MenuItem("Tools/Unit 4 Slash/6 Lägg till musik")]
    public static void AddMusic()
    {
        const string songPath = "Assets/Unit4/Strong And Strike (Extended).mp3";
        // Strömma låten i stället för att packa upp hela i minnet
        var importer = (AudioImporter)AssetImporter.GetAtPath(songPath);
        var settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.Streaming;
        importer.defaultSampleSettings = settings;
        importer.SaveAndReimport();

        var scene = OpenScene();
        var old = GameObject.Find("Music");
        if (old != null) Object.DestroyImmediate(old);

        var music = new GameObject("Music").AddComponent<AudioSource>();
        music.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(songPath);
        music.loop = true;
        music.playOnAwake = true;
        music.volume = 0.35f;   // lägre än svärdsljudet

        var gm = Object.FindAnyObjectByType<SlashGameManager>();
        if (gm != null) { gm.music = music; EditorUtility.SetDirty(gm); }

        Save(scene);
        Debug.Log("Slash: musik tillagd.");
    }

    static Material Lit(string name, Color color, float smooth)
    {
        var m = Mat(name, "Universal Render Pipeline/Lit");
        m.SetColor("_BaseColor", color);
        m.SetFloat("_Smoothness", smooth);
        EditorUtility.SetDirty(m);
        return m;
    }

    static GameObject Prim(PrimitiveType type, string name, GameObject parent, Material mat)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent.transform, false);
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    // Cylinder mellan två punkter (för stam och grenar)
    static void Segment(GameObject parent, Vector3 from, Vector3 to, float r, Material mat)
    {
        var seg = Prim(PrimitiveType.Cylinder, "Trunk", parent, mat);
        seg.transform.localPosition = (from + to) / 2f;
        seg.transform.localRotation = Quaternion.FromToRotation(Vector3.up, to - from);
        seg.transform.localScale = new Vector3(r * 2, (to - from).magnitude / 2f, r * 2);
    }

    static void Bonsai(GameObject root, Vector3 pos, float scale, Material leafMat, Material trunkMat, Material potMat, int seed)
    {
        var tree = new GameObject("Bonsai");
        tree.transform.SetParent(root.transform);
        tree.transform.position = pos;
        tree.transform.rotation = Quaternion.Euler(0, seed * 47f, 0);
        tree.transform.localScale = Vector3.one * scale;

        var p = Prim(PrimitiveType.Cylinder, "Pot", tree, potMat);
        p.transform.localPosition = new Vector3(0, 0.35f, 0);
        p.transform.localScale = new Vector3(2.6f, 0.35f, 1.8f);

        // En knotig stam i flera delar, som ett bonsaiträd
        Vector3 p0 = new Vector3(0, 0.7f, 0), p1 = new Vector3(0.6f, 2.0f, 0.2f),
                p2 = new Vector3(-0.5f, 3.1f, 0f), p3 = new Vector3(0.3f, 4.0f, -0.1f),
                b1 = new Vector3(1.8f, 2.5f, 0.3f), b2 = new Vector3(-1.8f, 3.4f, 0.2f);
        Segment(tree, p0, p1, 0.28f, trunkMat);
        Segment(tree, p1, p2, 0.22f, trunkMat);
        Segment(tree, p2, p3, 0.16f, trunkMat);
        Segment(tree, p1, b1, 0.12f, trunkMat);
        Segment(tree, p2, b2, 0.12f, trunkMat);

        // Platta "molnkuddar" av blad
        Vector3[] pads = { b1 + Vector3.up * 0.2f, b2 + Vector3.up * 0.2f, p3 + Vector3.up * 0.3f };
        Vector3[] sizes = { new Vector3(2.0f, 0.9f, 1.6f), new Vector3(2.2f, 0.9f, 1.7f), new Vector3(2.6f, 1.1f, 2.1f) };
        for (int i = 0; i < pads.Length; i++)
        {
            // Varje kudde byggs av några överlappande klot så att den ser fluffig ut
            for (int k = 0; k < 4; k++)
            {
                var pad = Prim(PrimitiveType.Sphere, "Foliage", tree, leafMat);
                float ang = (k * 90f + seed * 30f) * Mathf.Deg2Rad;
                Vector3 offset = k == 0 ? new Vector3(0, 0.15f, 0) : new Vector3(Mathf.Cos(ang) * sizes[i].x * 0.3f, 0, Mathf.Sin(ang) * sizes[i].z * 0.3f);
                pad.transform.localPosition = pads[i] + offset;
                pad.transform.localScale = k == 0 ? sizes[i] : new Vector3(sizes[i].x * 0.6f, sizes[i].y * 1.1f, sizes[i].z * 0.6f);
            }
        }
    }

    static void Torii(GameObject root, Vector3 pos, Material red, Material black)
    {
        var gate = new GameObject("Torii Gate");
        gate.transform.SetParent(root.transform);
        gate.transform.position = pos;
        foreach (float x in new[] { -3f, 3f })
        {
            var post = Prim(PrimitiveType.Cylinder, "Post", gate, red);
            post.transform.localPosition = new Vector3(x, 3.5f, 0);
            post.transform.localScale = new Vector3(0.7f, 3.5f, 0.7f);
        }
        var beam = Prim(PrimitiveType.Cube, "Nuki", gate, red);
        beam.transform.localPosition = new Vector3(0, 5.6f, 0);
        beam.transform.localScale = new Vector3(7.6f, 0.45f, 0.5f);
        var kasagi = Prim(PrimitiveType.Cube, "Kasagi", gate, red);
        kasagi.transform.localPosition = new Vector3(0, 7.1f, 0);
        kasagi.transform.localScale = new Vector3(9.2f, 0.6f, 0.9f);
        var cap = Prim(PrimitiveType.Cube, "Cap", gate, black);
        cap.transform.localPosition = new Vector3(0, 7.55f, 0);
        cap.transform.localScale = new Vector3(9.8f, 0.35f, 1.1f);
    }

    static void Rock(GameObject prefab, GameObject root, Vector3 topCenter, float width)
    {
        var rock = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        rock.name = "Floating Rock";
        foreach (var c in rock.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
        rock.transform.SetParent(root.transform);
        var b = GetBounds(rock);
        rock.transform.localScale *= width / Mathf.Max(b.size.x, b.size.z);
        b = GetBounds(rock);
        rock.transform.position += topCenter - new Vector3(b.center.x, b.max.y - 0.3f, b.center.z);
    }

    static GameObject Shape(string name, GameObject parent, Mesh mesh, Material mat, Vector3 pos, Vector3 scale, bool local = false)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, local);
        if (local) go.transform.localPosition = pos; else go.transform.position = pos;
        go.transform.localScale = scale;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }

    static void Petals(GameObject root, float top)
    {
        var go = new GameObject("Sakura Petals");
        go.transform.SetParent(root.transform);
        go.transform.position = new Vector3(0, top + 12f, 0);
        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 10f;
        main.startSpeed = 0.3f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.75f, 0.85f), new Color(1f, 0.9f, 0.95f));
        main.gravityModifier = 0.03f;
        main.maxParticles = 400;
        main.startRotation = new ParticleSystem.MinMaxCurve(0, Mathf.PI * 2);
        var em = ps.emission;
        em.rateOverTime = 20f;
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(40, 1, 40);
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        var mat = Mat("JP_Petal", "Universal Render Pipeline/Particles/Unlit");
        renderer.sharedMaterial = mat;
    }

    static Mesh HexMesh(float radius)
    {
        string path = Out + "/GrassHex.asset";
        var old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (old != null) return old;
        var v = new Vector3[7];
        var t = new int[18];
        for (int i = 0; i < 6; i++)
        {
            float a = 60f * i * Mathf.Deg2Rad;
            v[i + 1] = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a)) * radius;
            t[i * 3] = 0; t[i * 3 + 1] = (i + 1) % 6 + 1; t[i * 3 + 2] = i + 1; // medurs sett ovanifrån
        }
        var m = new Mesh { name = "GrassHex", vertices = v, triangles = t };
        m.RecalculateNormals();
        m.RecalculateBounds();
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static Mesh ConeMesh()
    {
        string path = Out + "/Cone.asset";
        var old = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (old != null) return old;
        int n = 24;
        var v = new Vector3[n + 2];
        var t = new int[n * 3];
        v[0] = new Vector3(0, 1, 0);           // toppen
        for (int i = 0; i <= n; i++)
        {
            float a = i * Mathf.PI * 2 / n;
            v[i + 1] = new Vector3(Mathf.Cos(a) * 0.5f, 0, Mathf.Sin(a) * 0.5f);
        }
        for (int i = 0; i < n; i++)
        {
            t[i * 3] = 0; t[i * 3 + 1] = i + 2; t[i * 3 + 2] = i + 1;
        }
        var m = new Mesh { name = "Cone", vertices = v, triangles = t };
        m.RecalculateNormals();
        m.RecalculateBounds();
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static Bounds GetBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>();
        var b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    // Hämtar ett material om det redan finns, annars skapas det
    static Material Mat(string name, string shader)
    {
        string path = Out + "/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find(shader)) { name = name };
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }

    // Skapar en kub utan collider (så att svärdet inte knuffar fiender eller spelaren)
    static void Part(string name, GameObject parent, Vector3 pos, Vector3 scale, Material mat)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        Object.DestroyImmediate(part.GetComponent<Collider>());
        part.transform.SetParent(parent.transform, false);
        part.transform.localPosition = pos;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = mat;
    }
}
#endif
