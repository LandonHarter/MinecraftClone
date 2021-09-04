using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clouds : MonoBehaviour
{

    public int cloudHeight = 100;
    public int cloudDepth = 1;

    [SerializeField] private Texture2D cloudPattern = null;
    [SerializeField] private Material cloudMaterial = null;
    [SerializeField] private World world = null;
    bool[,] cloudData;

    public Settings settings;

    int cloudTexWidth;

    int cloudTileSize;
    Vector3Int offset;

    Dictionary<Vector2Int, GameObject> clouds = new Dictionary<Vector2Int, GameObject>();

    private void Start()
    {

        cloudTexWidth = cloudPattern.width;
        cloudTileSize = VoxelData.ChunkWidth;
        offset = new Vector3Int(-(cloudTexWidth / 2), 0, -(cloudTexWidth / 2));

        transform.position = new Vector3(VoxelData.worldCenter, cloudHeight, VoxelData.worldCenter);

        LoadCloudData();
        CreateClouds();

    }

    private void LoadCloudData()
    {

        cloudData = new bool[cloudTexWidth, cloudTexWidth];
        Color[] cloudTex = cloudPattern.GetPixels();

        for (int x = 0; x < cloudTexWidth; x++)
        {
            for (int y = 0; y < cloudTexWidth; y++)
            {

                cloudData[x, y] = (cloudTex[y * cloudTexWidth + x].a > 0);

            }
        }
    }

    private void CreateClouds()
    {
        for (int x = 0; x < cloudTexWidth; x += cloudTileSize)
        {
            for (int y = 0; y < cloudTexWidth; y += cloudTileSize)
            {
                Mesh cloudMesh;
                if (settings.style == CloudStyle.Fast)
                    cloudMesh = CreateFastCloudMesh(x, y);
                else
                    cloudMesh = CreateFancyCloudMesh(x, y);

                Vector3 position = new Vector3(x, cloudHeight, y);
                position += transform.position - new Vector3(cloudTexWidth / 2f, 0f, cloudTexWidth / 2f);
                position.y = cloudHeight;
                clouds.Add(CloudTilePosFromV3(position), CreateCloudTile(cloudMesh, position));

            }
        }
    }

    public void UpdateClouds()
    {

        for (int x = 0; x < cloudTexWidth; x += cloudTileSize)
        {
            for (int y = 0; y < cloudTexWidth; y += cloudTileSize)
            {

                Vector3 position = world.player.position + new Vector3(x, 0, y) + offset;
                position = new Vector3(RoundToCloud(position.x), cloudHeight, RoundToCloud(position.z));
                Vector2Int cloudPosition = CloudTilePosFromV3(position);

                clouds[cloudPosition].transform.position = position;

            }
        }
    }

    private int RoundToCloud(float value)
    {

        return Mathf.FloorToInt(value / cloudTileSize) * cloudTileSize;

    }

    private Mesh CreateFastCloudMesh(int x, int z)
    {

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;

        for (int xIncrement = 0; xIncrement < cloudTileSize; xIncrement++)
        {
            for (int zIncrement = 0; zIncrement < cloudTileSize; zIncrement++)
            {

                int xVal = x + xIncrement;
                int zVal = z + zIncrement;

                if (cloudData[xVal, zVal])
                {
                    vertices.Add(new Vector3(xIncrement, 0, zIncrement));
                    vertices.Add(new Vector3(xIncrement, 0, zIncrement + 1));
                    vertices.Add(new Vector3(xIncrement + 1, 0, zIncrement + 1));
                    vertices.Add(new Vector3(xIncrement + 1, 0, zIncrement));

                    for (int i = 0; i < 4; i++)
                        normals.Add(Vector3.down);

                    triangles.Add(vertCount + 1);
                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 2);

                    triangles.Add(vertCount + 2);
                    triangles.Add(vertCount);
                    triangles.Add(vertCount + 3);
                    
                    vertCount += 4;

                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;

    }

    public Mesh CreateFancyCloudMesh(int x, int z)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        int vertCount = 0;

        for (int xIncrement = 0; xIncrement < cloudTileSize; xIncrement++)
        {
            for (int zIncrement = 0; zIncrement < cloudTileSize; zIncrement++)
            {

                int xVal = x + xIncrement;
                int zVal = z + zIncrement;

                if (cloudData[xVal, zVal])
                {
                    for (int p = 0; p < 6; p++)
                    {

                        if (!CheckCloudData(new Vector3Int(xVal, 0, zVal) + VoxelData.faceChecks[p]))
                        {
                            for (int i = 0; i < 4; i++)
                            {

                                Vector3 vert = new Vector3Int(xIncrement, 0, zIncrement);
                                vert += VoxelData.voxelVerts[VoxelData.voxelTris[p, i]];
                                vert.y *= cloudDepth;
                                vertices.Add(vert);

                            }

                            for (int i = 0; i < 4; i++)
                                normals.Add(VoxelData.faceChecks[p]);

                            triangles.Add(vertCount);
                            triangles.Add(vertCount + 1);
                            triangles.Add(vertCount + 2);
                            triangles.Add(vertCount + 2);
                            triangles.Add(vertCount + 1);
                            triangles.Add(vertCount + 3);

                            vertCount += 4;

                        }
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;
    }

    private bool CheckCloudData(Vector3 point)
    {
        if (point.y != 0)
            return false;

        int x = (int)point.x;
        int z = (int)point.z;
        if (point.x < 0) x = cloudTexWidth - 1;
        if (point.x > cloudTexWidth - 1) x = 0;
        if (point.z < 0) z = cloudTexWidth - 1;
        if (point.z > cloudTexWidth - 1) z = 0;

        return cloudData[x, z];
    }

    private GameObject CreateCloudTile(Mesh mesh, Vector3 position)
    {

        GameObject newCloudTile = new GameObject();
        newCloudTile.transform.position = position;
        newCloudTile.transform.parent = transform;
        newCloudTile.name = "Cloud " + position.x + ", " + position.z;
        MeshFilter mF = newCloudTile.AddComponent<MeshFilter>();
        MeshRenderer mR = newCloudTile.AddComponent<MeshRenderer>();

        mR.material = cloudMaterial;
        mF.mesh = mesh;

        return newCloudTile;

    }

    private Vector2Int CloudTilePosFromV3(Vector3 pos)
    {

        return new Vector2Int(CloudTileCoordFromFloat(pos.x), CloudTileCoordFromFloat(pos.z));

    }

    private int CloudTileCoordFromFloat(float value)
    {

        float a = value / (float)cloudTexWidth;
        a -= Mathf.FloorToInt(a); 
        int b = Mathf.FloorToInt((float)cloudTexWidth * a); 

        return b;
    }
}

public enum CloudStyle
{
    Fast,
    Fancy
}