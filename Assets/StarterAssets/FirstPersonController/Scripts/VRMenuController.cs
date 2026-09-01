using UnityEngine;
using UnityEngine.InputSystem;

public class VRMenuController : MonoBehaviour
{
    [SerializeField] private GameObject menu;

    private InputAction menuAction;

    private void Awake()
    {
        menuAction = new InputAction("Toggle VR Menu");

        // Tipka M za test v Unity Editorju.
        menuAction.AddBinding("<Keyboard>/m");

        // Gumb X na levem VR-kontrolerju.
        menuAction.AddBinding("<XRController>{LeftHand}/primaryButton");
    }

    private void OnEnable()
    {
        menuAction.performed += ToggleMenu;
        menuAction.Enable();
    }

    private void OnDisable()
    {
        menuAction.performed -= ToggleMenu;
        menuAction.Disable();
    }

    private void Start()
    {
        menu.SetActive(false);
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
        menu.SetActive(!menu.activeSelf);
    }
}