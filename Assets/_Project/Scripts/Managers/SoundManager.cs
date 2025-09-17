using System.Threading.Tasks;
using UnityEngine;

public class SoundManager : MonoBehaviour, IGameManager
{
    public static SoundManager Instance;
    
    public Task Init() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return Task.CompletedTask;
        }
        
        Instance = this;
        return Task.CompletedTask;
    }
}