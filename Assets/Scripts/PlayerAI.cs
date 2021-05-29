using UnityEngine;

public class PlayerAI : MonoBehaviour
{
    public static byte CurrentInput = 0x00;

    private static byte[] _data = null;

    private static byte _lastInput = 0x00;
    
    private void OnEnable()
    {
        CurrentInput = 0x03;
        
        Client.OnDataHandle += OnDataHandle;
    }

    private void OnDisable()
    {
        Client.OnDataHandle -= OnDataHandle;
    }

    public static byte GetCurrentInput()
    {
        CurrentInput = (byte)(CurrentInput ^ 0x03);
        
        var playerPosition = new Vector2(0, 0);
        for (int i = 0; i < _data[2]; i++)
        {
            if (_data[((i * 3) + 3) + 0] == 0)
            {
                playerPosition = new Vector2(_data[((i * 3) + 3) + 1], _data[((i * 3) + 3) + 2]);
            }
        }
        
        int ocupiedPositions = 0;
        
        for (int i = 0; i < _data[2]; i++)
        {
            if (_data[((i * 3) + 3) + 0] != 0 && _data[((i * 3) + 3) + 0] != 1)
            {
                var enemyPosition =  new Vector2(_data[((i * 3) + 3) + 1], _data[((i * 3) + 3) + 2]);
        
                if (enemyPosition == playerPosition + Vector2.up)
                {
                    ocupiedPositions |= 1 << 1;
                }
                else if (enemyPosition == playerPosition + Vector2.up + Vector2.right)
                {
                    ocupiedPositions |= 1 << 0;
                }
                else if (enemyPosition == playerPosition + Vector2.up + Vector2.left)
                {
                    ocupiedPositions |= 1 << 2;
                }
            }
        }
        
        Debug.Log($"Ocupied positions {ocupiedPositions} -> {CurrentInput}");

        var freePositions = ocupiedPositions ^ 0x07;
        
        if (freePositions == 1)
        {
            CurrentInput = 2;
        }
        else if (freePositions == 4)
        {
            CurrentInput = 1;
        }
        else if (freePositions == 5)
        {
            CurrentInput = (byte)(Random.Range(1, 3));
        }
        else
        {
            CurrentInput = _lastInput != 3 ? (byte) 3 : (byte) 0;
        }
        
        _lastInput = CurrentInput;
        return CurrentInput;
    }
    

    private void OnDataHandle(byte[] data)
    {
        _data = data;
        // CurrentInput = (byte)(CurrentInput ^ 0x03);

        // var playerPosition = new Vector2(0, 0);
        // for (int i = 0; i < data[2]; i++)
        // {
        //     if (data[((i * 3) + 3) + 0] == 0)
        //     {
        //         playerPosition = new Vector2(data[((i * 3) + 3) + 1], data[((i * 3) + 3) + 2]);
        //     }
        // }
        //
        // int ocupiedPositions = 0;
        //
        // for (int i = 0; i < data[2]; i++)
        // {
        //     if (data[((i * 3) + 3) + 0] != 0 && data[((i * 3) + 3) + 0] != 1)
        //     {
        //         var enemyPosition =  new Vector2(data[((i * 3) + 3) + 1], data[((i * 3) + 3) + 2]);
        //
        //         if (enemyPosition == playerPosition + Vector2.up)
        //         {
        //             ocupiedPositions |= 1 << 1;
        //         }
        //         else if (enemyPosition == playerPosition + Vector2.up + Vector2.right)
        //         {
        //             ocupiedPositions |= 1 << 0;
        //         }
        //         else if (enemyPosition == playerPosition + Vector2.up + Vector2.left)
        //         {
        //             ocupiedPositions |= 1 << 2;
        //         }
        //     }
        // }
        //
        // Debug.Log($"Ocupied positions {ocupiedPositions} -> {CurrentInput}");
    }
}