using UnityEngine;

/// <summary>
/// 線のクリックを処理するクラス
/// </summary>
public class LineClickHandler : MonoBehaviour
{
    private LineManager _lineManager;
    private GameManager _gameManager;


    private void Awake()
    {
        if (_lineManager == null)
            _lineManager = FindAnyObjectByType<LineManager>();
        if (_gameManager == null)
            _gameManager = FindAnyObjectByType<GameManager>();
    }

    private void OnMouseDown()
    {
        if (_lineManager == null || _gameManager == null) return;

        // クリック通知
        _lineManager.NotifyLineClicked(gameObject, _gameManager);
    }

    /// <summary>
    /// コライダーの有効/無効切り替え
    /// </summary>
    /// <param name="value"></param>
    public void SetColliderActive(bool value)
    {
        var col = GetComponent<BoxCollider>();
        if (col != null)
        {
            col.enabled = value;
        }
    }
}
