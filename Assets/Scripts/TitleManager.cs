using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// TitleScene ���Ǘ�����N���X
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
    /// GameScene �� �V�[���J��
    /// </summary>
    void ChangeGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}
