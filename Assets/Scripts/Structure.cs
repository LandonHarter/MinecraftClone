using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> GenerateMajorFlora(int index, Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        switch (index)
        {

            case 0:
                return MakeTree(position, minTrunkHeight, maxTrunkHeight);
            case 1:
                return MakeCacti(position, minTrunkHeight, maxTrunkHeight);
            case 2:
                return MakeJungleTree(position, minTrunkHeight, maxTrunkHeight);
            case 3:
                return MakeBirchTree(position, minTrunkHeight, maxTrunkHeight);
        }
        return new Queue<VoxelMod>();
    }
    public static Queue<VoxelMod> MakeTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        //Tree Generation
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 250, 3));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 6));

        //Leaves Generation
        for (int x = -3; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = -3; z < 4; z++)
                {
                    queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 11));
                }
            }
        }

        return queue;
    }

    public static Queue<VoxelMod> MakeBirchTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        //Tree Generation
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 250, 3));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 18));

        //Leaves Generation
        for (int x = -3; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                for (int z = -3; z < 4; z++)
                {
                    queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 20));
                }
            }
        }

        return queue;
    }

    public static Queue<VoxelMod> MakeJungleTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        //Tree Generation
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 250, 3));

        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int x = 0; x < 2; x++)
        {
            for (int z = 0; z < 2; z++)
            {
                for (int i = 1; i < height; i++)
                    queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + i, position.z + z), 17));
            }
        }
        

        //Leaves Generation
        for (int x = -5; x < 8; x++)
        {
            for (int y = 0; y < 5; y++)
            {
                for (int z = -5; z < 8; z++)
                {
                    queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 19));
                }
            }
        }

        return queue;
    }
    public static Queue<VoxelMod> MakeCacti(Vector3 position, int minHeight, int maxHeight)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();

        //Cacti Generation
        int height = (int)(maxHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), World.seed, 2));

        if (height < minHeight)
            height = minHeight;

        for (int i = 1; i <= height; i++)
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 12));

        return queue;
    }
}
