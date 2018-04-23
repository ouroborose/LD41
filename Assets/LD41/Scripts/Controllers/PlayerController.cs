using UnityEngine;

public class PlayerController : MonoBehaviour {
    public const float HOLD_ACTION_THRESHOLD = 1.0f;

    public KeyCode m_upKey = KeyCode.UpArrow;
    public KeyCode m_downKey = KeyCode.DownArrow;
    public KeyCode m_rightKey = KeyCode.RightArrow;
    public KeyCode m_leftKey = KeyCode.LeftArrow;

    public KeyCode m_actionKey = KeyCode.Z;
    public KeyCode m_jumpKey = KeyCode.X;


    public BaseActor m_character;

    protected Transform m_camTrans;
    protected float m_actionHoldTimer = 0.0f;

    protected void Start()
    {
        m_camTrans = Camera.main.transform;
    }

    public void ControlledUpdate()
    {
        if(m_character == null)
        {
            return;
        }

        Vector3 moveDir = Vector3.zero;
        Vector3 forward = Vector3.ProjectOnPlane(m_camTrans.forward, Vector3.up).normalized;
        Vector3 right = m_camTrans.right;
        if (Input.GetKey(m_upKey))
        {
            moveDir += forward;
        }
        if (Input.GetKey(m_downKey))
        {
            moveDir -= forward;
        }
        if (Input.GetKey(m_rightKey))
        {
            moveDir += right;
        }
        if (Input.GetKey(m_leftKey))
        {
            moveDir -= right;
        }

        if(moveDir != Vector3.zero)
        {
            moveDir.Normalize();
        }
        m_character.MoveToward(moveDir);

        if(Input.GetKeyDown(m_jumpKey))
        {
            m_character.Jump();
        }

        if(Input.GetKeyDown(m_actionKey))
        {
            m_actionHoldTimer = 0.0f;
        }
        else if(Input.GetKey(m_actionKey))
        {
            m_actionHoldTimer += Time.deltaTime;
            if(m_actionHoldTimer > HOLD_ACTION_THRESHOLD)
            {
                m_character.ActivateHoldAction();
            }
        }
        if(Input.GetKeyUp(m_actionKey))
        {
            m_character.DoAction();
        }
    }

    
}
