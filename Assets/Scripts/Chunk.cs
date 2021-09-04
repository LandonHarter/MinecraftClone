using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Chunk
{

    public ChunkCoord coord;

    GameObject chunkObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;

    int vertexIndex = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<int> transparentTriangles = new List<int>();
    Material[] materials = new Material[2];
    List<Vector2> uvs = new List<Vector2>();
    List<Color> colors = new List<Color>();
    List<Vector3> normals = new List<Vector3>();

    public Vector3 position;

    private bool _isActive;

    ChunkData chunkData;

    public Chunk(ChunkCoord _coord)
    {
        coord = _coord;
    }

    public void Init()
    {
        chunkObject = new GameObject();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        materials[0] = World.Instance.material;
        materials[1] = World.Instance.transparentMaterial;
        meshRenderer.materials = materials;

        chunkObject.transform.SetParent(World.Instance.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObject.name = "Chunk " + coord.x + ", " + coord.z;
        position = chunkObject.transform.position;

        chunkData = World.Instance.worldData.RequestChunk(new Vector2Int((int)position.x, (int)position.z), true);

        lock (World.Instance.ChunkUpdateThreadLock)
        {
            World.Instance.chunksToUpdate.Add(this);
        }
    }

    public void UpdateChunk()
    {
        ClearMeshData();
        if (World.Instance.lightStyle == LightStyle.Fast)
            CalculateLight();

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (World.Instance.blocktypes[chunkData.map[x, y, z].id].isSolid)
                        UpdateMeshData(new Vector3(x, y, z));
                }
            }
        }

        World.Instance.chunksToDraw.Enqueue(this);
    }

    void CalculateLight()
    {
        Queue<Vector3Int> litVoxels = new Queue<Vector3Int>();

        for (int x = 0; x < VoxelData.ChunkWidth; x++)
        {
            for (int z = 0; z < VoxelData.ChunkWidth; z++)
            {
                float lightRay = 1f;

                for (int y = VoxelData.ChunkHeight - 1; y >= 0; y--)
                {

                    VoxelState thisVoxel = chunkData.map[x, y, z];

                    if (thisVoxel.id > 0 && World.Instance.blocktypes[thisVoxel.id].transparency < lightRay)
                        lightRay = World.Instance.blocktypes[thisVoxel.id].transparency;

                    thisVoxel.globalLightPercent = lightRay;

                    chunkData.map[x, y, z] = thisVoxel;

                    if (lightRay > VoxelData.lightFallOff)
                        litVoxels.Enqueue(new Vector3Int(x, y, z));
                }
            }
        }

        while (litVoxels.Count > 0)
        {
            Vector3Int v = litVoxels.Dequeue();

            for (int p = 0; p < 6; p++)
            {
                Vector3 currentVoxel = v + VoxelData.faceChecks[p];
                Vector3Int neighbor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

                if (IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z))
                {
                    if (chunkData.map[neighbor.x, neighbor.y, neighbor.z].globalLightPercent < chunkData.map[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFallOff)
                    {
                        chunkData.map[neighbor.x, neighbor.y, neighbor.z].globalLightPercent = chunkData.map[v.x, v.y, v.z].globalLightPercent - VoxelData.lightFallOff;

                        if (chunkData.map[neighbor.x, neighbor.y, neighbor.z].globalLightPercent > VoxelData.lightFallOff)
                            litVoxels.Enqueue(neighbor);
                    }
                }
            }
        }
    }

    void ClearMeshData()
    {

        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        transparentTriangles.Clear();
        uvs.Clear();
        colors.Clear();
        normals.Clear();
    }

    public bool isActive
    {

        get { return _isActive; }
        set
        {

            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);

        }

    }

    bool IsVoxelInChunk(int x, int y, int z)
    {

        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0 || z > VoxelData.ChunkWidth - 1)
            return false;
        else
            return true;

    }

    public void EditVoxel(Vector3 pos, byte newID, int direction)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        chunkData.map[xCheck, yCheck, zCheck].id = newID;
        chunkData.map[xCheck, yCheck, zCheck].orientation = direction;
        World.Instance.worldData.AddToModifiedChunkList(chunkData);

        lock (World.Instance.ChunkUpdateThreadLock)
        {
            World.Instance.chunksToUpdate.Insert(0, this);
            UpdateSurroundingVoxels(xCheck, yCheck, zCheck);
        }
    }

    void UpdateSurroundingVoxels(int x, int y, int z)
    {

        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
        {

            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];

            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {

                World.Instance.chunksToUpdate.Insert(0, World.Instance.GetChunkFromVector3(currentVoxel + position));

            }

        }

    }

    VoxelState CheckVoxel(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (!IsVoxelInChunk(x, y, z))
            return World.Instance.GetVoxelState(pos + position);

        return chunkData.map[x, y, z];

    }

    public VoxelState GetVoxelFromGlobalVector3(Vector3 pos)
    {

        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(position.x);
        zCheck -= Mathf.FloorToInt(position.z);

        return chunkData.map[xCheck, yCheck, zCheck];

    }

    void UpdateMeshData(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        byte blockID = chunkData.map[x, y, z].id;
        VoxelState voxel = chunkData.map[x, y, z];

        float rotation = 0;
        switch (chunkData.map[x, y, z].orientation)
        {
            case 0:
                rotation = 180f;
                break;
            case 5:
                rotation = 270f;
                break;
            case 1:
                rotation = 0f;
                break;
            default:
                rotation = 90f;
                break;
        }

        for (int p = 0; p < 6; p++)
        {
            int translatedP = p;
            if (voxel.orientation != 1)
            {
                if (voxel.orientation == 0)
                {
                    if (p == 0) { translatedP = 1; }
                    else if (p == 1) { translatedP = 0; }
                    else if (p == 4) { translatedP = 5; }
                    else if (p == 5) { translatedP = 4; }
                }
                else if (voxel.orientation == 5)
                {
                    if (p == 0) { translatedP = 5; }
                    else if (p == 1) { translatedP = 4; }
                    else if (p == 4) { translatedP = 0; }
                    else if (p == 5) { translatedP = 1; }
                }
                else if (voxel.orientation == 4)
                {
                    if (p == 0) { translatedP = 4; }
                    else if (p == 1) { translatedP = 5; }
                    else if (p == 4) { translatedP = 1; }
                    else if (p == 5) { translatedP = 0; }
                }
            }

            VoxelState neighbor = CheckVoxel(pos + VoxelData.faceChecks[translatedP]);

            if (neighbor != null && World.Instance.blocktypes[neighbor.id].renderNeighborFaces)
            {
                int faceVertCount = 0;

                for (int i = 0; i < World.Instance.blocktypes[blockID].meshData.faces[p].vertData.Length; i++)
                {
                    VertData vertData = World.Instance.blocktypes[blockID].meshData.faces[p].GetVertData(i);
                    vertices.Add(pos + vertData.GetRotatedPosition(new Vector3(0, rotation, 0)));
                    normals.Add(VoxelData.faceChecks[p]);
                    colors.Add(new Color(0, 0, 0, 255));
                    AddTexture(World.Instance.blocktypes[blockID].GetTextureID(p), vertData.uv);
                    faceVertCount++;
                }

                if (!World.Instance.blocktypes[blockID].renderNeighborFaces)
                {
                    for (int i = 0; i < World.Instance.blocktypes[blockID].meshData.faces[p].triangles.Length; i++)
                    {
                        triangles.Add(vertexIndex + World.Instance.blocktypes[blockID].meshData.faces[p].triangles[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < World.Instance.blocktypes[blockID].meshData.faces[p].triangles.Length; i++)
                    {
                        transparentTriangles.Add(vertexIndex + World.Instance.blocktypes[blockID].meshData.faces[p].triangles[i]);
                    }
                }

                vertexIndex += faceVertCount;
            }
        }

    }

    public void CreateMesh()
    {

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();

        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.SetTriangles(transparentTriangles.ToArray(), 1);

        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();
        mesh.normals = normals.ToArray();
        
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;

    }

    void AddTexture(int textureID, Vector2 uv)
    {
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        x += VoxelData.NormalizedBlockTextureSize * uv.x;
        y += VoxelData.NormalizedBlockTextureSize * uv.y;

        uvs.Add(new Vector2(x, y));
    }
}

public class ChunkCoord
{

    public int x;
    public int z;

    public ChunkCoord()
    {

        x = 0;
        z = 0;

    }

    public ChunkCoord(int _x, int _z)
    {

        x = _x;
        z = _z;

    }

    public ChunkCoord(Vector3 pos)
    {

        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;

    }

    public bool Equals(ChunkCoord other)
    {

        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;

    }

}

[System.Serializable]
public class VoxelState
{

    public byte id;
    public int orientation;
    public float globalLightPercent;

    public VoxelState()
    {
        id = 0;
        orientation = 1;
        globalLightPercent = 0f;
    }

    public VoxelState(byte _id)
    {
        id = _id;
        orientation = 1;
        globalLightPercent = 0f;
    }
}