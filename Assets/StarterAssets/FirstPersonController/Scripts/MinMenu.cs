using UnityEngine;
using UnityEngine.SceneManagement;

public class MinMenu : MonoBehaviour
{
    private bool seNalagam;

    public void OdpriDiplodoka()
    {
        OdpriSceno("Diplodok");
    }

    public void OdpriPleziozavra()
    {
        OdpriSceno("Pleziozaver");
    }

    public void OdpriStegozavra()
    {
        OdpriSceno("Stegozaver");
    }

    public void OdpriKoritozavra()
    {
        OdpriSceno("Koritozaver");
    }

    public void OdpriMenu()
    {
        OdpriSceno("Menu");
    }

    private void OdpriSceno(string imeScene)
    {
        if (seNalagam)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(imeScene))
        {
            Debug.LogError(
                "Scene '" + imeScene +
                "' ni vključena v Build Profiles ali ima drugačno ime."
            );

            return;
        }

        seNalagam = true;
        Debug.Log("Odpiram sceno: " + imeScene);
        SceneManager.LoadSceneAsync(imeScene);
    }
}