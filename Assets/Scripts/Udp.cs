using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

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