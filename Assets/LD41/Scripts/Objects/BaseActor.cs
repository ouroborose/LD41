using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BaseActor : BaseObject {
    public static readonly Color PLACEMENT_ALLOWED_COLOR = new Color(0, 1, 0, 0.5f);
    public static readonly Color PLACEMENT_NOT_ALLOWED_COLOR = new Color(1, 0, 0, 0.5f);

    public const float KNOCK_BACK_FORCE = 10.0f;

    public static RaycastHit[] s_sharedHits = new RaycastHit[50];
    public static Ray s_sharedRay = new Ray();
    public static int s_sharedHitsCount = 0;

    public PlayerData.PlayerId m_playerId = PlayerData.PlayerId.UNASSIGNED;


    [Header("Movement")]
    [SerializeField] protected int m_maxHp = 10;
    [SerializeField] protected float m_moveSpeed = 1.0f;
    [SerializeField] protected float m_jumpVelocity = 10.0f;

    [Header("Actions")]
    [SerializeField] protected float m_actionDelayTime = 0.1f;
    [SerializeField] protected float m_comboFinishDelayTime = 0.5f;
    [SerializeField] protected float m_comboTimeThreshold = 0.25f;
    [SerializeField] protected float m_hitRange = 1.0f;

    [SerializeField] protected Transform m_holdPoint;
    [SerializeField] protected BaseObject m_placementIndicator;


    [Header("VFX")]
    [SerializeField]
    protected ParticleSystem m_movementDust;
    protected ParticleSystem.EmissionModule m_movementDustEmission;

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
        PICK_UP,
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

    [Header("Animations")]
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

    protected BaseBuildingPart m_pickUpCandidate = null;
    protected BaseTile m_placementTile = null;
    protected bool m_placementAllowed = false;
    protected List<KeyValuePair<RaycastHit, BaseBuildingPart>> m_buildingPartsInRange = new List<KeyValuePair<RaycastHit, BaseBuildingPart>>();
    protected List<KeyValuePair<RaycastHit, BaseActor>> m_actorsInRange = new List<KeyValuePair<RaycastHit, BaseActor>>();

    protected override void Awake()
    {
        base.Awake();
        
        for(int i = 0; i < m_animations.Count; ++i)
        {
            m_animationMapping.Add(m_animations[i].m_id, m_animations[i]);
        }

        m_placementIndicator.transform.parent = null;
        m_placementIndicator.transform.rotation = Quaternion.identity;
        m_placementIndicator.gameObject.SetActive(false);

        m_movementDustEmission = m_movementDust.emission;

        PlayAnimation(AnimationID.IDLE);
    }
    
    public virtual void Reset()
    {
        m_currentHp = m_maxHp;
    }

    public override void ControlledUpdate()
    {
        base.ControlledUpdate();

        UpdateDetection();
        UpdateAction();
        UpdateMovement();
        UpdateAnimationState();
        UpdateAnimation();
    }

    protected override void OnKillPlaneHit()
    {
        //base.OnKillPlaneHit();
        Respawn(false);
    }

    protected void UpdateDetection()
    {
        m_pickUpCandidate = null;
        m_placementTile = null;
        m_placementAllowed = false;

        m_buildingPartsInRange.Clear();
        m_actorsInRange.Clear();

        s_sharedRay = new Ray(transform.position + Vector3.up * 0.75f, transform.forward);
        s_sharedHitsCount = Physics.SphereCastNonAlloc(s_sharedRay, 0.3f, s_sharedHits, m_hitRange);

        float bestPartValue = float.MaxValue;

        for (int i = 0; i < s_sharedHitsCount; ++i)
        {
            RaycastHit hit = s_sharedHits[i];
            BaseObject obj = hit.collider.GetComponentInParent<BaseObject>();
            if (obj == null || obj == this)
            {
                continue;
            }

            BaseBuildingPart part = obj as BaseBuildingPart;
            if(part != null && !part.m_isBeingCarried)
            {
                m_buildingPartsInRange.Add(new KeyValuePair<RaycastHit, BaseBuildingPart>(hit, part));
                if(m_heldPart == null && part.m_isBroken)
                {
                    if(hit.distance < bestPartValue)
                    {
                        bestPartValue = hit.distance;
                        m_pickUpCandidate = part;
                    }
                }
                continue;
            }

            BaseActor actor = obj as BaseActor;
            if(actor != null)
            {
                m_actorsInRange.Add(new KeyValuePair<RaycastHit, BaseActor>(hit, actor));
            }
        }

        if(m_heldPart != null)
        {
            m_placementTile = LevelGenerator.Instance.GetClosestTile(transform.position + transform.forward);

            if(m_placementTile != null)
            {
                BaseBuildingPart topPart = m_placementTile.GetTopPart();
                if(topPart == null)
                {
                    m_placementAllowed = true;
                    m_placementIndicator.transform.position = m_placementTile.transform.position;
                }
                else
                {
                    m_placementAllowed = topPart.m_type != BaseBuildingPart.BuildingPartType.Roof && topPart.transform.position.y <= transform.position.y + 1.5f;
                    m_placementIndicator.transform.position = topPart.transform.position + Vector3.up;
                }
            }
        }

        m_placementIndicator.gameObject.SetActive(m_placementTile != null);
        if(m_placementIndicator.gameObject.activeSelf)
        {
            m_placementIndicator.SetColor(m_placementAllowed ? PLACEMENT_ALLOWED_COLOR : PLACEMENT_NOT_ALLOWED_COLOR);
        }
    }

    protected void UpdateAction()
    {
        m_actionDelayTimer -= Time.deltaTime;
        if (m_actionRequested && m_actionDelayTimer <= 0.0f)
        {
            m_actionDelayTimer = m_actionDelayTime;
            m_actionRequested = false;

            if(m_heldPart == null)
            {
                if(m_pickUpCandidate == null || m_attackComboCount > 0)
                {
                    HandleAttacking();
                }
                else
                {
                    PickUpPart(m_pickUpCandidate);
                }
            }
            else
            {
                if(m_placementAllowed)
                {
                    PlacePart();
                }
                else
                {
                    HandlePlacementErrorFeedback();
                }
            }
        }

        m_timeSinceLastAttack += Time.deltaTime;
        if(m_timeSinceLastAttack >= m_comboTimeThreshold)
        {
            m_attackComboCount = 0;
        }
    }

    protected void PickUpPart(BaseBuildingPart pickup)
    {
        if (pickup == null)
        {
            return;
        }

        PlayAnimation(AnimationID.PICK_UP, true);
        m_heldPart = pickup;
        m_heldPart.PickUp(m_holdPoint);
        m_actionDelayTimer = 0.5f;
    }

    protected void PlacePart()
    {
        if(m_heldPart == null || m_placementTile == null)
        {
            return;
        }

        m_heldPart.Place(m_placementTile, m_playerId);
        m_heldPart = null;
    }

    protected void HandlePlacementErrorFeedback()
    {
        if(m_heldPart == null)
        {
            return;
        }

        m_heldPart.Shake(Random.onUnitSphere * 0.2f);
    }

    protected void DropPart()
    {
        if(m_heldPart == null)
        {
            return;
        }
    }

    protected void HandleAttacking()
    {
        m_timeSinceLastAttack = 0.0f;
        DoAttack(m_attackComboCount);
        m_attackComboCount++;
    }

    protected AnimationID DoAttack(int comboCount)
    {
        AnimationID attackID = (AnimationID)((int)AnimationID.ATTACK_1 + comboCount%2);
        //Debug.Log(attackID);
        PlayAnimation(attackID, true);
        MoveToward(transform.forward);

        int damage = CalculateDamage(attackID);

        for (int i = 0; i < m_buildingPartsInRange.Count; ++i)
        {
            KeyValuePair<RaycastHit, BaseBuildingPart> pair = m_buildingPartsInRange[i];

            Vector3 impactDir = pair.Key.point - s_sharedRay.origin;
            impactDir.Normalize();
            pair.Value.TakeDamage(damage, impactDir);
            VFXManager.Instance.DoHitVFX(pair.Key.point, pair.Key.normal);
        }

        for(int i = 0; i < m_actorsInRange.Count; ++i)
        {
            KeyValuePair<RaycastHit, BaseActor> pair = m_actorsInRange[i];
            Vector3 impactDir = pair.Key.point - s_sharedRay.origin;
            impactDir.Normalize();
            pair.Value.TakeDamage(damage, impactDir);
            VFXManager.Instance.DoHitVFX(pair.Key.point, pair.Key.normal);
        }
        return attackID;
    }

    protected int CalculateDamage(AnimationID attackID)
    {
        if(attackID == AnimationID.ATTACK_3)
        {
            return 3;
        }
        return 1;
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
        m_movementDustEmission.enabled = m_isOnGround;
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

    public void Respawn(bool reset = true)
    {
        TeleportTo(m_spawnPos);
        m_rigidbody.velocity = Vector3.zero;
        if(reset)
        {
            Reset();
        }
    }

    public void TakeDamage(int damage, Vector3 impactDir)
    {
        m_rigidbody.AddForce(impactDir * KNOCK_BACK_FORCE * damage, ForceMode.Impulse);
    }
}
