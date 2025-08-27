using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// TitleScene を管理するクラス
/// </summary>
public class TitleManager : MonoBehaviour
{
    [SerializeField] private Button startButton;

    void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(ChangeGameScene);
    }

    /// <summary>
    /// GameScene へ シーン遷移
    /// </summary>
    void ChangeGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
