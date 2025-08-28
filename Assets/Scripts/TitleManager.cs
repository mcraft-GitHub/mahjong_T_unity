using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// TitleScene を管理するクラス
/// </summary>
public class TitleManager : MonoBehaviour
{
    [SerializeField] private Button _startButton;

    void Start()
    {
        if (_startButton != null)
            _startButton.onClick.AddListener(ChangeGameScene);
    }

    /// <summary>
    /// GameScene へ シーン遷移
    /// </summary>
    void ChangeGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
