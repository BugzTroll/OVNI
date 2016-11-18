using UnityEngine;
using System.Collections;

public class Fader : MonoBehaviour {

    public float fadeSpeed;

    private bool sceneStarting = true;
    private bool sceneEnding = false;

    private UnityEngine.UI.RawImage image;

    private string NextSceneName;

    void Awake()
    {
        GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);

        image = GetComponent<UnityEngine.UI.RawImage>();
        image.enabled = true;

        sceneStarting = true;
    }

    void Update()
    {
        if (sceneStarting)
            StartScene();

        else if (sceneEnding)
            EndScene(NextSceneName);
    }

    void FadeToBlack()
    {
        image.color = Color.Lerp(image.color, Color.black, fadeSpeed * Time.deltaTime);
    }

    void FadeToClear()
    {
        image.color = Color.Lerp(image.color, Color.clear, fadeSpeed * Time.deltaTime);
    }

    void StartScene()
    {
        FadeToClear();

        if (image.color.a <= 0.05f)
        {
            image.color = Color.clear;
            image.enabled = false;

            sceneStarting = false;
        }
    }

    public void EndScene(string nextSceneName)
    {
        sceneEnding = true;
        NextSceneName = nextSceneName;

        SceneEnding();

    }

    void SceneEnding()
    {
        image.enabled = true;

        FadeToBlack();

        if (image.color.a >= 0.95f)
            GameManager.Instance.ChangeScene(NextSceneName);
    }

}
