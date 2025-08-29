using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ResultScene ���Ǘ�����N���X
/// </summary>
public class ResultManager : MonoBehaviour
{
    [SerializeField] private FadeControl _fadeControl;

    [SerializeField] private TMP_Text _finalTimeText;

    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Button _backToTitleButton;

    void Start()
    {
        _fadeControl.SceneStart();

        if (_playAgainButton != null)
            _playAgainButton.onClick.AddListener(() => _fadeControl.BeginFadeToScene("GameScene"));
        if (_backToTitleButton != null)
            _backToTitleButton.onClick.AddListener(() => _fadeControl.BeginFadeToScene("TitleScene"));

        SetFinalTimeText();
    }

    /// <summary>
    /// �Q�[���������o�ߎ��Ԃ̐ݒ�
    /// </summary>
    private void SetFinalTimeText()
    {
        if (GameResultKeeper._Instance == null) return;

        if (_finalTimeText != null)
            _finalTimeText.text = GameResultKeeper._Instance.MakeResultTime();
    }
}