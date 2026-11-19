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
}
#endif
