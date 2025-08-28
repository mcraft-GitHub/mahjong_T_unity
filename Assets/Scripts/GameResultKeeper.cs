using UnityEngine;

/// <summary>
/// ���U���g�̒l�������Ă����N���X
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
    /// �o�ߎ��Ԍv�Z�p startTime �̐ݒ�
    /// </summary>
    public void StartTime()
    {
        startTime = Time.time;
    }

    /// <summary>
    /// �ŏI�o�߃^�C���̌v�Z
    /// GameScene ���� ResultScene �֑J�ڂ���ۂɎg�p
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
    /// �o�߂���������n��
    /// </summary>
    /// <returns> �o�� �� </returns>
    public int GetMinutes()
    {
        return minutes;
    }

    /// <summary>
    /// �o�߂����b����n��
    /// </summary>
    /// <returns> �o�� �b </returns>
    public int GetSeconds()
    {
        return seconds;
    }
}
