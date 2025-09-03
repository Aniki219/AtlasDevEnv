using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

[System.Serializable]
public class AtlasStateMonitor : EditorWindow
{
    private GameObject atlasGameObject;
    private StateMachine stateMachine;
    private Vector2 scrollPosition;
    private bool showStateBehaviors = true;
    private bool showStateTransitions = true;
    private bool showStates = true;
    
    private List<GameObject> activeStateBehaviors = new List<GameObject>();
    private List<StateType> activeStateTransitions = new List<StateType>();
    private List<GameObject> activeStates = new List<GameObject>();

    [MenuItem("Tools/Atlas State Monitor")]
    public static void ShowWindow()
    {
        GetWindow<AtlasStateMonitor>("Atlas State Monitor");
    }

    private void OnEnable()
    {
        EditorApplication.update += UpdateMonitor;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateMonitor;
    }

    private void UpdateMonitor()
    {
        if (Application.isPlaying)
        {
            FindAtlasStateMachine();
            UpdateActiveComponents();
            Repaint();
        }
    }

    private void FindAtlasStateMachine()
    {
        var pc = PlayerController.Instance;

        if (!pc) return;

        atlasGameObject = pc.gameObject;

        if (!atlasGameObject) return;

        stateMachine = atlasGameObject.GetComponentInChildren<StateMachine>();
    }

    private void UpdateActiveComponents()
    {
        if (stateMachine == null) return;

        activeStates = stateMachine
            .GetComponentsInChildren<State>()
            .Select(stateComponent => stateComponent.gameObject)
            .ToList();

        activeStateBehaviors = stateMachine.GetComponentsInChildren<StateBehavior>()
            .Select(beh => beh.gameObject)
            .ToList();

        AtlasStateTransitions stateTransitions = (AtlasStateTransitions) stateMachine.stateTransitions;
        activeStateTransitions = stateTransitions.CanTransitions[stateMachine.currentState.stateType].ToList();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Atlas StateMachine Monitor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("This monitor only works during Play Mode", MessageType.Info);
            return;
        }

        if (atlasGameObject == null)
        {
            EditorGUILayout.HelpBox("GameObject 'Atlas' not found in the scene", MessageType.Warning);
            return;
        }

        if (stateMachine == null)
        {
            EditorGUILayout.HelpBox("StateMachine component not found on Atlas GameObject", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField($"Monitoring: {atlasGameObject.name}.{stateMachine.GetType().Name}", EditorStyles.helpBox);
        EditorGUILayout.Space();

        // Toggle sections
        EditorGUILayout.BeginHorizontal();
        showStates = EditorGUILayout.Toggle("States", showStates);
        showStateBehaviors = EditorGUILayout.Toggle("StateBehaviors", showStateBehaviors);
        showStateTransitions = EditorGUILayout.Toggle("StateTransitions", showStateTransitions);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Display State components
        if (showStates)
        {
            EditorGUILayout.LabelField($"Active State Components ({activeStates.Count})", EditorStyles.boldLabel);
            if (activeStates.Count == 0)
            {
                EditorGUILayout.LabelField("  None found", EditorStyles.miniLabel);
            }
            else
            {
                foreach (GameObject go in activeStates)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  • {go.name}");
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = go;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.Space();
        }

        // Display StateBehavior components
        if (showStateBehaviors)
        {
            EditorGUILayout.LabelField($"Active StateBehavior Components ({activeStateBehaviors.Count})", EditorStyles.boldLabel);
            if (activeStateBehaviors.Count == 0)
            {
                EditorGUILayout.LabelField("  None found", EditorStyles.miniLabel);
            }
            else
            {
                foreach (GameObject go in activeStateBehaviors)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  • {go.name}");
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = go;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.Space();
        }

        // Display StateTransition components
        if (showStateTransitions)
        {
            EditorGUILayout.LabelField($"Active StateTransition Components ({activeStateTransitions.Count})", EditorStyles.boldLabel);
            if (activeStateTransitions.Count == 0)
            {
                EditorGUILayout.LabelField("  None found", EditorStyles.miniLabel);
            }
            else
            {
                foreach (StateType stateType in activeStateTransitions)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  • {stateType}");
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.Space();
        }


        EditorGUILayout.EndScrollView();
    }
}