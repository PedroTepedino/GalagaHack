using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Client : MonoBehaviour
{
    [SerializeField] private ObjectManager _objectManager;

    public static Action<byte[]> OnDataHandle;
    
    private UDP udp = new UDP();

    private int _frame = 0;
    private byte _ack = 0x00;
    private byte _input = 0x00;
    
    private WaitForSecondsRealtime _frameRate = new WaitForSecondsRealtime(0.1f);
    private Coroutine _gameplayCoroutine = null;

    public static bool GameRolling { get; private set; } = false;
    private float _timer = 0;
    [SerializeField] private float _timeBetweenMessagesSent = 0.5f;

    private bool _validDataRecived = true;

    private void OnEnable()
    {
        udp.OnRecivedData += ListenDataRecived;
        
        udp.Connect(5005);
        
        // SendFrame(_frame, _ack);
    }

    private IEnumerator StartGameplay()
    {
        yield return new WaitForSeconds(0.1f);
        
        SendFrame(_frame, _ack);

        yield return null;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (GameRolling && _timer <= 0 && _validDataRecived)
        {
            _timer = _timeBetweenMessagesSent;
            SendFrame(_frame, _ack);
        }
        
        if (Input.GetButtonDown("Go"))
        {
            GameRolling = true;
        }
    }

    private void OnDisable()
    {
        udp.OnRecivedData -= ListenDataRecived;
    }

    private void SendFrame(int frame, byte ack)
    {
        print("frame " + frame);
        
        _validDataRecived = false;

        byte[] packet = new byte[] {(byte)(frame << 1), ack};
        
        udp.SendData(packet);
    }

    private void ListenDataRecived(byte[] data)
    {
        byte keyword = (byte)(data[0] ^ (_frame << 1) );

        Debug.Log("Keyword : " + keyword);
        
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

        HandleData(decryptedData);
        
        _frame++;
        _ack = decryptedData[1];
        _validDataRecived = true;
        // SendFrame(_frame, _ack);
    }

    private void HandleData(byte[] data)
    {
        bool _hasPlayer = false;
        for (int i = 0; i < data[2]; i++)
        {
            if (data[((i * 3) + 3)] == 0)
            {
                _hasPlayer = true;
            }
        }

        if (!_hasPlayer)
        {
            GameRolling = false;

            Debug.LogError("You Lost");
        }

        OnDataHandle?.Invoke(data);

        //Debug.LogWarning("object count : " + data[2]);

        for (int i = 0; i < data[2]; i++)
        {
            // SendDataToController(data[((i * 3) + 3) + 0], data[((i * 3) + 3) + 1], data[((i * 3) + 3)+ 2]);
            // var aux = "";
            // for (int j = 0; j < 3; j++)
            // {
            //     aux += data[((i + 3) * 3) + j] + "  ";
            // }
            //
            // Debug.LogWarning(aux);
            
            //MoveObject(data[((i + 3) * 3) + 0], data[((i + 3) * 3) + 1], data[((i + 3) * 3)+ 2]);
        }
    }

    private void SendDataToController(byte id, byte horizontal, byte vertical)
    {
        Debug.LogWarning(id + " : " + horizontal + " , " + vertical);
        
        _objectManager.MoveObject(id, horizontal, vertical);
    }
}

public class UDP
{
    public UdpClient socket;
    public IPEndPoint endPoint;

    public Action<byte[]> OnRecivedData;
    
    public UDP()
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
