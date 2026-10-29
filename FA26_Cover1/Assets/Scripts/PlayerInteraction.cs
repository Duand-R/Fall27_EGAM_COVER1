using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Player References")]
    public Camera playerCamera;
    public PlayerMovement playerMovement;
    public PlayerLook playerLook;

    [Header("Inventory Windows")]
    public GameObject containerPanel;
    public GameObject backpackPanel;
    public ContainerUI containerUI;

    [Header("Screen UI")]
    public GameObject crosshair;
    public TMP_Text interactionText;

    [Header("Interaction Settings")]
    public float interactDistance = 3f;

    // 表示当前是否正在操作背包或箱子界面
    private bool isOpen;

    private void Start()
    {
        // 游戏开始时隐藏两个窗口
        containerPanel.SetActive(false);
        backpackPanel.SetActive(false);

        crosshair.SetActive(true);

        interactionText.text = "Press E to Search";
        interactionText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Tab：关闭当前界面，或单独打开背包
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenBackpack();
            }

            return;
        }

        // 界面打开时，只处理 Esc 关闭
        if (isOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseInventory();
            }

            return;
        }

        // 检测玩家是否看向附近的箱子
        LootContainer target = FindContainer();

        // 只有可以搜索时才显示提示
        interactionText.gameObject.SetActive(target != null);

        if (target != null && Input.GetKeyDown(KeyCode.E))
        {
            OpenContainer(target);
        }
    }

    private LootContainer FindContainer()
    {
        // 从画面中心向前检测
        Ray ray = playerCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactDistance,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore))
        {
            return hit.collider.GetComponentInParent<LootContainer>();
        }

        return null;
    }

    private void OpenBackpack()
    {
        // 单独打开玩家背包
        containerPanel.SetActive(false);
        backpackPanel.SetActive(true);

        SetInventoryMode(true);
    }

    private void OpenContainer(LootContainer target)
    {
        // 读取当前箱子的物品
        containerUI.container = target;
        containerUI.RefreshItems();

        // 同时打开箱子和玩家背包
        containerPanel.SetActive(true);
        backpackPanel.SetActive(true);

        SetInventoryMode(true);
    }

    public void CloseInventory()
    {
        // 关闭所有库存窗口
        containerPanel.SetActive(false);
        backpackPanel.SetActive(false);

        SetInventoryMode(false);
    }

    private void SetInventoryMode(bool open)
    {
        isOpen = open;

        // 操作界面时隐藏准星和交互提示
        crosshair.SetActive(!open);
        interactionText.gameObject.SetActive(false);

        // 操作界面时暂停角色移动和鼠标视角
        playerMovement.enabled = !open;
        playerLook.enabled = !open;

        // 打开界面时释放鼠标，关闭后恢复视角控制
        Cursor.lockState = open
            ? CursorLockMode.None
            : CursorLockMode.Locked;

        Cursor.visible = open;
    }
}