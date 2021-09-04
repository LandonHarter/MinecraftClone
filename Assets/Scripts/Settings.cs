using UnityEngine.UI;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public Slider renderDistanceSlider;
    public Toggle enableThreading;
    public Toggle enableClouds;
    public Toggle pp;
    public Dropdown cloudStyleDropdown;
    public Dropdown lightStyleDropdown;
    public GameObject clouds;
    public GameObject postproccesing;
    public CloudStyle style = CloudStyle.Fancy;
    public LightStyle lightStyle = LightStyle.Fancy;

    [Space]

    public Material block;
    public Material transparentBlock;
    public Shader standard;
    public Shader blockShader;
    public Shader transparentBlockShader;

    public World world;

    void Start()
    {
        try
        {
            renderDistanceSlider.value = PlayerPrefs.GetInt("ViewDistance");

            if (PlayerPrefs.GetInt("Clouds") == 0)
                enableClouds.isOn = false;
            if (PlayerPrefs.GetInt("Clouds") == 1)
                enableClouds.isOn = true;

            if (PlayerPrefs.GetInt("CloudStyle") == 1)
            {
                style = CloudStyle.Fast;
                cloudStyleDropdown.value = 1;
            }
            if (PlayerPrefs.GetInt("CloudStyle") == 2)
            {
                style = CloudStyle.Fancy;
                cloudStyleDropdown.value = 2;
            }

            if (PlayerPrefs.GetInt("LightStyle") == 1)
            {
                lightStyle = LightStyle.Fast;
                lightStyleDropdown.value = 1;
            }
            if (PlayerPrefs.GetInt("LightStyle") == 2)
            {
                lightStyle = LightStyle.Fancy;
                lightStyleDropdown.value = 2;
            }
        }
        catch
        {
            renderDistanceSlider.value = VoxelData.ViewDistanceInChunks;
        }

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            enableThreading.isOn = false;
    }
    public void ChangeRenderDistance()
    {
        VoxelData.ViewDistanceInChunks = (int)renderDistanceSlider.value;
        PlayerPrefs.SetInt("ViewDistance", (int)renderDistanceSlider.value);
    }
    public void EnableOrDisableThreading()
    {
        world.enableThreading = enableThreading.isOn;
    }
    public void EnableOrDisableClouds()
    {
        clouds.SetActive(enableClouds.isOn);

        if (enableClouds.isOn)
            PlayerPrefs.SetInt("Clouds", 1);
        else
            PlayerPrefs.SetInt("Clouds", 0);
    }

    public void EnablePP()
    {
        postproccesing.SetActive(pp.isOn);
    }

    public void ChangeCloudStyle()
    {
        if (cloudStyleDropdown.value == 1)
            style = CloudStyle.Fast;
        if (cloudStyleDropdown.value == 2)
            style = CloudStyle.Fancy;

        PlayerPrefs.SetInt("CloudStyle", cloudStyleDropdown.value);
    }

    public void ChangeLightStyle()
    {
        if (lightStyleDropdown.value == 1)
        {
            block.shader = blockShader;
            transparentBlock.shader = transparentBlockShader;
            lightStyle = LightStyle.Fast;
        }
        if (lightStyleDropdown.value == 2)
        {
            block.shader = standard;
            transparentBlock.shader = standard;
            lightStyle = LightStyle.Fancy;
        }

        PlayerPrefs.SetInt("LightStyle", lightStyleDropdown.value);
    }

    public void Back()
    {
        gameObject.SetActive(false);
    }
}

public enum LightStyle
{
    Fast,
    Fancy
}
