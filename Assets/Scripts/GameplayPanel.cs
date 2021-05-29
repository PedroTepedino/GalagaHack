using TMPro;
using UnityEngine;

public class GameplayPanel : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TextMeshProUGUI _text;

    private bool _lost = true;

    private void Awake()
    {
        Client.OnLost += OnLost;  
        Client.OnStart += OnStart;
        Client.OnTimeOut += OnTimeOut;
    }

    private void OnDestroy()
    {
        Client.OnLost -= OnLost; 
        Client.OnStart -= OnStart;
        Client.OnTimeOut -= OnTimeOut;
    }

    private void OnStart()
    {
        _lost = false;
    }

    private void OnLost(int frame)
    {
        _lost = true;

        if (frame == 127)
        {
            _text.text = "YOU WON!";
        }
        else
        {
            _text.text = $"You Lost on frame : {frame}";
        }
    }
    
    private void OnTimeOut()
    {
        _lost = true;
        _text.text = "Server TIMEOUT";
    }

    private void Update()
    {
        _panel.SetActive(_lost);
    }

    private void OnValidate()
    {
        if (!_panel) _panel = this.transform.GetChild(0).gameObject;

        if (!_text) _text = this.GetComponentInChildren<TextMeshProUGUI>();
    }
}