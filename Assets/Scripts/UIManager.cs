using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI を管理するクラス
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text pairsLeftText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] public Button restartButton;

    private int matchedPairs = 0;

    /// <summary>
    /// 経過時間の更新
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
    /// 残りペア数表示の更新
    /// </summary>
    public void UpdateUI(int totalPairs)
    {
        if (pairsLeftText != null) pairsLeftText.text = (totalPairs - matchedPairs).ToString();
    }
}
