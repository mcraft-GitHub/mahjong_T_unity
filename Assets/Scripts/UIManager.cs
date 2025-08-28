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
    [SerializeField] private TMP_Text _pairsLeftText;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] public Button _restartButton;


    /// <summary>
    /// �o�ߎ��Ԃ̍X�V
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
    /// �c��y�A���\���̍X�V
    /// </summary>
    public void UpdateUI(int totalPairs, int matchedPairs)
    {
        if (_pairsLeftText != null) _pairsLeftText.text = (totalPairs - matchedPairs).ToString();
    }
}
