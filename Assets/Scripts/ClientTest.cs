using System;
using UnityEngine;
using UnityEngine.iOS;

public class ClientTest : MonoBehaviour
{
    private Udp udp = new Udp();

    private int _frame = 0x00;
    private byte _ack = 0x00;
    private byte _input = 0x00;

    private byte _lastAck = 0x00;

    public static bool GameRolling { get; private set; } = false;
    private float _timer = 0;
    [SerializeField] private float _timeBetweenMessagesSent = 0.5f;

    private bool _validDataRecived = true;

    private bool _lostGame = false;

    private double _elapsedTime = 0;
    

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

        _elapsedTime = Time.timeSinceLevelLoadAsDouble;
        
        SendFrame(_frame, _ack, _input);
    }

    private void SendFrame(int frame, byte ack, byte input)
    {
        byte[] packet = {(byte)((_frame << 1) ), (byte)((_ack))};

        udp.SendData(packet);
    }

    private void ListenDataRecived(byte[] data)
    {
        // if (data.Length < 3)
        // {
        //     Debug.LogError("Insuficient Data!");
        //     // ResendFrame();
        //     return;
        // }
        
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

        // if ((data.Length / 3) - 1 != decryptedData[2])
        // {
        //     Debug.LogError($"number of objects do not match {(data.Length / 3) - 1 } {decryptedData[2]}");
        //     //ResendFrame();
        //     return;
        // }
//        HandleData(decryptedData);
        
        byte newAck = (byte)(decryptedData[1]);
        
        //Debug.LogWarning(decryptedData[1] + "   " + _lastAck + "    " + newAck + "    " + (newAck != _lastAck));
        
        if (newAck != _lastAck)
        {
            //Debug.LogWarning(decryptedData[1] + "   " + _lastAck + "    " + newAck + "    " + (newAck != _lastAck));
            Debug.LogWarning($" ACK changed {_lastAck} {newAck}");
            Debug.LogWarning($"Elapsed time: {_elapsedTime}");
        }
        
        _lastAck = newAck;
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
    }
}