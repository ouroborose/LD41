using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseActor : BaseObject {

    [SerializeField] protected int m_maxHp = 10;
    [SerializeField] protected float m_moveSpeed = 1.0f;
    [SerializeField] protected float m_jumpVelocity = 10.0f;

    [SerializeField] protected float m_actionDelayTime = 0.1f;
    [SerializeField] protected float m_comboFinishDelayTime = 0.5f;
    [SerializeField] protected float m_comboTimeThreshold = 0.25f;

    [SerializeField] protected Transform m_holdPoint;

    public enum AnimationID : int
    {
        IDLE = 0,
        WALK,
        RUN,
        ATTACK_1,
        ATTACK_2,
        ATTACK_3,
        HOLD_IDLE,
        HOLD_WALK,
        JUMP,
    }

    [System.Serializable]
    public class AnimationData
    {
        public AnimationID m_id;
        public bool m_loops = true;
        public bool m_blocking = false;
        public float m_secondsPerFrame = 0.1f;
        public List<GameObject> m_frames;
    }

    [SerializeField] protected List<AnimationData> m_animations = new List<AnimationData>();

    protected Dictionary<AnimationID, AnimationData> m_animationMapping = new Dictionary<AnimationID, AnimationData>();
    protected AnimationData m_currentAnimation;

    protected GameObject m_currentFrame;
    protected float m_animationTimer = 0.0f;
    protected int m_currentFrameIndex = 0;

    protected Vector3 m_spawnPos;
    protected int m_currentHp = 0;

    protected float m_actionDelayTimer = 0.0f;
    protected int m_attackComboCount = 0;
    protected float m_timeSinceLastAttack = 0.0f;

    protected Vector3 m_moveDir;
    protected bool m_isOnGround = false;
    protected bool m_jumpRequested = false;
    protected bool m_actionRequested = false;

    protected BaseBuildingPart m_heldPart = null;

    protected override void Awake()
    {
        base.Awake();
        
        for(int i = 0; i < m_animations.Count; ++i)
        {
            m_animationMapping.Add(m_animations[i].m_id, m_animations[i]);
        }

        PlayAnimation(AnimationID.IDLE);
    }

    public virtual void Reset()
    {
        m_currentHp = m_maxHp;
    }

    public override void ControlledUpdate()
    {
        base.ControlledUpdate();

        UpdateAction();
        UpdateMovement();
        UpdateAnimationState();
        UpdateAnimation();
    }

    protected void UpdateAction()
    {
        m_actionDelayTimer -= Time.deltaTime;
        if (m_actionRequested && m_actionDelayTimer <= 0.0f)
        {
            m_actionDelayTimer = m_actionDelayTime;
            Debug.Log("action");
            m_actionRequested = false;

            if(m_heldPart == null)
            {
                // TODO: search for building parts to pickup
                m_timeSinceLastAttack = 0.0f;
                AnimationID attackID = DoAttack(m_attackComboCount);
                if(attackID == AnimationID.ATTACK_3)
                {
                    m_actionDelayTimer = m_comboFinishDelayTime;
                }


                if(m_timeSinceLastAttack < m_comboTimeThreshold && m_attackComboCount < 2)
                {
                    m_attackComboCount++;
                }
                else
                {
                    m_attackComboCount = 0;
                }
            }
        }

        m_timeSinceLastAttack += Time.deltaTime;
        if(m_timeSinceLastAttack >= m_comboTimeThreshold)
        {
            m_attackComboCount = 0;
        }
    }

    protected AnimationID DoAttack(int comboCount)
    {
        AnimationID attackID = (AnimationID)((int)AnimationID.ATTACK_1 + comboCount);
        Debug.Log(attackID);
        PlayAnimation(attackID, true);
        MoveToward(transform.forward);

        // TODO: deal damage
        return attackID;
    }

    protected void UpdateMovement()
    {
        if(m_jumpRequested)
        {
            m_jumpRequested = false;
            if(m_isOnGround)
            {
                m_rigidbody.AddForce(Vector3.up * m_jumpVelocity, ForceMode.Impulse);
            }
        }

        Vector3 velocity = m_rigidbody.velocity;
        if(velocity.y < 0.0f)
        {
            velocity.y *= 1.1f;
            m_rigidbody.velocity = velocity;
        }
        
        
        Vector3 pos = m_rigidbody.position;
        Vector3 toPos = transform.position + m_moveDir * (m_moveSpeed * Time.deltaTime);
        toPos.y = pos.y;

        if (m_moveDir != Vector3.zero)
        {
            m_rigidbody.MoveRotation(Quaternion.LookRotation(m_moveDir));
        }

        m_rigidbody.MovePosition(toPos);
        Ray ray = new Ray(transform.position + new Vector3(0, 1f, 0), Vector3.down);
        m_isOnGround = Physics.SphereCast(ray, 0.25f, 0.8f, Utils.ENVIRONMENT_LAYER_MASK);
    }

    protected void UpdateAnimationState()
    {
        if(m_isOnGround)
        {
            if(m_moveDir != Vector3.zero)
            {
                PlayAnimation((m_heldPart == null)?AnimationID.WALK:AnimationID.HOLD_WALK);
            }
            else
            {
                PlayAnimation((m_heldPart == null) ? AnimationID.IDLE:AnimationID.HOLD_IDLE);
            }
        }
        else
        {
            PlayAnimation(AnimationID.JUMP);
        }
    }

    protected void UpdateAnimation()
    {
        if (m_currentAnimation != null && (m_currentAnimation.m_frames.Count > 1 || !m_currentAnimation.m_loops || m_currentAnimation.m_blocking))
        {
            m_animationTimer += Time.deltaTime;
            if (m_animationTimer > m_currentAnimation.m_secondsPerFrame)
            {
                m_animationTimer -= m_currentAnimation.m_secondsPerFrame;
                m_currentFrameIndex++;
                if(m_currentFrameIndex >= m_currentAnimation.m_frames.Count)
                {
                    if(m_currentAnimation.m_loops)
                    {
                        m_currentFrameIndex = 0; // loop
                    }
                    else
                    {
                        PlayAnimation(AnimationID.IDLE);
                        return;
                    }
                }
                SetFrame(m_currentAnimation.m_frames[m_currentFrameIndex]);
            }
        }
    }

    public void PlayAnimation(AnimationID id, bool force = false)
    {
        if(!force && m_currentAnimation != null && (m_currentAnimation.m_id == id || m_currentAnimation.m_blocking && m_currentFrameIndex < m_currentAnimation.m_frames.Count))
        {
            return;
        }

        AnimationData data;
        if(m_animationMapping.TryGetValue(id, out data))
        {
            PlayAnimation(data);
        }
    }

    public void PlayAnimation(AnimationData data)
    {
        m_currentAnimation = data;
        m_animationTimer = 0;
        m_currentFrameIndex = 0;
        SetFrame(m_currentAnimation.m_frames[m_currentFrameIndex]);
    }

    public void SetFrame(GameObject frame)
    {
        if (m_currentFrame != null)
        {
            m_currentFrame.SetActive(false);
        }

        m_currentFrame = frame;
        m_currentFrame.SetActive(true);
    }

    public void TeleportTo(Vector3 pos)
    {
        transform.position = pos;
    }

    public void MoveToward(Vector3 dir)
    {
        m_moveDir = dir;
    }

    public void Jump()
    {
        m_jumpRequested = true;
    }

    public void DoAction()
    {
        m_actionRequested = true;
    }

    public void SetSpawnPos(Vector3 spawnPos, bool respawn = true)
    {
        m_spawnPos = spawnPos;
        if (respawn)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        TeleportTo(m_spawnPos);
        Reset();
    }

    public void TakeDamage(int damage, Vector3 knockBack)
    {

    }
}
