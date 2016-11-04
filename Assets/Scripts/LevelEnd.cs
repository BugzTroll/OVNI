using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    public GameObject LevelEndPanel;

    private void OnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.GameSuccess:
                LevelSuccess();
                break;
            case GameManager.GameState.GameOver:
                LevelFailed();
                break;
        }
    }

    private void LevelFailed()
    {
        LevelEndPanel.SetActive(true);
        LevelEndPanel.transform.Find("retryText").gameObject.SetActive(true);
  }

    private void LevelSuccess()
    {
        LevelEndPanel.SetActive(true);
        LevelEndPanel.transform.Find("continueText").gameObject.SetActive(true);
    }

    private void Start()
    {
        GameManager.GameStateUpdated += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.GameStateUpdated -= OnGameStateChanged;
    }
}