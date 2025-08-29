using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class FadeControl : MonoBehaviour
{
    [SerializeField] private CanvasGroup _fadePanel;
    [SerializeField] private bool _isBlink = false;
    [SerializeField] private float _blinkSpeed = 1f;
    [SerializeField] private float _maxBlinkAlpha = 0.5f;

    private float _fadeDuration = 1f;
    private bool _isFade = false;
    private float _blinkAlpha = 0f;
    private int _direction = 1;

    void Start()
    {
        StartCoroutine(FadeInScene());
    }

    private void Update()
    {
        PanelBlink();
    }

    /// <summary>
    /// フェードイン ラッパー関数
    /// </summary>
    public void SceneStart()
    {
        StartCoroutine(FadeInScene());
    }

    /// <summary>
    /// フェードイン
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeInScene()
    {
        _isFade = true;
        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _fadePanel.alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeDuration);
            yield return null;
        }
        _fadePanel.alpha = 0f;
        _isFade = false;
    }

    /// <summary>
    /// フェードアウト
    /// </summary>
    /// <returns></returns>
    private IEnumerator FadeOutScene()
    {
        _isFade = true;
        float elapsed = 0f;
        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            _fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsed / _fadeDuration);
            yield return null;
        }
        _fadePanel.alpha = 1f;
        _isFade = false;
    }

    /// <summary>
    /// 画面の黒色に点滅
    /// </summary>
    private void PanelBlink()
    {
        if (_isFade || !_isBlink) return;

        _blinkAlpha += _direction * _blinkSpeed * Time.deltaTime;

        if (_blinkAlpha >= _maxBlinkAlpha)
        {
            _blinkAlpha = _maxBlinkAlpha;
            _direction = -1;
        }
        else if (_blinkAlpha <= 0f)
        {
            _blinkAlpha = 0f;
            _direction = 1;
        }

        _fadePanel.alpha = _blinkAlpha;
    }

    /// <summary>
    /// シーン遷移とフェードアウト ラッパー関数
    /// </summary>
    public void BeginFadeToScene(string sceneName)
    {
        StartCoroutine(ChangeScene(sceneName));
    }

    /// <summary>
    /// 他Scene へ シーン遷移
    /// </summary>
    private IEnumerator ChangeScene(string sceneName)
    {
        yield return StartCoroutine(FadeOutScene());
        SceneManager.LoadScene(sceneName);
    }
}
