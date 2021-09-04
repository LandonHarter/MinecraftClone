using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using UnityEngine;

public static class SaveSystem
{
    public static bool finishedSaving;
    
    public static void SaveWorld(WorldData world)
    {
        string savePath = World.Instance.appPath + "/Worlds/" + world.worldName + "/";
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "World.world", FileMode.Create);
        formatter.Serialize(stream, world);
        stream.Close();

        Thread thread = new Thread(() => SaveChunks(world));
        thread.Start();
    }

    public static WorldData LoadWorld(string worldName, int seed = 0)
    {
        string loadPath = World.Instance.appPath + "/Worlds/" + worldName + "/";
        if (File.Exists(loadPath + "World.world"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "World.world", FileMode.Open);
            WorldData world = formatter.Deserialize(stream) as WorldData;
            stream.Close();

            return new WorldData(world);
        }
        else
        {
            WorldData world = new WorldData(worldName, seed);
            SaveWorld(world);

            return world;
        }
    }

    public static ChunkData LoadChunk(string worldName, Vector2Int position)
    {
        string chunkName = position.x + "-" + position.y;

        string loadPath = World.Instance.appPath + "/Worlds/" + worldName + "/Chunks/" + chunkName + ".chunk";
        if (File.Exists(loadPath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath, FileMode.Open);
            ChunkData chunkData = formatter.Deserialize(stream) as ChunkData;
            stream.Close();

            return chunkData;
        }

        return null;
    }

    public static void SaveChunk(ChunkData chunk, string worldName)
    {
        string chunkName = chunk.position.x + "-" + chunk.position.y;

        string savePath = World.Instance.appPath + "/Worlds/" + worldName + "/Chunks/";
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + chunkName + ".chunk", FileMode.Create);
        formatter.Serialize(stream, chunk);
        stream.Close();
    }

    public static void SaveChunks(WorldData world)
    {
        List<ChunkData> chunks = new List<ChunkData>(world.modifiedChunks);
        world.modifiedChunks.Clear();

        int count = 0;
        foreach (ChunkData chunk in chunks)
        {
            SaveSystem.SaveChunk(chunk, world.worldName);
            count++;
        }
        finishedSaving = true;
    }

    public static int AmountOfWorlds()
    {
        int count = 0;

        string[] subdirectoryEntries = Directory.GetDirectories(Application.persistentDataPath + "/Worlds/");
        foreach (string subdirectory in subdirectoryEntries)
            count++;

        return count;
    }

    public static string[] WorldNames()
    {
        string[] dirNames = new string[AmountOfWorlds()];
        string[] names = Directory.GetDirectories(Application.persistentDataPath + "/Worlds/", "*", SearchOption.AllDirectories);
        for (int i = 0; i < dirNames.Length; i++)
        {
            dirNames[i] = names[i].Substring((Application.persistentDataPath + "/Worlds/").Length);
        }

        return dirNames;
    }

    public static void SaveToolbarID(int[] tb, string worldName)
    {
        string savePath = World.Instance.appPath + "/Worlds/" + worldName + "/Data/Toolbar/";
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "id.toolbar", FileMode.Create);
        formatter.Serialize(stream, tb);
        stream.Close();
    }
    public static void SaveToolbarAmount(int[] tb, string worldName)
    {
        string savePath = World.Instance.appPath + "/Worlds/" + worldName + "/Data/Toolbar/";
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "amount.toolbar", FileMode.Create);
        formatter.Serialize(stream, tb);
        stream.Close();
    }

    public static int[] LoadToolbarID(string worldName)
    {
        string loadPath = World.Instance.appPath + "/Worlds/" + worldName + "/Data/Toolbar/";
        if (File.Exists(loadPath + "id.toolbar"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "id.toolbar", FileMode.Open);
            int[] id = formatter.Deserialize(stream) as int[];
            stream.Close();

            return id;
        }

        return null;
    }
    public static int[] LoadToolbarAmount(string worldName)
    {
        string loadPath = World.Instance.appPath + "/Worlds/" + worldName + "/Data/Toolbar/";
        if (File.Exists(loadPath + "amount.toolbar"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "amount.toolbar", FileMode.Open);
            int[] id = formatter.Deserialize(stream) as int[];
            stream.Close();

            return id;
        }

        return null;
    }

    public static void SavePlayerPosition(Vector3 position, string worldName)
    {
        string savePath = World.Instance.appPath + "/Worlds/" + worldName + "/Data/PlayerData/";
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "position.data", FileMode.Create);
        formatter.Serialize(stream, new float[] { position.x, position.y, position.z });
        stream.Close();
    }

    public static float[] LoadPlayerPosition(string worldName)
    {
        string loadPath = World.Instance.appPath + "/Worlds/" + worldName + "/Data/PlayerData/";
        if (File.Exists(loadPath + "position.data"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "position.data", FileMode.Open);
            float[] position = formatter.Deserialize(stream) as float[];
            stream.Close();

            return position;
        }

        return null;
    }

    public static void SaveDayData(string worldName)
    {
        string savePath = World.Instance.appPath + "/Worlds/" + worldName + "/Data/";
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath + "light.data", FileMode.Create);
        formatter.Serialize(stream, new float[] { World.Instance.time, World.Instance.lightIntensity });
        stream.Close();
    }

    public static float[] LoadDayData(string worldName)
    {
        string loadPath = World.Instance.appPath + "/Worlds/" + worldName + "/Data/";
        if (File.Exists(loadPath + "light.data"))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(loadPath + "light.data", FileMode.Open);
            float[] light = formatter.Deserialize(stream) as float[];
            stream.Close();

            return light;
        }

        return null;
    }
}
