using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootSequencer : MonoBehaviour
{
    [SerializeField] private GameObject[] gameManagerRefs;
    [SerializeField] private GameObject playerRef;
    private GameObject player;
    private Dictionary<Type, IGameManager> managers = new Dictionary<Type, IGameManager>();

    public string baseSceneName;

    private void Start()
    {
        Initialize();
    }

    private async void Initialize()
    {
        await InstantiateManagers();
        await InstantiatePlayer();
        await InitializeManagers();

        await LoadGameWorld();
        await SetupInitialRoom();

        await ActivatePlayer();
        await InitializePlayer();

        Destroy(gameObject);
    }

    private async Task<GameObject> InstantiateSingleAsync(GameObject prefab, Transform parent = null)
    {
        var results = await InstantiateAsync(prefab, 1, parent);
        return results[0];
    }

    private async Task InstantiatePlayer()
    {
        var playerManager = GetManager<PlayerManager>(); // Using a helper method
        player = await InstantiateSingleAsync(playerRef, playerManager.gameObject.transform);
        player.name = "Atlas";
        player.SetActive(false);
    }

    // Helper method to make manager access cleaner
    private T GetManager<T>() where T : class, IGameManager
    {
        if (managers.TryGetValue(typeof(T), out var manager))
        {
            return manager as T;
        }
        throw new InvalidOperationException($"Manager of type {typeof(T).Name} not found. Ensure it's properly instantiated.");
    }

    private async Task ActivatePlayer()
    {
        player.SetActive(true);
        await Task.CompletedTask;
    }

    private async Task SetupInitialRoom()
    {
        await SceneManager.LoadSceneAsync(baseSceneName, LoadSceneMode.Additive);
        await Task.CompletedTask;
    }

    private async Task LoadGameWorld()
    {
        await Task.CompletedTask;
    }

    private async Task InitializePlayer()
    {
        /*
            Look for a player spawn point object
            I think we just do this in Tiled
            We can maybe go as far as to load the level
            which contains the player spawn point
            Or maybe we make several options
            - load current level
            - use tiled spawn point
            - spawn at specific location
            - etc
        */
        StateMachine stateMachine = player.GetComponentInChildren<StateMachine>();
        StateRegistry stateRegistry = stateMachine.stateRegistry;
        AtlasTransitionManager stateTransition = stateMachine.stateTransitions as AtlasTransitionManager;

        /*
            Initialize State Repository
            Initialize Transitions
            Initialize StateMachine
        */
        await stateRegistry.Init();
        await stateTransition.Init();
        await stateMachine.Init();

        await Task.CompletedTask;
    }
    
    private async Task InstantiateManagers() 
{
    // Make sure dictionary is initialized
    if (managers == null)
        managers = new Dictionary<Type, IGameManager>();
    
    await Task.WhenAll(gameManagerRefs.Select(async gm => {
        GameObject manager = await InstantiateSingleAsync(gm);
        
        // Get the IGameManager component
        if (!manager.TryGetComponent<IGameManager>(out var comp))
        {
            throw new Exception($"GameObject {gm.name} does not have a component implementing IGameManager");
        }
        
        manager.name = gm.name;
        
        // Use the actual component type as the key
        Type managerType = comp.GetType();
        
        // Check for duplicates - this prevents accidentally registering the same manager type twice
        if (managers.ContainsKey(managerType))
        {
            Debug.LogWarning($"Manager of type {managerType.Name} already exists. Overwriting.");
        }
        
        managers[managerType] = comp; // Using indexer instead of Add() handles duplicates gracefully
        
        return manager;
    }));
}

    private async Task InitializeManagers()
    {
        await Task.WhenAll(managers.Values.Select(async gm => await gm.Init()));
    }
}
