using UnityEngine.UI;
using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    public bool isGrounded;
    public bool isSprinting;
    public bool creativeModeMining = true;

    public Transform cam;
    public World world;
    public Crafting crafting;
    public ToolCrafter toolCrafting;
    public Smelting smelting;
    public GameObject craftingMenu;
    public GameObject toolCraftingMenu;
    public GameObject craftingTableMenu;
    public GameObject pauseMenu;
    public Slider cameraFOVslider;
    public GameObject commandChat;
    public Material[] blockBreakingMaterial;
    public MeshRenderer[] highlightBlockPieces;
    public Material highlightBlockMaterial;
    public GameObject crosshair;

    bool miningBlock = false;
    bool brokeBlock = false;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;
    public float sensitivity;

    public float playerWidth = 0.15f;
    public float boundsTolerance = 0.1f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private float controllerHorizontal;
    private float controllerVeritcal;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;

    public int orientation;

    public Transform hightlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 8;

    public Toolbar toolbar;

    private void Start() {
        //Lock Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraFOVslider.value = cam.GetComponent<Camera>().fieldOfView;

        if (!World.Instance.mainMenu.newWorld)
            transform.position = new Vector3(SaveSystem.LoadPlayerPosition(World.Instance.worldData.worldName)[0], SaveSystem.LoadPlayerPosition(World.Instance.worldData.worldName)[1], SaveSystem.LoadPlayerPosition(World.Instance.worldData.worldName)[2]);
    }

    private void FixedUpdate() {
        if (!world.inUI && commandChat.activeSelf == false)
        {
            CalculateVelocity();

            if (jumpRequest)
                Jump();

            transform.Rotate(Vector3.up * mouseHorizontal * sensitivity);
            cam.Rotate(Vector3.right * -mouseVertical * sensitivity);
            transform.Translate(velocity, Space.World);
        }
    }

    private void Update() {

        if (Input.GetKeyDown(KeyCode.E) && !toolCraftingMenu.activeSelf)
        {
            world.inUI = !world.inUI;
            craftingMenu.SetActive(!craftingMenu.activeSelf);
            Cursor.visible = !Cursor.visible;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            world.inUI = false;
            toolCraftingMenu.SetActive(false);
            Cursor.visible = false;
        }

        if (!world.inUI)
        {
            GetPlayerInputs();
            PlaceCursorBlock();
        }

        Vector3 XZDirection = transform.forward;
        XZDirection.y = 0;
        if (Vector3.Angle(XZDirection, Vector3.forward) <= 45)
            orientation = 0;
        else if (Vector3.Angle(XZDirection, Vector3.right) <= 45)
            orientation = 5;
        else if (Vector3.Angle(XZDirection, Vector3.back) <= 45)
            orientation = 1;
        else
            orientation = 4;
    }
    void Jump () {

        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;

    }

    private void CalculateVelocity () {

        // Affect vertical momentum with gravity.
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        // if we're sprinting, use the sprint multiplier.
        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime * walkSpeed;

        // Apply vertical momentum (falling/jumping).
        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = checkDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = checkUpSpeed(velocity.y);
    }

    private void GetPlayerInputs () 
    {
        bool smelted = false;

        if (commandChat.activeSelf == false)
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            mouseHorizontal = Input.GetAxis("Mouse X");
            mouseVertical = Input.GetAxis("Mouse Y");
            controllerHorizontal = Input.GetAxis("Controller X");
            controllerVeritcal = Input.GetAxis("Controller Y");

            if (Input.GetMouseButtonDown(1))
            {
                if (world.GetChunkFromVector3(hightlightBlock.transform.position).GetVoxelFromGlobalVector3(hightlightBlock.transform.position).id == 25)
                {
                    OpenCraftingTableMenu();
                }
                if (world.GetChunkFromVector3(hightlightBlock.transform.position).GetVoxelFromGlobalVector3(hightlightBlock.transform.position).id == 28)
                {
                    for (int i = 0; i < world.smeltableObjects.Length; i++)
                    {
                        if (world.smeltableObjects[i].block == toolbar.slots[toolbar.slotindex].itemSlot.stack.id)
                        {
                            smelting.Smelt(world.smeltableObjects[i]);
                        }
                    }

                    smelted = true;
                }
                if (world.GetChunkFromVector3(hightlightBlock.transform.position).GetVoxelFromGlobalVector3(hightlightBlock.transform.position).id == 27 && !craftingMenu.activeSelf)
                {
                    world.inUI = !world.inUI;
                    toolCraftingMenu.SetActive(!craftingMenu.activeSelf);
                    Cursor.visible = !Cursor.visible;
                }
            }

            if (Input.GetButtonDown("Sprint"))
                isSprinting = true;
            if (Input.GetButtonUp("Sprint"))
                isSprinting = false;

            if (isGrounded && Input.GetButton("Jump"))
                jumpRequest = true;

            if (hightlightBlock.gameObject.activeSelf && !pauseMenu.activeSelf)
            {
                //Destory Block
                if (Input.GetButtonDown("Break Block") && !miningBlock)
                {
                    miningBlock = true;
                    StartCoroutine(BreakBlock());
                }

                //Place Block
                if (Input.GetButtonDown("Place Block") && !smelted)
                {
                    if (toolbar.slots[toolbar.slotindex].hasItem && new Vector3(Mathf.FloorToInt(gameObject.transform.position.x), Mathf.FloorToInt(gameObject.transform.position.y), Mathf.FloorToInt(gameObject.transform.position.z)) != new Vector3(Mathf.FloorToInt(hightlightBlock.transform.position.x), Mathf.FloorToInt(hightlightBlock.transform.position.y), Mathf.FloorToInt(hightlightBlock.transform.position.z)))
                    {
                        if (new Vector3(Mathf.FloorToInt(gameObject.transform.position.x), Mathf.FloorToInt(gameObject.transform.position.y - 1), Mathf.FloorToInt(gameObject.transform.position.z)) != new Vector3(Mathf.FloorToInt(hightlightBlock.transform.position.x), Mathf.FloorToInt(hightlightBlock.transform.position.y), Mathf.FloorToInt(hightlightBlock.transform.position.z)))
                        {
                            if (world.blocktypes[toolbar.slots[toolbar.slotindex].itemSlot.stack.id].canPlace)
                            {
                                world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, toolbar.slots[toolbar.slotindex].itemSlot.stack.id, orientation);
                                toolbar.slots[toolbar.slotindex].itemSlot.Take(1);
                            }
                        }
                    }
                }
            }
        }  

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (commandChat.activeSelf == false)
            {
                pauseMenu.SetActive(!pauseMenu.activeSelf);

                if (pauseMenu.activeSelf)
                {
                    Time.timeScale = 0;
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Time.timeScale = 1;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeSelf == false)
            {
                commandChat.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if (craftingTableMenu.activeSelf)
                craftingTableMenu.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            commandChat.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            commandChat.gameObject.GetComponent<InputField>().text = "/";
            commandChat.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void OpenCraftingTableMenu()
    {
        craftingTableMenu.SetActive(true);
    }

    IEnumerator BreakBlock()
    {
        byte blockID = world.GetChunkFromVector3(hightlightBlock.position).GetVoxelFromGlobalVector3(hightlightBlock.position).id;

        if (world.blocktypes[blockID].cantBreak)
            miningBlock = false;

        if (miningBlock)
        {
            if (!creativeModeMining)
            {
                StartCoroutine(BreakBlockAnimation(blockID));
                yield return new WaitForSeconds(world.blocktypes[blockID].blockBreakTime);
            }
            else
                brokeBlock = true;

            bool addedItem = false;
            for (int i = 0; i < 9; i++)
            {
                if (toolbar.slots[i].itemSlot.stack.id == (byte)world.blocktypes[blockID].outputBlock && addedItem == false)
                {
                    toolbar.slots[i].itemSlot.Add(1);
                    toolbar.slots[i].UpdateSlot();
                    addedItem = true;
                }
            }
            for (int i = 0; i < 9; i++)
            {
                if (toolbar.slots[i].itemSlot.stack.id == 0 && addedItem == false)
                {
                    toolbar.slots[i].itemSlot.stack.id = (byte)world.blocktypes[blockID].outputBlock;
                    toolbar.slots[i].itemSlot.Add(1);
                    toolbar.slots[i].UpdateSlot();
                    addedItem = true;
                }
            }

            //Particles
            if (world.blocktypes[blockID].breakParticles != null)
            {
                ParticleSystem breakParticles = world.blocktypes[blockID].breakParticles;
                breakParticles = Instantiate(breakParticles);
                breakParticles.transform.position = hightlightBlock.transform.position;
                breakParticles.Play();

                Destroy(breakParticles.gameObject, 0.5f);
            }

            if (brokeBlock)
            {
                world.GetChunkFromVector3(hightlightBlock.position).EditVoxel(hightlightBlock.position, 0, orientation);
            }
        }

        miningBlock = false;
    }

    IEnumerator BreakBlockAnimation(int blockID)
    {
        Vector3 highlightBlockPosition = hightlightBlock.position;

        for (int i = 0; i < 10; i++)
        {
            if (hightlightBlock.position != highlightBlockPosition)
            {
                yield return null;
                miningBlock = false;
                break;
            }

            if (!Input.GetMouseButton(0))
            {
                yield return null;
                miningBlock = false;
                break;
            }
                
            for (int index = 0; index < 6; index++)
            {
                highlightBlockPieces[index].material = blockBreakingMaterial[i];
            }
            yield return new WaitForSeconds(world.blocktypes[blockID].blockBreakTime / 10);

            if (i == 9)
                brokeBlock = true;
        }

        for (int i = 0; i < 6; i++)
        {
            highlightBlockPieces[i].material = highlightBlockMaterial;
            miningBlock = false;
        }
    }

    private void PlaceCursorBlock()
    {
        float step = checkIncrement;
        Vector3 lastPosition = new Vector3();

        while (step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);

            if (world.CheckForVoxel(pos)) {
                hightlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x) + 0.5f, Mathf.FloorToInt(pos.y) + 0.5f, Mathf.FloorToInt(pos.z) + 0.5f);
                placeBlock.position = lastPosition;

                hightlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }

            lastPosition = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }

        hightlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }

    private float checkDownSpeed (float downSpeed) {
        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed, transform.position.z + playerWidth))
           ) {

            isGrounded = true;
            return 0;

        } else {

            isGrounded = false;
            return downSpeed;

        }

    }

    private float checkUpSpeed (float upSpeed) {

        if (
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed, transform.position.z + playerWidth))
           ) {

            return 0;

        } else {

            return upSpeed;

        }

    }

    public bool front {

        get {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z + playerWidth))
                )
                return true;
            else
                return false;
        }

    }
    public bool back {

        get {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z - playerWidth))
                )
                return true;
            else
                return false;
        }

    }
    public bool left {

        get {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }

    }
    public bool right {

        get {
            if (
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f, transform.position.z))
                )
                return true;
            else
                return false;
        }

    }

    public void ChangeCameraFOV()
    {
        cam.gameObject.GetComponent<Camera>().fieldOfView = cameraFOVslider.value;
    }
}
