using UnityEngine;

public enum InputTypes
{
    None = 0,
    Left = 1,
    Right = 2,
    Shoot = 3,
}

public class InputManager : MonoBehaviour
{
    public static InputTypes CurrentInput { get; private set; } = InputTypes.None;
    
    public static byte GetCurrentInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButton("Shoot"))
        {
            CurrentInput = InputTypes.Shoot;
        }
        else if (horizontal > 0.2f)
        {
            CurrentInput = InputTypes.Right;
        }
        else if (horizontal < -0.2f)
        {
            CurrentInput = InputTypes.Left;
        }
        else
        {
            CurrentInput = InputTypes.None;
        }

        return (byte) (int) CurrentInput;
    }
}