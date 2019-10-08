using UnityEngine;

public abstract class BaseCameraController : MonoBehaviour
{
    public float strafe { get; protected set; }
    public float push { get; protected set; }
    public float lookVertical { get; protected set; }
    public float lookHorizontal { get; protected set; }

    private void Update()
    {
        ApplyInput();
    }

    protected abstract void ApplyInput();

    public void SetStrafe(float amount)
    {
        strafe = Mathf.Clamp(amount, -1, 1);
    }
    public void SetPush(float amount)
    {
        push = Mathf.Clamp(amount, -1, 1);
    }
    public void SetLookVertical(float amount)
    {
        lookVertical = Mathf.Clamp(amount, -1, 1);
    }
    public void SetLookHorizontal(float amount)
    {
        lookHorizontal = Mathf.Clamp(amount, -1, 1);
    }
}
