using System.Globalization;
using TMPro;
using UnityEngine;

public class TimerText : MonoBehaviour
{
    [SerializeField] private Client _client;
    [SerializeField] private TextMeshProUGUI _text;

    private void Awake()
    {
        if (!_client)
        {
            this.gameObject.SetActive(false);
            Debug.LogWarning("Missing Client!");
        }
    }

    private void Update()
    {
        _text.text = _client.Timer.ToString(CultureInfo.InvariantCulture);
    }

    private void OnValidate()
    {
        if (!_text) _text = this.GetComponent<TextMeshProUGUI>();

        if (!_client) _client = FindObjectOfType<Client>();
    }
}
