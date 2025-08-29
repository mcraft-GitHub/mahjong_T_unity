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
        ResultSceneStart();

        if (_playAgainButton != null)
            _playAgainButton.onClick.AddListener(BeginFadeToGameScene);
        if (_backToTitleButton != null)
            _backToTitleButton.onClick.AddListener(BeginFadeToTitleScene);

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

    /// <summary>
    /// �t�F�[�h�C�� ���b�p�[�֐�
    /// </summary>
    private void ResultSceneStart()
    {
        StartCoroutine(ResultFadeIn());
    }

    /// <summary>
    /// �Q�[���V�[���J�ڂƃt�F�[�h�A�E�g ���b�p�[�֐�
    /// </summary>
    private void BeginFadeToGameScene()
    {
        StartCoroutine(ChangeGameScene());
    }

    /// <summary>
    /// �^�C�g���V�[���J�ڂƃt�F�[�h�A�E�g ���b�p�[�֐�
    /// </summary>
    private void BeginFadeToTitleScene()
    {
        StartCoroutine(ChangeTitleScene());
    }

    /// <summary>
    /// �t�F�[�h�C��
    /// </summary>
    private IEnumerator ResultFadeIn()
    {
        yield return StartCoroutine(_fadeControl.FadeInScene());
    }

    /// <summary>
    /// GameScene �� �V�[���J��
    /// </summary>
    private IEnumerator ChangeGameScene()
    {
        yield return StartCoroutine(_fadeControl.FadeOutScene());
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// TitleScene �� �V�[���J��
    /// </summary>
    private IEnumerator ChangeTitleScene()
    {
        yield return StartCoroutine(_fadeControl.FadeOutScene());
        SceneManager.LoadScene("TitleScene");
    }
}
