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
    public static EffectManager _Instance
    {
        get;
        private set;
    }

    [System.Serializable]
    public class EffectConfig
    {
        public string _id;
        public GameObject _prefab;
        public float _defaultLifetime = 2f;
        public int _initialPoolSize = 5;
    }

    [SerializeField] private float _zOffset = -1f;

    [Header("Effect List")]
    [SerializeField] private List<EffectConfig> _effectConfigs = new List<EffectConfig>();

    private Dictionary<string, Queue<GameObject>> _effectPoolsById = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, EffectConfig> _effectConfigsById = new();

    void Awake()
    {
        if(!InitializeSingleton()) return;

        foreach (var config in _effectConfigs)
        {
            // キャッシュ
            _effectConfigsById[config._id] = config;

            // プール初期化
            var queue = new Queue<GameObject>();
            for (var i = 0; i < config._initialPoolSize; i++)
            {
                GameObject obj = Instantiate(config._prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            _effectPoolsById[config._id] = queue;
        }
    }

    /// <summary>
    /// エフェクトを再生する
    /// </summary>
    public void PlayEffect(string id, Vector3 position)
    {
        if (!_effectPoolsById.ContainsKey(id)) return;
        if (!_effectConfigsById.TryGetValue(id, out var config)) return;

        var pool = _effectPoolsById[id];
        if (pool.Count == 0)
        {
            // プール不足なら追加生成
            GameObject obj = Instantiate(config._prefab, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }

        GameObject effectObject = pool.Dequeue();
        position.z += _zOffset;
        effectObject.transform.position = position;
        effectObject.SetActive(true);

        var ps = effectObject.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            StartCoroutine(ReturnToPool(id, effectObject, ps.main.duration));
        }
        else
        {
            StartCoroutine(ReturnToPool(id, effectObject, config._defaultLifetime));
        }
    }

    /// <summary>
    /// エフェクトをプールに戻す
    /// </summary>
    /// <param name="id"></param>
    /// <param name="effectObject"></param>
    /// <param name="delay"></param>
    /// <returns></returns>
    private IEnumerator ReturnToPool(string id, GameObject effectObject, float delay)
    {
        yield return new WaitForSeconds(delay);
        effectObject.SetActive(false);
        _effectPoolsById[id].Enqueue(effectObject);
    }

    /// <summary>
    /// インスタンス破棄処理
    /// </summary>
    /// <returns></returns>
    private bool InitializeSingleton()
    {
        if (_Instance == null)
        {
            _Instance = this;
            DontDestroyOnLoad(gameObject);
            return true;
        }
        else
        {
            Destroy(gameObject);
            return false;
        }
    }
}