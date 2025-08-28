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
    [SerializeField] private TMP_Text _pairsLeftText;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] public Button _restartButton;


    /// <summary>
    /// 経過時間の更新
    /// </summary>
    public IEnumerator UpdateTimer(bool gameActive)
    {
        while (gameActive)
        {
            _timerText.text = GameResultKeeper.Instance.MakeResultTime();
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// 残りペア数表示の更新
    /// </summary>
    public void UpdateUI(int totalPairs, int matchedPairs)
    {
        if (_pairsLeftText != null) _pairsLeftText.text = (totalPairs - matchedPairs).ToString();
    }
}
