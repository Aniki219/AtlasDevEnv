using UnityEngine;

public class EntityContext : MonoBehaviour
{
    public SpriteController sprite;
    public EntityBody body;
    public EntityController controller;
    public PlayerCanvasController canvas;
    public InputManager input;
    public StateMachine stateMachine;
}