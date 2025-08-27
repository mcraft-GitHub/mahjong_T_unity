using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ResultScene を管理するクラス
/// </summary>
public class ResultManager : MonoBehaviour
{
    [SerializeField] private TMP_Text finalTimeText;

    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button backToTitleButton;

    void Start()
    {
        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(ChangeGameScene);
        if (backToTitleButton != null)
            backToTitleButton.onClick.AddListener(ChangeTitleScene);

        SetFinalTimeText();
    }

    /// <summary>
    /// ゲーム完了時経過時間の設定
    /// </summary>
    private void SetFinalTimeText()
    {
        int minutes = GameResultKeeper.Instance.GetMinutes();
        int seconds = GameResultKeeper.Instance.GetSeconds();
        if (finalTimeText != null) finalTimeText.text = $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// GameScene へ シーン遷移
    /// </summary>
    private void ChangeGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// TitleScene へ シーン遷移
    /// </summary>
    private void ChangeTitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
