using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

        // Menijski gumb na levem kontrolerju kot dodatna Quest/OpenXR vezava.
        menuAction.AddBinding("<XRController>{LeftHand}/menuButton");
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
        // Glavni meni mora biti viden takoj; v igralnih scenah se odpre z gumbom.
        menu.SetActive(SceneManager.GetActiveScene().name == "Menu");
    }

    private void ToggleMenu(InputAction.CallbackContext context)
    {
        menu.SetActive(!menu.activeSelf);
    }
}
