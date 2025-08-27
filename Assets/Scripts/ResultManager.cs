using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ResultScene ���Ǘ�����N���X
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
    /// �Q�[���������o�ߎ��Ԃ̐ݒ�
    /// </summary>
    private void SetFinalTimeText()
    {
        int minutes = GameResultKeeper.Instance.GetMinutes();
        int seconds = GameResultKeeper.Instance.GetSeconds();
        if (finalTimeText != null) finalTimeText.text = $"{minutes:00}:{seconds:00}";
    }

    /// <summary>
    /// GameScene �� �V�[���J��
    /// </summary>
    private void ChangeGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// TitleScene �� �V�[���J��
    /// </summary>
    private void ChangeTitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
