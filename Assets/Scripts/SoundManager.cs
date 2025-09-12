using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 音を管理するクラス
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance
    {
        get;
        private set;
    }
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _bgmSource;
    [SerializeField] private AudioSource _seSource;

    [Header("Clips")]
    [SerializeField] private AudioClip[] _bgmClips;
    [SerializeField] private AudioClip[] _seClips;

    private Dictionary<string, AudioClip> _bgmDict;
    private Dictionary<string, AudioClip> _seDict;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 辞書化
        _bgmDict = new Dictionary<string, AudioClip>();
        foreach (var clip in _bgmClips)
        {
            if (clip != null) _bgmDict[clip.name] = clip;
        }

        _seDict = new Dictionary<string, AudioClip>();
        foreach (var clip in _seClips)
        {
            if (clip != null) _seDict[clip.name] = clip;
        }
    }

    /// <summary>
    /// bgmを再生する
    /// </summary>
    /// <param name="bgm"></param>
    public void PlayBGM(string bgmName)
    {
        if (_bgmSource == null) return;
        if (_bgmDict.TryGetValue(bgmName, out var clip))
        {
            _bgmSource.clip = clip;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }
    }

    /// <summary>
    /// SEを再生する
    /// </summary>
    /// <param name="se"></param>
    public void PlaySE(string seName)
    {
        if (_seSource == null || string.IsNullOrEmpty(seName)) return;

        if (_seDict.TryGetValue(seName, out var clip))
        {
            _seSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// BGMを停止する
    /// </summary>
    public void StopBGM()
    {
        if (_bgmSource != null)
        {
            _bgmSource.Stop();
        }
    }
}