using System;
using System.Collections.Generic;
using UnityEngine;

public class DecryptMessage : MonoBehaviour
{
    [SerializeField] private Sprite _sprite;
    [SerializeField] private Color _color;

    private Queue<ObjectInstance> _objectInstances;
    
    private byte[] _data = null;

    private bool _handled = false;

    private void Start()
    {
        _objectInstances = new Queue<ObjectInstance>();
    }

    private void OnEnable()
    {
        Client.OnFoundSecretMessage += OnFoundSecretMessage;
    }

    private void OnDisable()
    {
        Client.OnFoundSecretMessage -= OnFoundSecretMessage;   
    }

    private void Update()
    {
        if (_handled) return;
        if (_data == null) return;
        
        HandleData();
        TurnEverythingOffExceptSolutions();

        _handled = false;
    }

    private void OnFoundSecretMessage(byte[] data)
    {
        _data = data;
    }

    private void TurnEverythingOffExceptSolutions()
    {
        var objs = FindObjectsOfType<GameObject>();

        foreach (var o in objs)
        {
            if (!o.CompareTag("Solution"))
            {
                o.SetActive(false);
            }
        }
    }

    private void HandleData()
    {
        foreach (var objectInstance in _objectInstances)
        {
            objectInstance.ResetFrameStatus();
        }

        for (int i = 0; i < _data[2]; i++)
        {
            if(((i * 3) + 3) + 2 < _data.Length)
                MoveObject(_data[((i * 3) + 3) + 0], _data[((i * 3) + 3) + 1], _data[((i * 3) + 3)+ 2]);
        }

        foreach (var objectInstance in _objectInstances)
        {
            if (!objectInstance.ChangedThisFrame)
            {
                objectInstance.Move(new Vector2(-10, -10));
            }
        }
    }

    public void MoveObject(byte id, byte horizontal, byte vertical)
    {
        ObjectInstance obj = null;

        if (_objectInstances.Count <= 0)
        {
            obj = CreateObjectInstance(id);
        }
        else if (_objectInstances.Peek().ChangedThisFrame)
        {
            obj = CreateObjectInstance(id);
        }
        else
        {
            obj = _objectInstances.Dequeue();
        }

        obj.Move(new Vector2(horizontal, vertical));
        
        _objectInstances.Enqueue(obj);
    }

    private ObjectInstance CreateObjectInstance(byte id)
    {
        string name = $"[{id} - Solution - {_objectInstances.Count}]";
        
        ObjectInstance obj = new GameObject(name).AddComponent<ObjectInstance>();
        obj.gameObject.AddComponent<SpriteRenderer>().sprite = _sprite;
        
        obj.Setup(id, _color);
        obj.gameObject.tag = "Solution";

        return obj;
    }
}