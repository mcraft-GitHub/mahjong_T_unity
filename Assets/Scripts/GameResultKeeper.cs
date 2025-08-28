using UnityEngine;

/// <summary>
/// リザルトの値を持っておくクラス
/// </summary>
public class GameResultKeeper : MonoBehaviour
{
    public static GameResultKeeper Instance 
    {
        get;
        private set;
    }
    private const int MINUTES_PER_HOUR = 60;

    private int elapsed = 0;
    private int minutes = 0;
    private int seconds = 0;
    private float startTime;
    private string timeText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);

        StartTime();
    }

    /// <summary>
    /// 経過時間計算用 startTime の設定
    /// </summary>
    public void StartTime()
    {
        startTime = Time.time;
    }

    /// <summary>
    /// 最終経過タイムの計算
    /// GameScene から ResultScene へ遷移する際に使用
    /// </summary>
    public string MakeResultTime()
    {
        elapsed = Mathf.FloorToInt(Time.time - startTime);
        minutes = elapsed / MINUTES_PER_HOUR;
        seconds = elapsed % MINUTES_PER_HOUR;
        timeText = $"{minutes:00}:{seconds:00}";

        return timeText;
    }

    /// <summary>
    /// 経過した分数を渡す
    /// </summary>
    /// <returns> 経過 分 </returns>
    public int GetMinutes()
    {
        return minutes;
    }

    /// <summary>
    /// 経過した秒数を渡す
    /// </summary>
    /// <returns> 経過 秒 </returns>
    public int GetSeconds()
    {
        return seconds;
    }
}
