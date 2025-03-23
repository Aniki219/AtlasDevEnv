using UnityEngine;

public class PlayerManger : MonoBehaviour
{
    [SerializeField] PlayerController player;

    public PlayerController GetPlayer()
    {
        return player;
    }

    public int GetPlayerFacing()
    {
        return player.facing;
    }

    public void ShowStateDisplay(bool on = true)
    {

    }
}