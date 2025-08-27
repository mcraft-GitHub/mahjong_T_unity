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

    private int elapsed = 0;
    private int minutes = 0;
    private int seconds = 0;
    private float startTime;

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
    public void MakeResultTime()
    {
        elapsed = Mathf.FloorToInt(Time.time - startTime);
        minutes = (int)(elapsed / 60f);
        seconds = (int)(elapsed % 60f);
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
