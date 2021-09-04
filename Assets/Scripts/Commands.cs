using UnityEngine.UI;
using UnityEngine;

public class Commands : MonoBehaviour
{
    public Text chat;

    void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            CheckForCommands();
        }
    }

    void CheckForCommands()
    {
        if (GetComponent<InputField>().text != "" && GetComponent<InputField>().text != null)
        {
            string command = GetComponent<InputField>().text;

            if (!command.Contains("/"))
                chat.text += "\n <You>: " + command;

            if (command.Contains("/"))
            {
                if (command == "/seed")
                    chat.text += "\nThe seed is " + World.seed;
                else if (command == "/position")
                    chat.text += "\nX: " + GameObject.Find("World").GetComponent<World>().player.position.x + "  Y: " + GameObject.Find("World").GetComponent<World>().player.position.y + "  Z: " + GameObject.Find("World").GetComponent<World>().player.position.z;
                else
                    chat.text += "\n That isnt a valid command";
            }
        }

        gameObject.GetComponent<InputField>().text = "";
    }
}
