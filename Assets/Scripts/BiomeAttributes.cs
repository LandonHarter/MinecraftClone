using UnityEngine;

[CreateAssetMenu(fileName = "BiomeAttributes", menuName = "Minecraft/Biome Attribute")]
public class BiomeAttributes : ScriptableObject {

    [Header("Basic Biome Information")]
    public string biomeName;
    public float offset;
    public float scale;

    [Space]

    [Header("Terrain Settings")]
    public int solidGroundHeight;
    public int terrainHeight;
    public float terrainScale;

    public byte surfaceBlock;
    public byte subSurfaceBlock;
    public int subsurfaceBlockHeight;

    [Space]

    [Header("Block Spawning")]
    public Lode[] lodes;

    [Space]

    [Header("Major Flora")]
    public int[] majorFloraIndex;
    public float majorFloraZoneScale = 1.3f;
    [Range(0.1f, 1)]
    public float majorFloraZoneThreshold = 0.6f;
    public float majorFloraPlacementScale = 15f;
    [Range(0.1f, 1)]
    public float majorFloraPlacementThreshold = 0.8f;

    public int minHeight = 5;
    public int maxHeight = 12;

    public bool placeMajorFlora = true;
}

[System.Serializable]
public class Lode {

    public string nodeName;
    public byte blockID;
    public int minHeight;
    public int maxHeight;
    public float scale;
    public float threshold;
    public float noiseOffset;


}
