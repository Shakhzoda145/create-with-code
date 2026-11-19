#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

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
