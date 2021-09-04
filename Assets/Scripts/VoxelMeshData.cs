using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New VoxelMeshData", menuName = "Minecraft/VoxelMeshData")]
public class VoxelMeshData : ScriptableObject
{
    public string blockName;
    public FaceMeshData[] faces;
}

[System.Serializable]
public class VertData {

    public Vector3 position;
    public Vector2 uv;

    public VertData(Vector3 _position, Vector2 _uv)
    {
        position = _position;
        uv = _uv;
    }

    public Vector3 GetRotatedPosition(Vector3 angles)
    {
        Vector3 center = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 direction = position - center;
        direction = Quaternion.Euler(angles) * direction;
        return direction + center;
    }
}

[System.Serializable]
public class FaceMeshData
{
    public string direction;
    public VertData[] vertData;
    public int[] triangles;

    public VertData GetVertData(int index)
    {
        return vertData[index];
    }
}