using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;


public class Client : MonoBehaviour
{
    public static Action<byte[]> OnDataHandle;
    
    private Udp udp = new Udp();

    private int _frame = 0x00;
    private byte _ack = 0x00;
    private byte _input = 0x00;
    
    public static bool GameRolling { get; private set; } = false;
    private float _timer = 0;
    [SerializeField] private float _timeBetweenMessagesSent = 0.5f;

    private bool _validDataRecived = true;

    private bool _lostGame = false;

    private void OnEnable()
    {
        udp.OnRecivedData += ListenDataRecived;
        
        udp.Connect(5005);
    }
    
    private void OnDisable()
    {
        udp.OnRecivedData -= ListenDataRecived;
    }

    private void Update()
    {
        //Debug.LogWarning($"Input : {_input} {(byte)(_input >> 1)} {(byte)(_input << 7)} {(byte)((_frame << 1) | (_input >> 1))} {(byte)((_input << 7) | (_ack))}");

        _timer -= Time.deltaTime;

        if (GameRolling && _timer <= 0 && _validDataRecived)
        {
            _timer = _timeBetweenMessagesSent;
            _input = InputManager.GetCurrentInput();
            SendFrame(_frame, _ack, _input);
        }
        
        if (Input.GetButtonDown("Submit"))
        {
            GameRolling = true;
            
            //SendFrame(_frame, _ack);
        }
    }

    private void SendFrame(int frame, byte ack, byte input)
    {
//        print("frame " + frame);

        _validDataRecived = false;

        //byte[] packet = new byte[] {(byte)((frame << 1)), (byte)(ack)};
        byte[] packet = {(byte)((_frame << 1) | (_input >> 1)), (byte)((_input << 7) | (_ack))};

        udp.SendData(packet);
    }

    private void ListenDataRecived(byte[] data)
    {
        if (data.Length < 3)
        {
            Debug.LogError("Insuficient Data!");
            ResendFrame();
            return;
        }
        
        byte keyword = (byte)(data[0] ^ ((_frame << 1) | (_input >> 1)));

//        Debug.Log("Keyword : " + keyword);
        
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
            Debug.LogError($"number of objects do not match {(data.Length / 3) - 1 } {decryptedData[2]}");
            ResendFrame();
            return;
        }

        HandleData(decryptedData);
        
        _frame++;
        _ack = (byte)(decryptedData[1] ^ (_input << 7));
        _validDataRecived = true;
    }

    private void ResendFrame()
    {
        Debug.LogError("Bad Frame!");
        Debug.LogError($"Resending frame {_frame} {_input} {_ack}");
        
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

            Debug.Log("You Lost");
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
    
    public static byte[] BitArrayToByteArray(BitArray bits)
    {
        byte[] ret = new byte[(bits.Length - 1) / 8 + 1];
        bits.CopyTo(ret, 0);
        return ret;
    }

    public void Connect(int _localPort)
    {
        socket = new UdpClient();

        socket.Connect(endPoint);
        socket.BeginReceive(RecieveCallBack, null);

        // BitArray bit = new BitArray(new bool[]
        // {
        //     false, false, false, false, false, false, false, 
        //     false, false,
        //     false, false, false, false, false, false, false
        // });
        //
        // byte[] packet = BitArrayToByteArray(bit);
        
        // byte[] packet = BitConverter.GetBytes((short)(00));
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
        catch
        {
            //
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
        
        // var keyword = data[0];
        //
        // var aux = " ";
        // foreach (var b in data)
        // {
        //     aux += b + "  ";
        // }
        //
        // Debug.Log(aux);
        //
        // byte[] decripted = data;
        //
        // for (int i = 0; i < data.Length; i++)
        // {
        //     decripted[i] = (byte)(data[i] ^ keyword);
        // }
        //
        // aux = " ";
        // foreach (var b in decripted)
        // {
        //     aux += b + "  ";
        // }
        //
        // Debug.Log(aux);
        //
        // byte[] packet = new byte[] {0x00, 0x00};
        // packet[0] += 2;
        // packet[1] = decripted[1];
    }

    private void PrintBits(BitArray bitArray)
    {
        var aux = "";

        for (int i = 0, j = 1; i < bitArray.Length; i++, j++)
        {
            aux = (bitArray[i] ? '1' : '0') + aux;

            if (j == 4)
            {
                j = 0;
                aux = "  " + aux;
            }
        }

        Debug.Log(aux);
    }
}
