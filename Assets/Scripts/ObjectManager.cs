using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectManager : MonoBehaviour
{
    [SerializeField] private Color _playerColor = Color.blue;
    [SerializeField] private Color _shotColor = Color.cyan;

    // [SerializeField] private ObjectInstance _object;

    private Dictionary<byte, Queue<ObjectInstance>> _objectInstances;
    private Dictionary<byte, Color> _colorsDictionary;
    private List<ObjectInstance> _allObjects;

    private byte[] _data = null;
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _playerSprite;

    private void Start()
    {
        _objectInstances = new Dictionary<byte, Queue<ObjectInstance>>();
        _colorsDictionary = new Dictionary<byte, Color>();
        _allObjects = new List<ObjectInstance>();
        
        _objectInstances.Add(0x00, new Queue<ObjectInstance>());
        _objectInstances.Add(0x01, new Queue<ObjectInstance>());
        
        _colorsDictionary.Add(0x00, _playerColor);
        _colorsDictionary.Add(0x01, _shotColor);
    }

    private void OnEnable()
    {
        Client.OnDataHandle += ReciveDataHandle;
    }

    private void OnDisable()
    {
        Client.OnDataHandle -= ReciveDataHandle;
    }

    private void Update()
    {
        if (!Client.GameRolling) return;
        
        if (_data == null) return;
        
        HandleData();
    }

    private void ReciveDataHandle(byte[] data)
    {
        _data = data;

        // HandleData();
        
        // for (int i = 0; i < data[2]; i++)
        // {
        //     MoveObject(data[((i * 3) + 3) + 0], data[((i * 3) + 3) + 1], data[((i * 3) + 3)+ 2]);
        // }
    }

    private void HandleData()
    {
        foreach (var objectInstance in _allObjects)
        {
            objectInstance.ResetFrameStatus();
        }

        for (int i = 0; i < _data[2]; i++)
        {
            if(((i * 3) + 3) + 2 < _data.Length)
                MoveObject(_data[((i * 3) + 3) + 0], _data[((i * 3) + 3) + 1], _data[((i * 3) + 3)+ 2]);
        }
    }

    public void MoveObject(byte id, byte horizontal, byte vertical)
    {
        // Debug.Log($"-> {id} : {horizontal} , {vertical}");

        if (!_objectInstances.ContainsKey(id))
        {
            _objectInstances.Add(id, new Queue<ObjectInstance>());
        }

        ObjectInstance obj = null;

        if (_objectInstances[id].Count <= 0)
        {
            obj = CreateObjectInstance(id);
        }
        else if (_objectInstances[id].Peek().ChangedThisFrame)
        {
            obj = CreateObjectInstance(id);
        }
        else
        {
            obj = _objectInstances[id].Dequeue();
        }

        obj.Move(new Vector2(horizontal, vertical));
        
        _objectInstances[id].Enqueue(obj);
    }

    private ObjectInstance CreateObjectInstance(byte id)
    {
        string name = "";
        Sprite currentSprite = _playerSprite;

        if (id == 0)
        {
            name = "player";
        }
        else if(id == 1)
        {
            name = "player-shot";
        }
        else
        {
            name = $"{id}-instance";
            currentSprite = _defaultSprite;
        }
        
        ObjectInstance obj = new GameObject(name).AddComponent<ObjectInstance>();
        obj.gameObject.AddComponent<SpriteRenderer>().sprite = currentSprite;

        obj.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
        
        if (!_colorsDictionary.ContainsKey(id))
        {
            _colorsDictionary.Add(id, new Color(Random.Range(0f,1f), Random.Range(0f,1f), Random.Range(0f,1f), 1f));
        }
        
        //_objectInstances[id].Enqueue(obj);
        obj.Setup(id, _colorsDictionary[id]);
        
        _allObjects.Add(obj);
        
        return obj;
    }
}