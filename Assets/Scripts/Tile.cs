using UnityEngine;
using System.Collections;

/// <summary>
/// 盤上の1マスを表すタイルのクラス
/// </summary>
public class Tile : MonoBehaviour
{
    public int _row
    {
        get;
        private set;
    }
    public int _col
    {
        get;
        private set;
    }
    public string _type
    {
        get;
        private set;
    }
    public bool _isMatched
    {
        get;
        private set;
    } = false;

    private Renderer _renderer;
    private Collider _collider;

    private Color _originalColor = Color.white;
    [SerializeField] private Color _selectedColor = Color.yellow;
    [SerializeField] private Color _matchedColor = new Color(1f, 0.9f, 0.6f, 1f);
    [SerializeField] private Color _hoverColor = new Color(0.9f, 0.9f, 0.9f, 1f);

    private MaterialPropertyBlock _mpb;
    private static readonly int _colorPropId = Shader.PropertyToID("_Color");

    // 回転アニメーション用
    private Coroutine _flipAnimationCoroutine;
    private const float _flipDuration = 0.5f;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _collider = GetComponent<Collider>();

        if (_renderer != null)
        {
            _mpb = new MaterialPropertyBlock();

            if (_renderer.sharedMaterial != null && _renderer.sharedMaterial.HasProperty(_colorPropId))
            {
                _originalColor = _renderer.sharedMaterial.GetColor(_colorPropId);
            }

            ApplyColor(_originalColor);
        }
    }

    /// <summary>
    /// タイル初期化
    /// </summary>
    /// <param name="row"> 行 </param>
    /// <param name="col"> 列 </param>
    /// <param name="type"> 種類 </param>
    public void Setup(int row, int col, string type)
    {
        _row = row;
        _col = col;
        _type = type;
        _isMatched = false;
        if (_collider != null) _collider.enabled = true;
        ApplyColor(_originalColor);
    }

    /// <summary>
    /// タイル選択時の表示変更
    /// </summary>
    public void Select()
    {
        if (_isMatched) return;
        ApplyColor(_selectedColor);
    }

    /// <summary>
    /// 選択解除時の表示変更
    /// </summary>
    public void Deselect()
    {
        if (_isMatched) return;
        ApplyColor(_originalColor);
    }

    /// <summary>
    /// タイルがマッチした時の処理
    /// </summary>
    public void Match()
    {
        _isMatched = true;
        ApplyColor(_matchedColor);
        if (_collider != null) _collider.enabled = false;

        FlipTile(true);

        SoundManager.Instance.PlaySE("SePairConfirmed");

        if(EffectManager._Instance != null)
        {
            EffectManager._Instance.PlayEffect("FxPairConfirmed", transform.position);
        }
    }

    /// <summary>
    /// マッチ状態を解除する
    /// </summary>
    public void Unmatch()
    {
        if (!_isMatched) return;

        _isMatched = false;

        // 見た目・操作を元に戻す
        ApplyColor(_originalColor);
        if (_collider != null)
        {
            _collider.enabled = true;
        }

        // 回転の初期化
        FlipTile(false);

        SoundManager.Instance.PlaySE("SePairCancel");
    }

    /// <summary>
    /// 色の適応
    /// </summary>
    /// <param name="c"> 色 </param>
    private void ApplyColor(Color c)
    {
        if (_renderer == null) return;
        if (_mpb == null)
        {
            _mpb = new MaterialPropertyBlock();
        }
        _mpb.SetColor(_colorPropId, c);
        _renderer.SetPropertyBlock(_mpb);
    }

    /// <summary>
    /// タイルを裏返すアニメーション
    /// </summary>
    /// <param name="isMatch"> マッチ成立(表) or マッチ解除(裏) </param>
    public void FlipTile(bool isMatch)
    {
        float targetAngle = isMatch ? 180f : 0f;
        if (_flipAnimationCoroutine != null)
        {
            StopCoroutine(_flipAnimationCoroutine);
        }
        _flipAnimationCoroutine = StartCoroutine(AnimateRotationY(targetAngle, _flipDuration));
    }

    /// <summary>
    /// Y軸回転アニメーション
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator AnimateRotationY(float targetAngle, float duration)
    {
        Quaternion startRot = transform.localRotation;
        Quaternion endRot = Quaternion.Euler(0, targetAngle, 0);

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(startRot, endRot, elapsedTime / duration);
            yield return null;
        }

        transform.localRotation = endRot;
        _flipAnimationCoroutine = null;
    }

    /// <summary>
    /// 確定状態をリセット
    /// </summary>
    public void ResetState()
    {
        _isMatched = false;

        // 色を戻す
        ApplyColor(_originalColor);

        // コライダー復活
        if (_collider != null)
        {
            _collider.enabled = true;
        }

        // 回転を戻す
        transform.localRotation = Quaternion.identity;
    }
}
