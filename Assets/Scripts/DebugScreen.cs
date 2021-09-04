using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugScreen : MonoBehaviour {

    World world;
    Text text;

    float frameRate;
    float timer;

    int halfWorldSizeInVoxels;
    int halfWorldSizeInChunks;

    void Start() {
        Application.targetFrameRate = -1;

        world = GameObject.Find("World").GetComponent<World>();
        text = GetComponent<Text>();

        halfWorldSizeInVoxels = VoxelData.WorldSizeInVoxels / 2;
        halfWorldSizeInChunks = VoxelData.WorldSizeInChunks / 2;
    }

    void Update() {

        string debugText = "Debug Menu";
        debugText += "\n";
        debugText += frameRate + " FPS\n";
        debugText += "Chunk: " + world.playerChunkCoord.x + ", " + world.playerChunkCoord.z + "\n";
        debugText += "X: " + world.player.position.x + "\n";
        debugText += "Y: " + world.player.position.y + "\n";
        debugText += "Z: " + world.player.position.z + "\n";

        string direction = "";
        switch (world._player.orientation)
        {
            case 0:
                direction = "North";
                break;
            case 5:
                direction = "East";
                break;
            case 1:
                direction = "South";
                break;
            default:
                direction = "West";
                break;
        }
        debugText += "Direction: " + direction + "\n";

        text.text = debugText;

        if (timer > 1f) {

            frameRate = (int)(1f / Time.unscaledDeltaTime);
            timer = 0;

        } else
            timer += Time.deltaTime;

    }
}
