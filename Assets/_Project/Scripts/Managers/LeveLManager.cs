using System;
using System.Threading.Tasks;
using UnityEngine;

public class LevelManager : MonoBehaviour, IGameManager
{
    public static LevelManager Instance;
    public GameObject levelObject;
    public GameObject cameraPrefab;

    private void Awake()
    {
        if (cameraPrefab == null)
        {
            Debug.LogError("Camera prefab not assigned to LevelManager");
        }
    }

    public Task Init()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return Task.CompletedTask;
        }

        Instance = this;
        return Task.CompletedTask;
    }

    public GameObject InstantiateLevel(int x, int y)
    {
        var levelPrefab = Resources.Load<GameObject>($"Tilemaps/gameworld/Level_{x}_{y}");

        if (!levelPrefab)
        {
            Debug.LogWarning($"Failed to load Level_{x}_{y}");
            return null;
        }

        // Clean up previous level properly
        if (levelObject != null)
        {
            DestroyImmediate(levelObject);
            levelObject = null;
        }

        levelObject = Instantiate(levelPrefab, transform);

        var cameraObject = Instantiate(cameraPrefab, levelObject.transform);
        if (levelObject.TryGetComponent<PolygonCollider2D>(out var levelBoundsPoly))
        {
            if (cameraObject.TryGetComponent<CameraController>(out var cameraController))
            {
                cameraController.roomBounds = levelBoundsPoly;
                cameraController.setBounds(levelBoundsPoly);
                return levelObject;
            }
            else
            {
                throw new Exception("Camera Prefab does not have a CameraController component!");
            }
        }
        else
        {
            throw new Exception($"LevelObject {levelObject.name} does not have a PolygonCollider2D component!");
        }
    }
}