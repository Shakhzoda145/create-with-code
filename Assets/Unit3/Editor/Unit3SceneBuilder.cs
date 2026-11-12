#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

// Bygger scenen "Prototype 3" enligt lektion 3.1–3.4.
// Kör via menyn: Tools > Unit 3 > Bygg Prototype 3-scenen
public static class Unit3SceneBuilder
{
    const string Root = "Assets/Samples/ExtralityLab@DSV/0.2.2/CwC-Starting-Assets/Unit 3/";
    const string Lib = Root + "Course Library/";
    const string ScenePath = Root + "Scenes/Prototype 3.unity";
    const string PrefabFolder = "Assets/Prefabs/Unit3";

    [MenuItem("Tools/Unit 3/Bygg Prototype 3-scenen")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        if (GameObject.Find("Player") != null)
        {
            EditorUtility.DisplayDialog("Unit 3", "Scenen har redan ett Player-objekt. Inget ändrades.", "OK");
            return;
        }

        // Taggar (3.2)
        AddTag("Ground");
        AddTag("Obstacle");

        // Ground (3.2)
        var ground = GameObject.Find("Ground");
        ground.tag = "Ground";

        // Background (3.1–3.2)
        var background = GameObject.Find("Background");
        var bgCol = Add<BoxCollider>(background);
        var sr = background.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            bgCol.size = new Vector3(sr.sprite.bounds.size.x, sr.sprite.bounds.size.y, 0.2f);
            bgCol.center = sr.sprite.bounds.center;
        }
        bgCol.isTrigger = true; // ska inte krocka med något
        Add<MoveLeft>(background);
        Add<RepeatBackground>(background);

        // Player (3.1)
        var charPrefab = Load<GameObject>(Lib + "Characters/Town/SimplePeople_Man_Business_White.prefab");
        var player = (GameObject)PrefabUtility.InstantiatePrefab(charPrefab);
        player.name = "Player";
        player.transform.position = Vector3.zero;
        player.transform.rotation = Quaternion.Euler(0, 90, 0); // tittar åt höger

        var rb = player.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionY;
        FitBoxCollider(player, player.AddComponent<BoxCollider>());

        var anim = player.GetComponent<Animator>();
        anim.applyRootMotion = false; // annars springer han iväg framåt av animationen

        var audio = player.AddComponent<AudioSource>();
        audio.playOnAwake = false;

        var pc = player.AddComponent<RunnerPlayerController>();
        pc.jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        pc.jumpForce = 10;
        pc.gravityModifier = 1.5f;
        pc.jumpSound = Load<AudioClip>(Lib + "Sound/SFX/jump1.wav");
        pc.crashSound = Load<AudioClip>(Lib + "Sound/SFX/crash1.wav");

        // Partiklar som barn till Player (3.4)
        var explosion = InstantiateChild(Lib + "Particles/FX_Explosion_Smoke.prefab", player, new Vector3(0, 1.5f, 0));
        var exPs = explosion.GetComponent<ParticleSystem>();
        var exMain = exPs.main; exMain.playOnAwake = false;
        pc.explosionParticle = exPs;

        var dirt = InstantiateChild(Lib + "Particles/FX_DirtSplatter.prefab", player, new Vector3(0, 0.1f, -0.5f));
        dirt.transform.rotation = Quaternion.Euler(0, -90, 0); // sprutar bakåt (åt vänster)
        pc.dirtParticle = dirt.GetComponent<ParticleSystem>();

        // Animator (3.3): börja springa direkt
        SetupAnimator(anim.runtimeAnimatorController as AnimatorController);

        // Musik på Main Camera (3.4)
        var cam = GameObject.Find("Main Camera");
        var music = Add<AudioSource>(cam);
        music.clip = Load<AudioClip>(Lib + "Sound/Music/cron_audio_8-bit_modern01.ogg");
        music.loop = true;
        music.volume = 0.4f;
        music.playOnAwake = true;

        // Obstacle-prefab (3.1–3.2)
        if (!AssetDatabase.IsValidFolder(PrefabFolder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");
            AssetDatabase.CreateFolder("Assets/Prefabs", "Unit3");
        }
        var obstacleSrc = Load<GameObject>(Lib + "Obstacles/Prop_Barrier01.prefab");
        var obstacle = (GameObject)PrefabUtility.InstantiatePrefab(obstacleSrc);
        PrefabUtility.UnpackPrefabInstance(obstacle, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        obstacle.name = "Obstacle";
        obstacle.transform.position = new Vector3(25, 0, 0);
        obstacle.tag = "Obstacle";
        obstacle.AddComponent<Rigidbody>();
        FitBoxCollider(obstacle, obstacle.AddComponent<BoxCollider>());
        obstacle.AddComponent<MoveLeft>();
        var obstaclePrefab = PrefabUtility.SaveAsPrefabAsset(obstacle, PrefabFolder + "/Obstacle.prefab");
        Object.DestroyImmediate(obstacle);

        // Spawn Manager (3.1)
        var spawn = new GameObject("Spawn Manager");
        spawn.AddComponent<ObstacleSpawnManager>().obstaclePrefab = obstaclePrefab;

        EditorUtility.SetDirty(pc);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Unit 3", "Klart! Scenen Prototype 3 är byggd och sparad. Tryck Play och hoppa med Space.", "OK");
    }

    static T Add<T>(GameObject go) where T : Component
    {
        return go.TryGetComponent<T>(out var c) ? c : go.AddComponent<T>();
    }

    static T Load<T>(string path) where T : Object
    {
        var a = AssetDatabase.LoadAssetAtPath<T>(path);
        if (a == null) Debug.LogError("Unit 3: hittade inte " + path);
        return a;
    }

    static GameObject InstantiateChild(string path, GameObject parent, Vector3 localPos)
    {
        var go = (GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>(path));
        go.transform.SetParent(parent.transform, false);
        go.transform.localPosition = localPos;
        return go;
    }

    // Anpassar en BoxCollider efter objektets synliga modell
    static void FitBoxCollider(GameObject go, BoxCollider col)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;
        var b = renderers[0].bounds;
        foreach (var r in renderers) b.Encapsulate(r.bounds);
        var t = go.transform;
        col.center = t.InverseTransformPoint(b.center);
        var s = t.InverseTransformVector(b.size);
        col.size = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
    }

    static void AddTag(string tag)
    {
        var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags = tm.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tm.ApplyModifiedProperties();
    }

    static void SetupAnimator(AnimatorController ctrl)
    {
        if (ctrl == null) { Debug.LogWarning("Unit 3: ingen AnimatorController på Player."); return; }

        // Speed_f = 1.0 så att han springer
        var ps = ctrl.parameters;
        for (int i = 0; i < ps.Length; i++)
            if (ps[i].name == "Speed_f") ps[i].defaultFloat = 1.0f;
        ctrl.parameters = ps;

        // Run_Static som standardläge, Running_Jump lite långsammare
        foreach (var layer in ctrl.layers)
        {
            foreach (var cs in layer.stateMachine.states)
            {
                if (cs.state.name == "Run_Static" && layer.stateMachine == ctrl.layers[0].stateMachine)
                    layer.stateMachine.defaultState = cs.state;
            }
            SetSpeedRecursive(layer.stateMachine, "Running_Jump", 0.7f);
        }
        EditorUtility.SetDirty(ctrl);
    }

    static void SetSpeedRecursive(AnimatorStateMachine sm, string name, float speed)
    {
        foreach (var cs in sm.states) if (cs.state.name == name) cs.state.speed = speed;
        foreach (var child in sm.stateMachines) SetSpeedRecursive(child.stateMachine, name, speed);
    }
}
#endif
