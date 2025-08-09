using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

// Alternative: Menu item that runs directly without opening a window
public class StateTypeAssignerMenu
{
    [MenuItem("Tools/Assign State Types (Quick)")]
    public static void AssignStateTypesQuick()
    {
        State[] allStates = (State[])Resources.FindObjectsOfTypeAll(typeof(State));

        if (allStates.Length == 0)
        {
            Debug.LogWarning("No State components found in the current scene.");
            return;
        }

        int assigned = 0;
        int failed = 0;
        string[] stateTypeNames = Enum.GetNames(typeof(StateType));

        foreach (State state in allStates)
        {
            string gameObjectName = state.gameObject.name;

            if (!gameObjectName.StartsWith("st_"))
            {
                failed++;
                continue;
            }

            string stateNamePart = gameObjectName.Substring(3);

            if (string.IsNullOrEmpty(stateNamePart))
            {
                failed++;
                continue;
            }

            string matchingEnumName = stateTypeNames.FirstOrDefault(name =>
                string.Equals(name, stateNamePart, StringComparison.OrdinalIgnoreCase));

            if (matchingEnumName != null && Enum.TryParse<StateType>(matchingEnumName, out StateType stateType))
            {
                Undo.RecordObject(state, $"Set StateType for {gameObjectName}");
                state.stateType = stateType;
                EditorUtility.SetDirty(state);
                assigned++;
            }
            else
            {
                failed++;
            }
        }

        Debug.Log($"Quick State Type Assignment Complete! Assigned: {assigned}/{allStates.Length}, Failed: {failed}");

        if (assigned > 0)
        {
            Debug.Log("Don't forget to save your scene to persist the changes!");
        }
    }
}