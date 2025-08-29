using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ResultScene を管理するクラス
/// </summary>
public class ResultManager : MonoBehaviour
{
    [SerializeField] private FadeControl _fadeControl;

    [SerializeField] private TMP_Text finalTimeText;

    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button backToTitleButton;

    void Start()
    {
        ResultSceneStart();

        if (playAgainButton != null)
            playAgainButton.onClick.AddListener(BeginFadeToGameScene);
        if (backToTitleButton != null)
            backToTitleButton.onClick.AddListener(BeginFadeToTitleScene);

        SetFinalTimeText();
    }

    /// <summary>
    /// ゲーム完了時経過時間の設定
    /// </summary>
    private void SetFinalTimeText()
    {
        if (GameResultKeeper._Instance == null) return;

        if (finalTimeText != null)
            finalTimeText.text = GameResultKeeper._Instance.MakeResultTime();
    }

    /// <summary>
    /// フェードイン ラッパー関数
    /// </summary>
    private void ResultSceneStart()
    {
        StartCoroutine(ResultFadeIn());
    }

    /// <summary>
    /// ゲームシーン遷移とフェードアウト ラッパー関数
    /// </summary>
    private void BeginFadeToGameScene()
    {
        StartCoroutine(ChangeGameScene());
    }

    /// <summary>
    /// タイトルシーン遷移とフェードアウト ラッパー関数
    /// </summary>
    private void BeginFadeToTitleScene()
    {
        StartCoroutine(ChangeTitleScene());
    }

    /// <summary>
    /// フェードイン
    /// </summary>
    private IEnumerator ResultFadeIn()
    {
        yield return StartCoroutine(_fadeControl.FadeInScene());
    }

    /// <summary>
    /// GameScene へ シーン遷移
    /// </summary>
    private IEnumerator ChangeGameScene()
    {
        yield return StartCoroutine(_fadeControl.FadeOutScene());
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// TitleScene へ シーン遷移
    /// </summary>
    private IEnumerator ChangeTitleScene()
    {
        yield return StartCoroutine(_fadeControl.FadeOutScene());
        SceneManager.LoadScene("TitleScene");
    }
}
