using UnityEngine;

public class TerrainLinker : MonoBehaviour
{
    [SerializeField]
    Terrain terrainA;
    [SerializeField]
    Terrain terrainB;
    [SerializeField]
    Terrain terrainC;

    void Awake()
    {
        terrainA.SetNeighbors(null, terrainB, null, null);
        terrainB.SetNeighbors(null, terrainC, null, terrainA);
        terrainC.SetNeighbors(null, null, null, terrainB);
    }
}