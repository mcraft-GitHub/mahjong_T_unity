using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// TitleScene を管理するクラス
/// </summary>
public class TitleManager : MonoBehaviour
{
    [SerializeField] private FadeControl _fadeControl;
    [SerializeField] private Button _startButton;

    void Start()
    {
        if (_startButton != null)
            _startButton.onClick.AddListener(() => _fadeControl.BeginFadeToScene("GameScene"));
    }
}
