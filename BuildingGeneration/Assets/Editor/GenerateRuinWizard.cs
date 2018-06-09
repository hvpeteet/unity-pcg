using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateRuinsWizard : ScriptableWizard
{
    public string nickname = "Unnamed";
    public int rounds = 100;
    public int elites = 10;
    public int pop_size = 100;

    private static string prefabs_folder = "Assets/Prefabs";
    private static string ruins_folder = prefabs_folder + "/Ruins";


    [MenuItem("Henry's Tools/Generate Ruins")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<CreateRuinsWizard>("Generate Ruins", "Create");
    }

    private void OnStatusUpdate(float percent_complete, string status_message)
    {
        EditorUtility.DisplayProgressBar(
            "Ruin generation progress",
            status_message,
            percent_complete);
    }

    private void OnWizardCreate()
    {
        if (!AssetDatabase.IsValidFolder(prefabs_folder))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(ruins_folder))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Ruins");
        }
        string ruinPath = System.IO.Path.Combine("Assets/Prefabs/Ruins/", nickname + ".prefab");

        // Warning dialog if file exists.
        Debug.Log(AssetDatabase.FindAssets(ruinPath));
        if (AssetDatabase.LoadAssetAtPath(ruinPath, typeof(GameObject)) &&
            !EditorUtility.DisplayDialog("Replace Existing File?", "Should " + ruinPath + " be replaced?", "replace", "cancel"))
        {
            return;
        }
        RuinGenerator gen = new RuinGenerator()
        {
            num_rounds = rounds,
            num_elite = elites,
            pop_size = pop_size,
            on_status_update = OnStatusUpdate
        };
        Blueprint blueprint = gen.GenerateRuin();
        blueprint.SaveAsPrefab("Assets/Prefabs/Ruins", nickname);
        EditorUtility.ClearProgressBar();
    }
}
