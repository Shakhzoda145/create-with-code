#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Bygger "Prototype 4" enligt lektion 4.1–4.4. Meny: Tools > Unit 4 > Bygg Prototype 4-scenen
public static class Unit4SceneBuilder
{
    const string Root = "Assets/Samples/ExtralityLab@DSV/0.2.2/CwC-Starting-Assets/Unit 4/";
    const string Lib = Root + "Course Library/";
    const string ScenePath = Root + "Scenes/Prototype 4.unity";
    const string Out = "Assets/Unit4/Generated";

    [MenuItem("Tools/Unit 4/Bygg Prototype 4-scenen")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        if (GameObject.Find("Player") != null)
        {
            EditorUtility.DisplayDialog("Unit 4", "Scenen har redan en Player. Inget ändrades.", "OK");
            return;
        }

        CwcEditorHelpers.AddTag("Enemy");
        CwcEditorHelpers.AddTag("Powerup");
        if (!AssetDatabase.IsValidFolder(Out)) AssetDatabase.CreateFolder("Assets/Unit4", "Generated");

        // Studsigt fysikmaterial (4.2)
        var bouncy = new PhysicsMaterial("Bouncy")
        {
            bounciness = 1f,
            bounceCombine = PhysicsMaterialCombine.Multiply
        };
        AssetDatabase.CreateAsset(bouncy, Out + "/Bouncy.physicMaterial");

        // Material med kursens texturer
        var playerMat = MakeMat("Player_Mat", Lib + "Textures/PolygonPrototype_Texture_Grid_03.png");
        var enemyMat = MakeMat("Enemy_Mat", Lib + "Textures/PolygonPrototype_Texture_Grid_07.png");

        // Player (4.1)
        var player = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        player.name = "Player";
        player.transform.position = Vector3.zero;
        player.transform.localScale = Vector3.one * 1.5f;
        player.GetComponent<Renderer>().sharedMaterial = playerMat;
        player.GetComponent<SphereCollider>().sharedMaterial = bouncy;
        player.AddComponent<Rigidbody>();
        var pc = player.AddComponent<SumoPlayerController>();

        // Focal Point med kameran som barn (4.1)
        var focal = new GameObject("Focal Point");
        focal.transform.position = Vector3.zero;
        var cam = GameObject.Find("Main Camera");
        cam.transform.SetParent(focal.transform, true);
        focal.AddComponent<RotateCamera>();

        // Powerup-indikator (4.3)
        var ring = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(Lib + "Powerup Indicators/SelectionRing_02.prefab"));
        ring.name = "Powerup Indicator";
        ring.transform.localScale = Vector3.one * 2f;
        ring.SetActive(false);
        pc.powerupIndicator = ring;

        // Enemy-prefab (4.2)
        var enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        enemy.name = "Enemy";
        enemy.tag = "Enemy";
        enemy.transform.localScale = Vector3.one * 1.5f;
        enemy.GetComponent<Renderer>().sharedMaterial = enemyMat;
        enemy.GetComponent<SphereCollider>().sharedMaterial = bouncy;
        enemy.AddComponent<Rigidbody>();
        enemy.AddComponent<Enemy>();
        var enemyPrefab = PrefabUtility.SaveAsPrefabAsset(enemy, Out + "/Enemy.prefab");
        Object.DestroyImmediate(enemy);

        // Powerup-prefab (4.3)
        var gem = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(Lib + "Pickups/Gem_01.prefab"));
        PrefabUtility.UnpackPrefabInstance(gem, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        gem.name = "Powerup";
        gem.tag = "Powerup";
        var gemCol = gem.AddComponent<BoxCollider>();
        gemCol.isTrigger = true;
        var powerupPrefab = PrefabUtility.SaveAsPrefabAsset(gem, Out + "/Powerup.prefab");
        Object.DestroyImmediate(gem);

        // Spawn Manager (4.2 + 4.4)
        var sm = new GameObject("Spawn Manager").AddComponent<SpawnManager>();
        sm.enemyPrefab = enemyPrefab;
        sm.powerupPrefab = powerupPrefab;

        CwcEditorHelpers.AddSceneToBuild(ScenePath);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Unit 4", "Klart! Prototype 4 är byggd. W/S = rulla, A/D = rotera kameran.", "OK");
    }

    static Material MakeMat(string name, string texPath)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(shader) { name = name };
        var tex = Load<Texture2D>(texPath);
        if (tex != null) { m.SetTexture("_BaseMap", tex); m.mainTexture = tex; }
        AssetDatabase.CreateAsset(m, Out + "/" + name + ".mat");
        return m;
    }

    static T Load<T>(string path) where T : Object
    {
        var a = AssetDatabase.LoadAssetAtPath<T>(path);
        if (a == null) Debug.LogError("Unit 4: hittade inte " + path);
        return a;
    }
}
#endif
