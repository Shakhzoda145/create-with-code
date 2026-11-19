#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Flyttar ön så att toppen ligger strax under y = 0, så att spelare och fiender
// (som skapas på y = 0) landar ovanpå ön i stället för inuti den. Ger ön ett rutnätsmaterial.
public static class Unit4IslandFix
{
    [MenuItem("Tools/Unit 4/Justera ön")]
    public static void Fix()
    {
        var island = GameObject.Find("Island");
        if (island == null) { Debug.LogError("Unit 4: hittade ingen Island i scenen."); return; }
        var r = island.GetComponent<Renderer>();
        float top = r.bounds.max.y;
        Debug.Log("Unit 4: öns topp låg på y = " + top + ", bounds = " + r.bounds);
        island.transform.position += Vector3.up * (-0.8f - top);
        Debug.Log("Unit 4: öns topp ligger nu på y = " + r.bounds.max.y);

        var shader = Shader.Find("Universal Render Pipeline/Lit");
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/Samples/ExtralityLab@DSV/0.2.2/CwC-Starting-Assets/Unit 4/Course Library/Textures/PolygonPrototype_Texture_Grid_01.png");
        if (shader != null && tex != null)
        {
            var m = new Material(shader) { name = "Island_Mat" };
            m.SetTexture("_BaseMap", tex);
            m.mainTextureScale = new Vector2(4, 4);
            AssetDatabase.CreateAsset(m, "Assets/Unit4/Generated/Island_Mat.mat");
            r.sharedMaterial = m;
        }

        EditorSceneManager.MarkSceneDirty(island.scene);
        EditorSceneManager.SaveScene(island.scene);
        AssetDatabase.SaveAssets();
    }
}
#endif
