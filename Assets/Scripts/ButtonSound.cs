using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ボタンを押した際、共通の音を鳴らす
/// </summary>
public class ButtonSound : MonoBehaviour
{
    private void Awake()
    {
        Button button = GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySE("SeConfirmClick");
        });
    }
}
