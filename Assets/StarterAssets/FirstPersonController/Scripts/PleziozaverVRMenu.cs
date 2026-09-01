using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PleziozaverVRMenu : MonoBehaviour
{
    private static readonly string[] SceneNames =
    {
        "Menu",
        "Diplodok",
        "Stegozaver",
        "Koritozaver"
    };

    private static readonly string[] ButtonLabels =
    {
        "Glavni meni",
        "Diplodok",
        "Stegozaver",
        "Koritozaver"
    };

    private readonly Color normalColor = new Color(0.04f, 0.19f, 0.26f, 0.96f);
    private readonly Color selectedColor = new Color(0.08f, 0.55f, 0.68f, 1f);

    private InputAction toggleAction;
    private InputAction navigateAction;
    private InputAction confirmAction;
    private GameObject menuRoot;
    private Image[] buttonImages;
    private int selectedIndex;
    private bool navigationReady = true;
    private bool loadingScene;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneHandler()
    {
        SceneManager.sceneLoaded -= AddMenuToWaterScene;
        SceneManager.sceneLoaded += AddMenuToWaterScene;
    }

    private static void AddMenuToWaterScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Pleziozaver")
            return;

        if (FindFirstObjectByType<PleziozaverVRMenu>() != null)
            return;

        var controller = new GameObject("Pleziozaver VR Menu Controller");
        controller.AddComponent<PleziozaverVRMenu>();
        SceneManager.MoveGameObjectToScene(controller, scene);
        Debug.Log("Pleziozaver VR menu initialized. Press X on the left controller to open it.");
    }

    private void Awake()
    {
        toggleAction = new InputAction("Toggle water scene menu");
        toggleAction.AddBinding("<Keyboard>/m");
        toggleAction.AddBinding("<XRController>{LeftHand}/primaryButton");
        toggleAction.AddBinding("<XRController>{LeftHand}/menuButton");

        navigateAction = new InputAction("Navigate water scene menu", InputActionType.Value);
        navigateAction.AddBinding("<XRController>{LeftHand}/thumbstick");
        navigateAction.AddBinding("<Gamepad>/leftStick");

        confirmAction = new InputAction("Confirm water scene menu");
        confirmAction.AddBinding("<XRController>{RightHand}/primaryButton");
        confirmAction.AddBinding("<XRController>{RightHand}/triggerPressed");
        confirmAction.AddBinding("<Keyboard>/enter");
    }

    private IEnumerator Start()
    {
        while (Camera.main == null)
            yield return null;

        BuildMenu();
        menuRoot.SetActive(true);
    }

    private void OnEnable()
    {
        toggleAction.performed += ToggleMenu;
        confirmAction.performed += ConfirmSelection;
        toggleAction.Enable();
        navigateAction.Enable();
        confirmAction.Enable();
    }

    private void OnDisable()
    {
        toggleAction.performed -= ToggleMenu;
        confirmAction.performed -= ConfirmSelection;
        toggleAction.Disable();
        navigateAction.Disable();
        confirmAction.Disable();
    }

    private void OnDestroy()
    {
        toggleAction.Dispose();
        navigateAction.Dispose();
        confirmAction.Dispose();
    }

    private void Update()
    {
        if (menuRoot == null || !menuRoot.activeSelf)
            return;

        float vertical = navigateAction.ReadValue<Vector2>().y;
        if (Mathf.Abs(vertical) < 0.35f)
        {
            navigationReady = true;
            return;
        }

        if (!navigationReady)
            return;

        navigationReady = false;
        selectedIndex = (selectedIndex + (vertical < 0f ? 1 : -1) + ButtonLabels.Length) % ButtonLabels.Length;
        RefreshSelection();
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
        if (menuRoot == null || loadingScene)
            return;

        bool show = !menuRoot.activeSelf;
        if (show)
            PositionMenuInFrontOfPlayer();

        menuRoot.SetActive(show);
    }

    private void ConfirmSelection(InputAction.CallbackContext context)
    {
        if (menuRoot == null || !menuRoot.activeSelf || loadingScene)
            return;

        string sceneName = SceneNames[selectedIndex];
        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError("Scene '" + sceneName + "' is not included in Build Profiles.");
            return;
        }

        loadingScene = true;
        SceneManager.LoadSceneAsync(sceneName);
    }

    private void PositionMenuInFrontOfPlayer()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < 0.01f)
            forward = cameraTransform.forward;

        menuRoot.transform.position = cameraTransform.position + forward * 1.6f;
        menuRoot.transform.rotation = Quaternion.LookRotation(menuRoot.transform.position - cameraTransform.position, Vector3.up);
    }

    private void BuildMenu()
    {
        menuRoot = new GameObject("Water Scene Menu", typeof(RectTransform), typeof(Canvas));
        Canvas canvas = menuRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;

        RectTransform canvasRect = menuRoot.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(900f, 650f);
        canvasRect.localScale = Vector3.one * 0.001f;

        GameObject panel = CreateImage("Panel", menuRoot.transform, new Color(0.008f, 0.055f, 0.075f, 0.97f));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        CreateText("Title", panel.transform, "IZBERI SCENO", 54, new Vector2(0f, 245f), new Vector2(760f, 80f));
        CreateText("Help", panel.transform, "Leva palica: izbira     A ali desni sprozilec: potrdi     X: zapri", 27,
            new Vector2(0f, -275f), new Vector2(820f, 55f));

        buttonImages = new Image[ButtonLabels.Length];
        for (int i = 0; i < ButtonLabels.Length; i++)
        {
            GameObject button = CreateImage("Button " + ButtonLabels[i], panel.transform, normalColor);
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(600f, 82f);
            buttonRect.anchoredPosition = new Vector2(0f, 135f - i * 105f);
            buttonImages[i] = button.GetComponent<Image>();
            CreateText("Label", button.transform, ButtonLabels[i], 38, Vector2.zero, buttonRect.sizeDelta);
        }

        RefreshSelection();
        PositionMenuInFrontOfPlayer();
    }

    private static GameObject CreateImage(string objectName, Transform parent, Color color)
    {
        var gameObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        gameObject.transform.SetParent(parent, false);
        gameObject.GetComponent<Image>().color = color;
        return gameObject;
    }

    private static void CreateText(string objectName, Transform parent, string value, int fontSize,
        Vector2 position, Vector2 size)
    {
        var gameObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        gameObject.transform.SetParent(parent, false);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Text text = gameObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
    }

    private void RefreshSelection()
    {
        for (int i = 0; i < buttonImages.Length; i++)
            buttonImages[i].color = i == selectedIndex ? selectedColor : normalColor;
    }
}
