using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public GameObject logo;  // Hierarchy¿« LogoImage ≥÷±‚
    public float animTime = 0.7f;

    public void OnStartClick()
    {
        StartCoroutine(LoadNextScene(animTime));
    }

    IEnumerator LoadNextScene(float wait)
    {
        yield return new WaitForSeconds(wait);
        SceneManager.LoadScene("PlayDemoScene");
    }
}

