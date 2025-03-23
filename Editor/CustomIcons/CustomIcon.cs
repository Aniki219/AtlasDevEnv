using UnityEngine;

public class CustomIcon : MonoBehaviour, IHierarchyIcon
{
    public Sprite sprite;
    public string EditorIconPath { get { return sprite.name; } }
}
