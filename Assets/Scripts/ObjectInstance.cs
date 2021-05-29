using UnityEngine;

public class ObjectInstance : MonoBehaviour
{
    private int _id;
    private Vector2 _position;
    private Color _color;
    private SpriteRenderer _renderer;
    private bool _changedThisFrame = false;
    public bool ChangedThisFrame => _changedThisFrame;

    private void Start()
    {
        if(_renderer == null)
            _renderer = this.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        this.transform.position = _position;
    }

    public void Setup(int id, Color color)
    {
        if (_renderer == null)
        {
            _renderer = this.GetComponent<SpriteRenderer>();
        }
        
        _id = id;
        _color = color;
        _renderer.color = _color;
        _changedThisFrame = false;
    }

    public void Move(Vector2 position)
    {
        _position = position;
        _changedThisFrame = true;
    }

    public void ResetFrameStatus()
    {
        _changedThisFrame = false;
    }
}