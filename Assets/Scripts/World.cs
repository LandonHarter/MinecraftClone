using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class World : MonoBehaviour
{

    [Header("World Generation Values")]
    public static int seed;
    public BiomeAttributes[] biomes;

    [Header("Performance")]
    public bool enableThreading;

    [Range(0f, 1f)]
    public float globalLightLevel;

    [Header("Day Night Settings")]
    public Color day;
    public Color night;
    public float lightIntensity;
    public float lightMinIntensity;
    public float lightMaxIntensity;
    public float dayLength;
    public float lightTransitionTime;
    public float time = 0;

    [Space]

    public Transform player;
    public Player _player;
    public Toolbar toolbar;
    public GameObject sun;
    public GameObject crafting;
    public Vector3 spawnPosition;

    public Material material;
    public Material transparentMaterial;
    public Material highlightBlockMaterial;

    public GameObject waitingPanel;

    public Clouds clouds;

    public BlockType[] blocktypes;
    public CraftingRecipe[] recipes;
    public ToolRecipes[] toolRecipes;
    public SmeltableObjects[] smeltableObjects;

    Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public ChunkCoord playerChunkCoord;
    ChunkCoord playerLastChunkCoord;

    public List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    public List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<Chunk> chunksToDraw = new Queue<Chunk>();

    bool applyingModifications = false;

    Queue<Queue<VoxelMod>> modifications = new Queue<Queue<VoxelMod>>();

    private bool _inUI = false;

    private static World _instance;
    public static World Instance
    {
        get { return _instance; }
    }

    public GameObject saveWarning;
    public GameObject finishedSaving;

    public GameObject debugScreen;

    public WorldData worldData;

    public string appPath;

    public LoadWorlds worlds;
    public MainMenu mainMenu;

    public GameObject[] playerParts;

    public LightStyle lightStyle;

    Thread ChunkUpdateThread;
    public object ChunkUpdateThreadLock = new object();
    public object ChunkListThreadLock = new object();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }

        appPath = Application.persistentDataPath;

        _player = player.GetComponent<Player>();
    }

    private void Start()
    {
        lightIntensity = lightMinIntensity;
        lightTransitionTime = float.Parse("0.0" + (dayLength / 2).ToString());

        if (!mainMenu.newWorld)
        {
            worldData = SaveSystem.LoadWorld(worldData.worldName);
        }
        else
        {
            worldData = SaveSystem.LoadWorld(mainMenu.worldNameInput.text, mainMenu.seed);
        }

        Random.InitState(seed);

        Shader.SetGlobalFloat("minGlobalLightLevel", VoxelData.minLightLevel);
        Shader.SetGlobalFloat("maxGlobalLightLevel", VoxelData.maxLightLevel);

        if (enableThreading)
        {
            ChunkUpdateThread = new Thread(new ThreadStart(ThreadedUpdate));
            ChunkUpdateThread.Start();
        }

        try
        {
            float[] light = SaveSystem.LoadDayData(worldData.worldName);
            time = light[0];
            lightIntensity = light[1];
        }
        catch
        {
            time = 0;
            lightIntensity = lightMinIntensity;
        }

        mainMenu.generatingWait.SetActive(true);
        lightStyle = Instance.mainMenu.settingsMenu.GetComponent<Settings>().lightStyle;
        spawnPosition = new Vector3((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f, VoxelData.ChunkHeight - 50f, (VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f);
        sun.transform.position = new Vector3(((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f), 150, ((VoxelData.WorldSizeInChunks * VoxelData.ChunkWidth) / 2f));
        GenerateWorld();
        playerLastChunkCoord = GetChunkCoordFromVector3(player.position);
    }

    void SetGlobalLightLevel()
    {
        Shader.SetGlobalFloat("GlobalLightLevel", globalLightLevel);
    }

    public void FinishedSaving() { finishedSaving.SetActive(true);  }

    private void Update()
    {
        if (transform.childCount >= (VoxelData.ViewDistanceInChunks * VoxelData.ViewDistanceInChunks) * 9)
        {
            mainMenu.mainMenuCam.SetActive(false);
            crafting.SetActive(true);
            worlds.mainmenu.SetActive(false);
        }

        SetGlobalLightLevel();
        playerChunkCoord = GetChunkCoordFromVector3(player.position);

        // Only update the chunks if the player has moved from the chunk they were previously on.
        if (!playerChunkCoord.Equals(playerLastChunkCoord))
            CheckViewDistance();

        if (chunksToCreate.Count > 0)
            CreateChunk();

        if (chunksToDraw.Count > 0)
        {
            chunksToDraw.Dequeue().CreateMesh();
        }

        if (!enableThreading)
        {

            if (!applyingModifications)
                ApplyModifications();

            if (chunksToUpdate.Count > 0)
                UpdateChunks();

        }

        Camera.main.backgroundColor = Color.Lerp(day, night, time / dayLength);
        

        if (Input.GetKeyDown(KeyCode.F1))
        {
            saveWarning.SetActive(true);

            List<int> id = new List<int>();
            List<int> amount = new List<int>();
            for (int i = 0; i < 9; i++)
            {
                id.Add(toolbar.slots[i].itemSlot.stack.id);
                amount.Add(toolbar.slots[i].itemSlot.stack.amount);
            }
            SaveSystem.SaveToolbarID(id.ToArray(), worldData.worldName);
            SaveSystem.SaveToolbarAmount(amount.ToArray(), worldData.worldName);
            SaveSystem.SavePlayerPosition(player.position, worldData.worldName);
            SaveSystem.SaveDayData(worldData.worldName);

            SaveSystem.SaveWorld(worldData);
        }
        if (SaveSystem.finishedSaving)
        {
            FinishedSaving();
            SaveSystem.finishedSaving = false;
        }

        //Day cycle
        time += Time.deltaTime;
        if (time > dayLength)
            time = 0;
        
        if (time < dayLength / 2 && lightIntensity < lightMaxIntensity)
        {
            lightIntensity += 0.1f * Time.deltaTime * lightTransitionTime;
        }
        if (time > dayLength / 2 && lightIntensity > lightMinIntensity)
        {
            lightIntensity -= 0.1f * Time.deltaTime * lightTransitionTime;
        }

        sun.GetComponent<Light>().intensity = lightIntensity;
        sun.transform.GetChild(0).GetComponent<Light>().intensity = lightIntensity;
    }

    void LoadWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.LoadDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.LoadDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.LoadDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.LoadDistanceInChunks; z++)
            {
                worldData.LoadChunk(new Vector2Int(x, z));
            }
        }
    }

    void GenerateWorld()
    {
        for (int x = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; x < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = (VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks; z < (VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks; z++)
            {
                ChunkCoord newChunk = new ChunkCoord(x, z);
                chunks[x, z] = new Chunk(newChunk);
                chunksToCreate.Add(newChunk);
            } 
        }

        player.position = spawnPosition;          
        clouds.UpdateClouds();
        CheckViewDistance();
    }

    void CreateChunk()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        chunks[c.x, c.z].Init();
    }

    void UpdateChunks()
    {
        lock (ChunkUpdateThreadLock)
        {
            chunksToUpdate[0].UpdateChunk();
            if (!activeChunks.Contains(chunksToUpdate[0].coord))
                activeChunks.Add(chunksToUpdate[0].coord);
           chunksToUpdate.RemoveAt(0);
        }
    }

    void ThreadedUpdate()
    {
        while (true)
        {
            if (!applyingModifications)
                ApplyModifications();

            if (chunksToUpdate.Count > 0)
                UpdateChunks();
        }
    }

    private void OnDisable()
    {
        if (enableThreading)
        {
            ChunkUpdateThread.Abort();
        }
    }

    void ApplyModifications()
    {
        applyingModifications = true;

        while (modifications.Count > 0)
        {

            Queue<VoxelMod> queue = modifications.Dequeue();

            while (queue.Count > 0)
            {

                VoxelMod v = queue.Dequeue();

                worldData.SetVoxel(v.position, v.id);
            }
        }

        applyingModifications = false;

    }

    ChunkCoord GetChunkCoordFromVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);

    }

    public Chunk GetChunkFromVector3(Vector3 pos)
    {

        int x = Mathf.FloorToInt(pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(pos.z / VoxelData.ChunkWidth);
        return chunks[x, z];

    }

    void CheckViewDistance()
    {
        clouds.UpdateClouds();

        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;

        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        activeChunks.Clear();

        // Loop through all chunks currently within view distance of the player.
        for (int x = coord.x - VoxelData.ViewDistanceInChunks; x < coord.x + VoxelData.ViewDistanceInChunks; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks; z < coord.z + VoxelData.ViewDistanceInChunks; z++)
            {
                ChunkCoord thisChunk = new ChunkCoord(x, z);

                // If the current chunk is in the world...
                if (IsChunkInWorld(thisChunk))
                {
                    // Check if it active, if not, activate it.
                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(thisChunk);
                        chunksToCreate.Add(thisChunk);
                    }
                    else if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                    }
                    activeChunks.Add(thisChunk);
                }

                // Check through previously active chunks to see if this chunk is there. If it is, remove it from the list.
                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].Equals(new ChunkCoord(x, z)))
                        previouslyActiveChunks.RemoveAt(i);
                }

            }
        }

        // Any chunks left in the previousActiveChunks list are no longer in the player's view distance, so loop through and disable them.
        foreach (ChunkCoord c in previouslyActiveChunks)
            chunks[c.x, c.z].isActive = false;

    }

    public bool CheckForVoxel(Vector3 pos)
    {
        VoxelState voxel = worldData.GetVoxel(pos);
        if (blocktypes[voxel.id].isSolid)
            return true;
        else
            return false;
    }

    public VoxelState GetVoxelState(Vector3 pos)
    {
        return worldData.GetVoxel(pos);
    }

    public bool inUI
    {

        get { return _inUI; }

        set
        {

            _inUI = value;
            if (_inUI)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

    }

    public byte GetVoxel(Vector3 pos)
    {

        int yPos = Mathf.FloorToInt(pos.y);

        /* IMMUTABLE PASS */

        // If outside world, return air.
        if (!IsVoxelInWorld(pos))
            return 0;

        // If bottom block of chunk, return bedrock.
        if (yPos == 0)
            return 1;

        /* BIOME SELECTION PASS */

        int solidGroundHeight = 42;
        float sumOfHiehgts = 0;
        int count = 0;
        float strongestWeight = 0;
        int strongestBiomeIndex = 0;

        for (int i = 0; i < biomes.Length; i++)
        {
            float weight = Noise.Get2DPerlin(new Vector2(pos.x, pos.z), biomes[i].offset, biomes[i].scale);

            if (weight > strongestWeight)
            {
                strongestWeight = weight;
                strongestBiomeIndex = i;
            }

            float height = biomes[i].terrainHeight * Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biomes[i].terrainScale) * weight;

            if (height > 0)
            {
                sumOfHiehgts += height;
                count++;
            }        
        }

        //Set Biome
        BiomeAttributes biome = biomes[strongestBiomeIndex];

        sumOfHiehgts /= count;
        int terrainHeight = Mathf.FloorToInt(sumOfHiehgts + solidGroundHeight);


        /* BASIC TERRAIN PASS */
        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = biome.surfaceBlock;
        else if (yPos < terrainHeight && yPos > terrainHeight - biome.subsurfaceBlockHeight)
            voxelValue = biome.subSurfaceBlock;
        else if (yPos > terrainHeight)
            return 0;
        else
            voxelValue = 2;

        /* SECOND PASS */

        if (voxelValue == 2)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                    if (Noise.Get3DPerlin(pos, lode.noiseOffset, lode.scale, lode.threshold))
                        voxelValue = lode.blockID;
            }
        }

        /* TREE PASS */

        if (yPos == terrainHeight && biome.placeMajorFlora)
        {
            if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraZoneScale) > biome.majorFloraZoneThreshold)
            {
                if (Noise.Get2DPerlin(new Vector2(pos.x, pos.z), 0, biome.majorFloraPlacementScale) > biome.majorFloraPlacementThreshold)
                {
                    System.Random random = new System.Random();
                    modifications.Enqueue(Structure.GenerateMajorFlora(biome.majorFloraIndex[(int)random.Next(0, biome.majorFloraIndex.Length)], pos, biome.minHeight, biome.maxHeight));
                }
            }

        }

        return voxelValue;


    }

    bool IsChunkInWorld(ChunkCoord coord)
    {

        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 && coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
            return true;
        else
            return
                false;

    }

    bool IsVoxelInWorld(Vector3 pos)
    {

        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels && pos.y >= 0 && pos.y < VoxelData.ChunkHeight && pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
            return true;
        else
            return false;

    }
}

[System.Serializable]
public class BlockType
{

    public string blockName;
    public bool isSolid;
    public int outputBlock;
    public VoxelMeshData meshData;
    public bool renderNeighborFaces;
    public bool cantBreak;
    public bool canPlace;
    public float transparency;
    public float blockBreakTime;
    public Sprite icon;
    public ParticleSystem breakParticles;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;

    // Back, Front, Top, Bottom, Left, Right

    public int GetTextureID(int faceIndex)
    {

        switch (faceIndex)
        {

            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID; invalid face index");
                return 0;


        }

    }
}

public class VoxelMod
{

    public Vector3 position;
    public byte id;

    public VoxelMod()
    {

        position = new Vector3();
        id = 0;

    }

    public VoxelMod(Vector3 _position, byte _id)
    {

        position = _position;
        id = _id;

    }

}