using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class Client : MonoBehaviour
{
    public static Action<byte[]> OnDataHandle;
    public static Action<int> OnLost;
    public static Action OnStart;
    public static Action OnTimeOut;
    public static Action<byte[]> OnFoundSecretMessage;
    
    private Udp udp = new Udp();

    private int _frame = 0x00;
    private byte _ack = 0x00;
    private byte _input = 0x00;

    private double _timeBetweenPacketSendAndRecived = 0f;
    
    public static bool GameRolling { get; private set; } = false;
    public float Timer { get; private set; } = 0;
    [SerializeField] private float _timeBetweenMessagesSent = 0.5f;

    private bool _validDataRecived = true;

    private bool _lostGame = true;

    private int _sendAtempts = 0;

    private void OnEnable()
    {
        udp.OnRecivedData += ListenDataRecived;
        
        udp.Connect();
    }
    
    private void OnDisable()
    {
        udp.OnRecivedData -= ListenDataRecived;
    }

    private void Update()
    {
        //Debug.LogWarning($"Input : {_input} {(byte)(_input >> 1)} {(byte)(_input << 7)} {(byte)((_frame << 1) | (_input >> 1))} {(byte)((_input << 7) | (_ack))}");

        Timer -= Time.deltaTime;
        _timeBetweenPacketSendAndRecived += Time.deltaTime;

        if (_timeBetweenPacketSendAndRecived >= 0.5 && GameRolling)
        {
            _sendAtempts++;
            SendFrame(_frame, _ack, _input);
        }

        if (_sendAtempts >= 5)
        {
            _lostGame = true;
            GameRolling = false;
            OnTimeOut?.Invoke();
        }

        if (GameRolling && Timer <= 0 && _validDataRecived)
        {
            Timer = _timeBetweenMessagesSent;
            // _input = InputManager.GetCurrentInput(); // Uncoment in case you want to controll the player
            _input = PlayerAI.GetCurrentInput(); // Coment is you wish to controll the player
            SendFrame(_frame, _ack, _input);
            _sendAtempts = 0;
        }

        if (Input.GetButtonDown("Submit") && _lostGame)
        {
            GameRolling = true;
            _lostGame = false;
            _validDataRecived = true;

            _frame = 0x00;
            _ack = 0x00;
            _input = 0x00;

            OnStart?.Invoke();
        }
    }

    private void SendFrame(int frame, byte ack, byte input)
    {
        print($"Current: {frame} : {ack} : {input}");
        _timeBetweenPacketSendAndRecived = 0.0;

        _validDataRecived = false;

        byte[] packet = {(byte)((_frame << 1) | (_input >> 1)), (byte)((_input << 7) | (_ack))};

        udp.SendData(packet);
    }

    private void ListenDataRecived(byte[] data)
    {
        _timeBetweenPacketSendAndRecived = 0.0;
        
        if (data.Length < 3)
        {
            ResendFrame();
            return;
        }
        
        byte keyword = (byte)(data[0] ^ ((_frame << 1) | (_input >> 1)));
        Debug.Log($"Keyword : {keyword}");
        byte[] decryptedData = new byte[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            decryptedData[i] = (byte) (data[i] ^ keyword);
        }
        

        var aux = "";
        foreach (var b in decryptedData)
        {
            aux += b + "  ";
        }
        Debug.Log(aux);

        if ((data.Length / 3) - 1 != decryptedData[2])
        {
            ResendFrame();
            return;
        }
        
        if (_frame == 127)
        {
            OnFoundSecretMessage?.Invoke(decryptedData);
            udp.Disconnect(); 
            this.gameObject.SetActive(false);
        }
        
        HandleData(decryptedData);

        _frame++;
        _ack = (byte)(decryptedData[1] ^ (_input << 7));
        _validDataRecived = true;
    }

    private void ResendFrame()
    {
        Timer = _timeBetweenMessagesSent;
        SendFrame(_frame, _ack, _input);
    }

    private void HandleData(byte[] data)
    {
        bool hasPlayer = false;
        for (int i = 0; i < data[2]; i++)
        {
            if (data[((i * 3) + 3)] == 0)
            {
                hasPlayer = true;
            }
        }

        if (!hasPlayer)
        {
            GameRolling = false;
            _lostGame = true;
            
            Debug.Log("You Lost");
            
            OnLost?.Invoke(_frame);
        }
        
        OnDataHandle?.Invoke(data);
    }
}

public class Udp
{
    public UdpClient socket;
    public IPEndPoint endPoint;

    public Action<byte[]> OnRecivedData;
    
    public Udp()
    {
        endPoint = new IPEndPoint(IPAddress.Parse("18.219.219.134"), 1981);
    }

    public void Connect()
    {
        socket = new UdpClient();

        socket.Connect(endPoint);
        socket.BeginReceive(RecieveCallBack, null);
    }

    public void Disconnect()
    {
        socket.Close();
    }
    

    public void SendData(byte[] _packet)
    {
        var aux = "";
        foreach (var b in _packet)
        {
            aux += b + "  ";
        }
        Debug.Log(aux + " :  " + _packet.Length);

        try
        {
            if (socket != null)
            {
                socket.BeginSend(_packet, _packet.Length, null, null);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void RecieveCallBack(IAsyncResult _result)
    {
        try
        {
            byte[] _data = socket.EndReceive(_result, ref endPoint);
            socket.BeginReceive(RecieveCallBack, null);

            HandleData(_data);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void HandleData(byte[] data)
    {
        OnRecivedData?.Invoke(data);
    }
}
