using UnityEngine;

/// <summary>
/// リザルトの値を持っておくクラス
/// </summary>
public class GameResultKeeper : MonoBehaviour
{
    public static GameResultKeeper _Instance 
    {
        get;
        private set;
    }
    private const int _MINUTES_PER_HOUR = 60;

    private int _elapsed = 0;
    private int _minutes = 0;
    private int _seconds = 0;
    private float _startTime;
    private string _timeText;

    void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _Instance = this;
        DontDestroyOnLoad(this);

        StartTime();
    }

    /// <summary>
    /// 経過時間計算用 _startTime の設定
    /// </summary>
    public void StartTime()
    {
        _startTime = Time.time;
    }

    /// <summary>
    /// 最終経過タイムの計算
    /// GameScene から ResultScene へ遷移する際に使用
    /// </summary>
    public string MakeResultTime()
    {
        _elapsed = Mathf.FloorToInt(Time.time - _startTime);
        _minutes = _elapsed / _MINUTES_PER_HOUR;
        _seconds = _elapsed % _MINUTES_PER_HOUR;
        _timeText = $"{_minutes:00}:{_seconds:00}";

        return _timeText;
    }

    /// <summary>
    /// 経過した分数を渡す
    /// </summary>
    /// <returns> 経過 分 </returns>
    public int GetMinutes()
    {
        return _minutes;
    }

    /// <summary>
    /// 経過した秒数を渡す
    /// </summary>
    /// <returns> 経過 秒 </returns>
    public int GetSeconds()
    {
        return _seconds;
    }
}
