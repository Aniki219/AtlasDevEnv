using System.Threading.Tasks;
using UnityEngine;

public class LevelManager : MonoBehaviour, IGameManager
{
    public static LevelManager Instance;
    public GameObject levelObject;

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

    public bool SetLevelObject(int x, int y, out GameObject levelObject)
    {
        Debug.Log("setting level object");
        var levelResource = Resources.Load<GameObject>($"Tilemaps/gameworld/Level_{x}_{y}");
        if (levelResource)
        {
            Debug.Log("loaded non null level resource");
            levelObject = levelResource as GameObject;
            return true;
        }
        levelObject = null;
        Debug.LogWarning($"Failed to load Level_{x}_{y}");
        return false;
    }

    public void InstantiateLevel(GameObject level)
    {
        Destroy(levelObject);
        levelObject = Instantiate(level, transform);
    }
}