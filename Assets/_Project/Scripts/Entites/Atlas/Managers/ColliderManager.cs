using UnityEngine;
using System;
using System.Collections.Generic;

public class ColliderManager : MonoBehaviour
{
    [SerializeField] private List<BoxCollider2D> HitboxColliders = new List<BoxCollider2D>();
    [SerializeField] private List<BoxCollider2D> SpecialColliders = new List<BoxCollider2D>();

    public BoxCollider2D getCollider()
    {
        if (HitboxColliders.Count == 0) throw new Exception("ColliderManager contains no HitboxColliders");
        BoxCollider2D foundCollider = HitboxColliders.Find(col => col.enabled);
        if (foundCollider == null) return HitboxColliders[0];
        return foundCollider;
    }

    public BoxCollider2D getCollider(string name, bool hitbox = false)
    {
        List<BoxCollider2D> colliders = hitbox ? HitboxColliders : getAllColliders();
        BoxCollider2D foundCollider = colliders.Find(col => col.name.Equals(name));
        if (foundCollider == null) throw new Exception("No collider with name: " + name);
        return foundCollider;
    }

    private List<BoxCollider2D> getAllColliders()
    {
        List<BoxCollider2D> colliders = new List<BoxCollider2D>();
        colliders.AddRange(HitboxColliders);
        colliders.AddRange(SpecialColliders);
        return colliders;
    }

    public BoxCollider2D setActiveCollider(string activeColliderName)
    {
        foreach (BoxCollider2D col in HitboxColliders)
        {
            col.enabled = false;
        }
        BoxCollider2D foundCollider = getCollider(activeColliderName, true);

        foundCollider.enabled = true;
        return foundCollider;
    }

    public void enableColliders(params string[] BoxCollider2Ds)
    {
        foreach (string name in BoxCollider2Ds)
        {
            bool foundActiveCollider = false;
            BoxCollider2D foundCollider = getCollider(name);
            if (HitboxColliders.Contains(foundCollider))
            {
                setActiveCollider(foundCollider.name);

                if (foundActiveCollider) throw new Exception("Multiple Hitbox Colliders enabled at once.");
                foundActiveCollider = true;
            }
            else
            {
                foundCollider.enabled = true;
            }
        }
    }

    public void disableColliders(params string[] colliderNames)
    {
        foreach (string name in colliderNames)
        {
            getCollider(name).enabled = false;
        }
    }

    public void disableCollider(string colliderName)
    {
        disableColliders(colliderName);
    }

    public void enableCollider(string colliderName)
    {
        enableColliders(colliderName);
    }
}