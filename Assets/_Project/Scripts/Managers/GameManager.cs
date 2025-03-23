using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerManger playerManger;
    public SoundManager soundManager;

    public PlayerController GetPlayer()
    {
        return playerManger.GetPlayer();
    }
}