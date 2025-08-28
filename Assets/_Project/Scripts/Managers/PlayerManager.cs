using System.Threading.Tasks;
using UnityEngine;

public class PlayerManager : MonoBehaviour, IGameManager
{
    public static PlayerManager Instance;

    public Task Init() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return Task.CompletedTask;
        }
        
        Instance = this;
        return Task.CompletedTask;
    }

    public void ShowStateDisplay(bool on = true)
    {

    }
}