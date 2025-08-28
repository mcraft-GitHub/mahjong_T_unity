using UnityEngine;

/// <summary>
/// ���U���g�̒l�������Ă����N���X
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
    /// �o�ߎ��Ԍv�Z�p _startTime �̐ݒ�
    /// </summary>
    public void StartTime()
    {
        _startTime = Time.time;
    }

    /// <summary>
    /// �ŏI�o�߃^�C���̌v�Z
    /// GameScene ���� ResultScene �֑J�ڂ���ۂɎg�p
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
    /// �o�߂���������n��
    /// </summary>
    /// <returns> �o�� �� </returns>
    public int GetMinutes()
    {
        return _minutes;
    }

    /// <summary>
    /// �o�߂����b����n��
    /// </summary>
    /// <returns> �o�� �b </returns>
    public int GetSeconds()
    {
        return _seconds;
    }
}
