using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI ���Ǘ�����N���X
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text pairsLeftText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] public Button restartButton;

    private int matchedPairs = 0;

    /// <summary>
    /// �o�ߎ��Ԃ̍X�V
    /// </summary>
    public IEnumerator UpdateTimer(bool gameActive)
    {
        while (gameActive)
        {
            timerText.text = GameResultKeeper.Instance.MakeResultTime();
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// �c��y�A���\���̍X�V
    /// </summary>
    public void UpdateUI(int totalPairs)
    {
        if (pairsLeftText != null) pairsLeftText.text = (totalPairs - matchedPairs).ToString();
    }
}
