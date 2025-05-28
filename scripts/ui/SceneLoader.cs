using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public CanvasGroup fadeCanvas;
    public float fadeTime = 1f;
    public string sceneToLoad = "GameScene"; // ��� MenuScene, ��� ����

    void Start()
    {
        fadeCanvas.alpha = 1f;
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(0.5f); // ��� ������� ������ ����� ���������

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneToLoad);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
        {
            yield return null;
        }

        async.allowSceneActivation = true;

        yield return null; // ����� �������������

        StartCoroutine(FadeOut()); // ������ ������� ������ �����
    }

    IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = 1 - (t / fadeTime);
            yield return null;
        }

        fadeCanvas.alpha = 0f;
        fadeCanvas.gameObject.SetActive(false);
    }
}
