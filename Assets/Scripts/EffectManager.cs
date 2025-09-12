using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EffectManager;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// エフェクト管理クラス
/// </summary>
public class EffectManager : MonoBehaviour
{
    public static EffectManager _Instance { get; private set; }

    [System.Serializable]
    public class EffectEntry
    {
        public string _key;              // 識別名
        public GameObject _prefab;       // エフェクトPrefab
        public float _delayTime = 2f;
        public int _poolSize = 5;        // プールする数
    }

    [Header("Effect List")]
    [SerializeField] private List<EffectEntry> effectEntries = new List<EffectEntry>();

    private Dictionary<string, Queue<GameObject>> _effectPools = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // プール初期化
        foreach (var entry in effectEntries)
        {
            var queue = new Queue<GameObject>();
            for (int i = 0; i < entry._poolSize; i++)
            {
                GameObject obj = Instantiate(entry._prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            _effectPools[entry._key] = queue;
        }
    }

    /// <summary>
    /// エフェクトを再生する
    /// </summary>
    public void PlayEffect(string key, Vector3 position)
    {
        if (!_effectPools.ContainsKey(key)) return;

        var entry = effectEntries.Find(effectEntry => effectEntry._key == key);
        if (entry == null) return;

        var pool = _effectPools[key];
        if (pool.Count == 0)
        {
            // プール不足なら追加生成

            GameObject obj = Instantiate(entry._prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        GameObject effect = pool.Dequeue();
        position.z -= 1f;
        effect.transform.position = position;
        effect.SetActive(true);

        var ps = effect.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            StartCoroutine(ReturnToPool(key, effect, ps.main.duration));
        }
        else
        {
            StartCoroutine(ReturnToPool(key, effect, entry._delayTime));
        }
    }

    /// <summary>
    /// エフェクトをプールに戻す
    /// </summary>
    /// <param name="key"></param>
    /// <param name="effect"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator ReturnToPool(string key, GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.SetActive(false);
        _effectPools[key].Enqueue(effect);
    }
}
