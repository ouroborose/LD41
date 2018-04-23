using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BaseActor : BaseObject {
    public static readonly Color PLACEMENT_ALLOWED_COLOR = new Color(0, 1, 0, 0.5f);
    public static readonly Color PLACEMENT_NOT_ALLOWED_COLOR = new Color(1, 0, 0, 0.5f);

    public const float MAX_VELOCITY = 10.0f;
    public const float KNOCK_BACK_FORCE = 10.0f;
    public const float SPECIAL_CHARGE_TIME = 3.0f;

    public static RaycastHit[] s_sharedHits = new RaycastHit[50];
    public static Ray s_sharedRay = new Ray();
    public static int s_sharedHitsCount = 0;

    public PlayerData.PlayerId m_playerId = PlayerData.PlayerId.UNASSIGNED;


    [Header("Movement")]
    [SerializeField] protected int m_maxHp = 10;
    [SerializeField] protected float m_moveSpeed = 1.0f;
    [SerializeField] protected float m_jumpVelocity = 10.0f;

    [Header("Actions")]
    [SerializeField] protected float m_hitRange = 1.0f;
    [SerializeField] protected float m_actionDelayTime = 0.1f;
    [SerializeField] protected float m_comboTimeThreshold = 0.25f;
    [SerializeField] protected float m_specialAttackActionDelay = 1.0f;
    [SerializeField] protected float m_throwForce = 10.0f;

    [SerializeField] protected Transform m_holdPoint;
    [SerializeField] protected BaseObject m_placementIndicator;


    [Header("VFX")]
    [SerializeField] protected ParticleSystem m_movementDust;
    protected ParticleSystem.EmissionModule m_movementDustEmission;
    
    [SerializeField] protected ParticleSystem m_holdActivatedParticles;
    protected ParticleSystem.EmissionModule m_holdActivatedParticlesEmission;

    [SerializeField] protected ParticleSystem m_specialAttackReadyParticles;
    protected ParticleSystem.EmissionModule m_specialAttackReadyPartieleEmission;

    [SerializeField] protected ParticleSystem m_specialAttackParticles;

    [Header("Audio")]
    [SerializeField] protected AudioClip m_hurtClip;
    [SerializeField] protected AudioClip m_attackClip;
    [SerializeField] protected AudioClip m_specialAttackClip;
    [SerializeField] protected AudioClip m_pickUpClip;
    [SerializeField] protected AudioClip m_putDownClip;
    [SerializeField] protected AudioClip m_throwClip;
    [SerializeField] protected AudioClip m_jumpClip;
    [SerializeField] protected AudioClip m_landClip;

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
    public bool m_run = false;
    public bool m_isOnGround { get; protected set; }
    protected bool m_jumpRequested = false;
    protected bool m_actionRequested = false;

    public bool m_specialAttackActive { get; protected set; }


    protected BaseBuildingPart m_heldPart = null;

    protected BaseBuildingPart m_pickUpCandidate = null;
    protected BaseTile m_placementTile = null;
    protected bool m_placementAllowed = false;
    protected List<KeyValuePair<RaycastHit, BaseBuildingPart>> m_buildingPartsInRange = new List<KeyValuePair<RaycastHit, BaseBuildingPart>>();
    protected List<KeyValuePair<RaycastHit, BaseActor>> m_actorsInRange = new List<KeyValuePair<RaycastHit, BaseActor>>();

    protected bool m_holdActionActivated = false;
    protected float m_specialAttackChargeTimer = 0.0f;

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

        m_holdActivatedParticlesEmission = m_holdActivatedParticles.emission;
        m_specialAttackReadyPartieleEmission = m_specialAttackReadyParticles.emission;
        m_movementDustEmission = m_movementDust.emission;

        m_specialAttackActive = false;

        PlayAnimation(AnimationID.IDLE);
    }
    
    public virtual void Reset()
    {
        m_currentHp = m_maxHp;
    }

    public override void ControlledUpdate()
    {
        base.ControlledUpdate();

        UpdateDetection(0.35f, m_hitRange);
        UpdateAction();
        UpdateMovement();
        UpdateAnimationState();
        UpdateAnimation();
    }

    protected void FixedUpdate()
    {
        m_rigidbody.velocity = Vector3.ClampMagnitude(m_rigidbody.velocity, MAX_VELOCITY);
    }

    protected override void OnKillPlaneHit()
    {
        //base.OnKillPlaneHit();
        Respawn(false);
    }

    protected void UpdateDetection(float radius , float range)
    {
        m_pickUpCandidate = null;
        m_placementTile = null;
        m_placementAllowed = false;

        m_buildingPartsInRange.Clear();
        m_actorsInRange.Clear();

        s_sharedRay = new Ray(transform.position + Vector3.up * 0.7f, transform.forward);
        s_sharedHitsCount = Physics.SphereCastNonAlloc(s_sharedRay, radius, s_sharedHits, range);

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
        
        if(m_holdActionActivated && m_heldPart == null)
        {
            m_specialAttackChargeTimer += Time.deltaTime;
            if(m_specialAttackChargeTimer >= SPECIAL_CHARGE_TIME)
            {
                if(!m_specialAttackReadyPartieleEmission.enabled)
                {
                    m_specialAttackReadyParticles.Emit(50);
                }
                m_specialAttackReadyPartieleEmission.enabled = true;
            }
        }

        m_actionDelayTimer -= Time.deltaTime;
        if (m_actionRequested && m_actionDelayTimer <= 0.0f)
        {
            m_actionDelayTimer = m_actionDelayTime;
            m_actionRequested = false;

            if (m_holdActionActivated)
            {
                DoHoldAction();
            }
            else
            {
                if (m_heldPart == null)
                {
                    if (m_pickUpCandidate == null || m_attackComboCount > 0)
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
                    if (m_placementAllowed)
                    {
                        PlacePart();
                    }
                    else
                    {
                        HandlePlacementErrorFeedback();
                    }
                }
            }
           
        }

        m_timeSinceLastAttack += Time.deltaTime;
        if(m_timeSinceLastAttack >= m_comboTimeThreshold)
        {
            m_attackComboCount = 0;
        }
    }
    
    public void CancelAction()
    {
        m_holdActionActivated = false;
        m_holdActivatedParticlesEmission.enabled = false;
        m_specialAttackReadyPartieleEmission.enabled = false;
        m_actionRequested = false;
        m_jumpRequested = false;
        m_run = false;
        m_moveDir = Vector3.zero;
    }

    protected void DoHoldAction()
    {
        m_holdActionActivated = false;
        if (m_heldPart != null)
        {
            // throw part
            ThrowPart();
        }
        else if (m_specialAttackChargeTimer > SPECIAL_CHARGE_TIME)
        {
            // do special attack
            m_actionDelayTimer = m_specialAttackActionDelay;
            StartCoroutine(HandleSpecialAttack());
        }
        else
        {
            HandleAttacking();
        }

        CancelAction();
    }

    protected IEnumerator HandleSpecialAttack()
    {
        m_specialAttackActive = true;       

        PlayAnimation(AnimationID.ATTACK_3);
        m_specialAttackReadyParticles.Emit(100);
        yield return new WaitForSeconds(0.25f);
        if(m_specialAttackParticles != null)
        {
            m_specialAttackParticles.Play();
        }
        float timer = 0;
        float atkTimer = 0.0f;
        while (timer < 3.0f)
        {
            atkTimer -= Time.deltaTime;
            if(atkTimer <= 0.0f)
            {
                atkTimer += 1.0f;
                m_specialAttackReadyParticles.Emit(50);
                UpdateDetection(0.75f, 3.0f);
                HandleDealingDamage(4, Vector3.up);
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        m_specialAttackActive = false;
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
        AudioManager.Instance.PlayOneShot(m_pickUpClip);
    }

    protected void PlacePart()
    {
        if(m_heldPart == null || m_placementTile == null)
        {
            return;
        }

        AudioManager.Instance.PlayOneShot(m_putDownClip);
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

        m_heldPart.Drop();
        m_heldPart = null;
    }

    protected void ThrowPart()
    {
        if(m_heldPart == null)
        {
            return;
        }

        AudioManager.Instance.PlayOneShot(m_throwClip);
        m_heldPart.Throw(this, transform.forward * m_throwForce);
        m_heldPart = null;
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

        HandleDealingDamage(damage);
        return attackID;
    }

    protected void HandleDealingDamage(int damage, Vector3 additionalImpact = default(Vector3))
    {
        for (int i = 0; i < m_buildingPartsInRange.Count; ++i)
        {
            KeyValuePair<RaycastHit, BaseBuildingPart> pair = m_buildingPartsInRange[i];

            Vector3 impactDir = pair.Key.point - s_sharedRay.origin;
            impactDir.Normalize();
            pair.Value.TakeDamage(damage, impactDir+ additionalImpact);
            VFXManager.Instance.DoHitVFX(pair.Key.point, pair.Key.normal);
        }

        for (int i = 0; i < m_actorsInRange.Count; ++i)
        {
            KeyValuePair<RaycastHit, BaseActor> pair = m_actorsInRange[i];
            Vector3 impactDir = pair.Key.point - s_sharedRay.origin;
            impactDir.Normalize();
            pair.Value.TakeDamage(damage, impactDir+ additionalImpact);
            VFXManager.Instance.DoHitVFX(pair.Key.point, pair.Key.normal);
        }

        if (m_buildingPartsInRange.Count > 0 || m_actorsInRange.Count > 0) 
        {
            AudioManager.Instance.PlayOneShot(m_attackClip);
        }
        else
        {
            AudioManager.Instance.PlayOneShot(m_attackClip, 0.25f);
        }

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
                AudioManager.Instance.PlayOneShot(m_jumpClip);
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
        float speed = m_moveSpeed;
        if(m_holdActionActivated)
        {
            speed *= 0.5f;
        }
        else if(m_run)
        {
            speed *= 2;
        }
        Vector3 toPos = transform.position + m_moveDir * (speed * Time.deltaTime);
        toPos.y = pos.y;

        if (m_moveDir != Vector3.zero)
        {
            m_rigidbody.MoveRotation(Quaternion.LookRotation(m_moveDir));
        }

        bool wasOnGround = m_isOnGround;
        m_rigidbody.MovePosition(toPos);
        Ray ray = new Ray(transform.position + new Vector3(0, 1f, 0), Vector3.down);
        m_isOnGround = Physics.SphereCast(ray, 0.25f, 0.8f, Utils.ENVIRONMENT_LAYER_MASK);

        if(!wasOnGround && m_isOnGround)
        {
            AudioManager.Instance.PlayOneShot(m_landClip);
        }

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

    public void ActivateHoldAction()
    {
        if(m_holdActionActivated)
        {
            return;
        }

        m_holdActionActivated = true;
        m_holdActivatedParticlesEmission.enabled = true;
        m_specialAttackChargeTimer = 0.0f;
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
        VFXManager.Instance.DoBreakPuffVFX(transform.position);
        TeleportTo(m_spawnPos);
        VFXManager.Instance.DoBreakPuffVFX(transform.position);
        m_rigidbody.velocity = Vector3.zero;
        if(reset)
        {
            Reset();
        }
    }

    public void TakeDamage(int damage, Vector3 impactDir)
    {
        m_rigidbody.AddForce(impactDir * KNOCK_BACK_FORCE * damage, ForceMode.Impulse);
        DropPart();
        CancelAction();

        AudioManager.Instance.PlayOneShot(m_hurtClip);
    }
}
