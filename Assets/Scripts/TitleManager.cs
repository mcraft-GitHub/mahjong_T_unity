using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// TitleScene ���Ǘ�����N���X
/// </summary>
public class TitleManager : MonoBehaviour
{
    [SerializeField] private FadeControl _fadeControl;
    [SerializeField] private Button _startButton;

    void Start()
    {
        if (_startButton != null)
            _startButton.onClick.AddListener(OnStartButtonClicked);
    }

    /// <summary>
    /// ���b�p�[�֐�
    /// </summary>
    private void OnStartButtonClicked()
    {
        StartCoroutine(ChangeGameScene());
    }

    /// <summary>
    /// GameScene �� �V�[���J��
    /// </summary>
    private IEnumerator ChangeGameScene()
    {
        yield return StartCoroutine(_fadeControl.FadeOutScene());
        SceneManager.LoadScene("GameScene");
    }
}
