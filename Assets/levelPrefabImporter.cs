using System.Linq;
using LDtkUnity;
using UnityEngine;

public class levelPrefabImporter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var layers = GetComponentsInChildren<LDtkComponentLayer>();
        var wallsLayer = LayerMask.NameToLayer("Solid");
        layers
            .Where(ld => ld.LayerDef.Identifier == "Walls")
            .SelectMany(l => l.GetComponentsInChildren<Transform>())
            .ToList()
            .ForEach(ld => ld.gameObject.layer = wallsLayer);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
