using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class LevelEditorWindow : EditorWindow
{
    public GameObject levelPrefab;  // Prefab level
    public GameObject unitPrefab;   // Prefab unit
    public GameObject BlockPrefab;   // Prefab unit
    public string unitContainerName;  // Container trong prefab level để chứa các unit
    public string unitContainerNameClone;// Container trong prefab level để chứa các unit
    public int index = 0;
    [MenuItem("Window/Level editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level editor");
    }
    void OnGUI()
    {
        GUILayout.Label("Modify Prefab Level", EditorStyles.boldLabel);

        levelPrefab = (GameObject)EditorGUILayout.ObjectField("Level Prefab", levelPrefab, typeof(GameObject), false);
        unitPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", unitPrefab, typeof(GameObject), false);
        BlockPrefab = (GameObject)EditorGUILayout.ObjectField("Block Prefab", BlockPrefab, typeof(GameObject), false);
        unitContainerName = EditorGUILayout.TextField("Container Name", unitContainerName);
        unitContainerNameClone = EditorGUILayout.TextField("Container Clone Name", unitContainerNameClone);
        index = EditorGUILayout.IntField("Index", index);
        EditorGUILayout.Space();

        if (GUILayout.Button("Update Prefab Level"))
        {
            UpdatePrefabLevel();
        }
    }

    void UpdatePrefabLevel()
    {
        if (levelPrefab == null || unitPrefab == null || string.IsNullOrEmpty(unitContainerName))
        {
            Debug.LogError("Please assign all fields and ensure container name is provided.");
            return;
        }

        // Get the prefab asset path
        string prefabPath = AssetDatabase.GetAssetPath(levelPrefab);

        // Load the prefab asset
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot == null)
        {
            Debug.LogError("Failed to load prefab contents.");
            return;
        }

        // Find the unit container in the prefab by name
        Transform prefabUnitContainer = prefabRoot.transform.GetChild(0).GetChild(0).Find(unitContainerName);
        if (prefabUnitContainer == null)
        {
            Debug.LogError($"Unit Container '{unitContainerName}' not found in the prefab.");
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return;
        }
        Transform prefabUnitContainerClone = prefabRoot.transform.GetChild(0).GetChild(0).Find(unitContainerNameClone);
        if (prefabUnitContainerClone == null)
        {
            Debug.LogError($"Unit Container '{unitContainerName}' not found in the prefab.");
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            return;
        }

        // Find all source units in the container
        int count = 0;
        foreach (Transform sourceUnitTransform in prefabUnitContainer)
        {
            GameObject sourceUnit = sourceUnitTransform.gameObject;
            GameObject sourceUnitClone = prefabUnitContainerClone.GetChild(count).gameObject;
            if (sourceUnit == null)
                continue;

            // Instantiate a new unit prefab in the unit container
            //GameObject newUnit = (GameObject)PrefabUtility.InstantiatePrefab(unitPrefab, prefabUnitContainer);

            //// Copy position, rotation, and all components from the source unit to the new unit
            //newUnit.transform.position = sourceUnit.transform.position;
            //newUnit.transform.rotation = sourceUnit.transform.rotation;
            PrefabUtility.ConnectGameObjectToPrefab(sourceUnit, unitPrefab);
            // Copy all components
            //CopyComponents(sourceUnitClone, sourceUnit);

            //sourceUnit.GetComponent<Unit>().AssignReder();
            //sourceUnit.GetComponent<Room>().EditorAssign();
            //CloneRoom(sourceUnitClone, sourceUnit);
            ClonePosition(sourceUnitClone, sourceUnit);
            count++;
        }

        // Apply changes to the prefab
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
      
        Debug.Log("Prefab level updated successfully.");
    }

    void CopyComponents(GameObject source, GameObject destination)
    {
        var srcComponents = source.GetComponents<Component>();
        foreach (var srcComponent in srcComponents)
        {
            if (srcComponent is Transform)
                continue;

            var destComponent = destination.GetComponent(srcComponent.GetType());
            if (destComponent == null)
            {
                destComponent = destination.AddComponent(srcComponent.GetType());
            }

            EditorUtility.CopySerialized(srcComponent, destComponent);
        }
    }
    void CloneRoom(GameObject source, GameObject destination)
    {
         int sourcBlockCount = source.transform.GetChild(0).childCount -1;
        for (int i = 0; i < sourcBlockCount; i++)
        { 
            GameObject newUnit = (GameObject)PrefabUtility.InstantiatePrefab(BlockPrefab, destination.transform.GetChild(0));
        }
        sourcBlockCount += 1;
        for (int i = 0; i < sourcBlockCount; i++)
        {
           Transform sourceUnit = source.transform.GetChild(0).GetChild(i);
           Transform destinationUnit = destination.transform.GetChild(0).GetChild(i);

            destinationUnit.transform.localPosition = sourceUnit.transform.localPosition;
            destinationUnit.transform.localRotation = sourceUnit.transform.localRotation;
            destinationUnit.GetComponent<Block>().EditorCodenameType(destinationUnit.GetComponent<Block>().GetCodeNameType);
        }
    }
    void ClonePosition(GameObject source, GameObject destination)
    {
        destination.transform.localPosition = source.transform.localPosition;
        destination.transform.localRotation = source.transform.localRotation;
    }
}
