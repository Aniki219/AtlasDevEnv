using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootSequencer : MonoBehaviour
{
    [SerializeField] private GameObject[] gameManagerRefs;
    [SerializeField] private GameObject playerRef;
    private GameObject player;
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

        await UnloadBootScene();
    }

    private async Task ActivatePlayer()
    {
        player.SetActive(true);
        await Task.CompletedTask;
    }

    private async Task InstantiatePlayer()
    {
        var p = await InstantiateAsync(playerRef);
        player = p[0];
        player.name = "Atlas";
        player.SetActive(false);
        DontDestroyOnLoad(player);
    }

    private async Task UnloadBootScene()
    {
        await SceneManager.UnloadSceneAsync("Boot");
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
        StateRegistry stateRegistry = stateMachine.GetComponent<StateRegistry>();
        AtlasStateTransitions stateTransition = stateMachine.GetComponent<AtlasStateTransitions>();
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

    private async Task InstantiateManagers() {
        await Task.WhenAll(gameManagerRefs.Select(async gm => {
                var go = await InstantiateAsync(gm);
                GameObject manager = go[0];
                manager.name = gm.name;
                DontDestroyOnLoad(manager);
                return manager;
            })
        );
    }

    private async Task InitializeManagers()
    {
        var gameManagers = gameManagerRefs
            .Select(go => {
                if (go.TryGetComponent<IGameManager>(out var manager)) {
                    return manager;
                } else {
                    throw new Exception(
                        $"Registered GameManager instance {go.name} does not implement IGameManager"
                    );
                }
            });

        await Task.WhenAll(gameManagers.Select(async gm => await gm.Init()));
    }
}
