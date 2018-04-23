using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BaseBuildingPart : BaseObject {
    public const float IMPACT_PUNCH_DIST = 0.1f;
    public const float MIN_BREAK_FORCE = 0.5f;
    public const float MAX_BREAK_FORCE = 0.75f;

    public static readonly Color BROKEN_COLOR = new Color(0.5f, 0.5f, 0.5f);

    public enum BuildingPartType
    {
        Story,
        Roof,
    }

    public BuildingPartType m_type = BuildingPartType.Story;
    public BaseTile m_currentTile;

    public int m_maxHp = 3;

    public PlayerData.PlayerId m_owner = PlayerData.PlayerId.UNASSIGNED;
    public bool m_isBeingCarried = false;
    public bool m_isBroken { get { return m_currentHp <= 0; } }

    protected int m_currentHp = 0;
    protected float m_pickupDelayTimer = 0.0f;

    protected BaseActor m_thrower = null;

    public virtual void Reset()
    {
        m_rigidbody.isKinematic = true;
        m_currentHp = m_maxHp;
        ResetColor();
        UpdateDamageVisuals();
    }

    protected void UpdateDamageVisuals()
    {
        SetTexture(LevelGenerator.Instance.GetDamageTexture(m_currentHp, m_maxHp));
    }

    public void TakeDamage(int damage, Vector3 impactDir)
    {
        Vector3 impactForce = impactDir * IMPACT_PUNCH_DIST * damage;
        if (m_isBroken)
        {
            m_rigidbody.AddForce(impactForce + Vector3.up * Random.Range(MIN_BREAK_FORCE, MAX_BREAK_FORCE) * 0.5f, ForceMode.Impulse);
        }
        else
        {
            m_currentHp -= damage;
            if (m_currentHp <= 0)
            {
                Break(impactDir);
                if(m_currentTile != null)
                {
                    m_currentTile.OnPartBreak(this);
                }
            }
            else
            {
                Shake(impactForce);
            }
            UpdateDamageVisuals();
        }
    }

    public void Shake(Vector3 force)
    {
        DOTween.Kill(m_model.transform, true);
        m_model.transform.DOPunchPosition(force, 0.33f);
    }

    public void Break(Vector3 impactDir)
    {
        m_currentHp = 0;

        Vector3 breakDir = Random.onUnitSphere * 0.5f;
        breakDir.y = 2;
        breakDir += impactDir;
        breakDir.Normalize();

        RestoreRigidbody();
        m_rigidbody.isKinematic = false;
        m_rigidbody.AddForce(breakDir * Random.Range(MIN_BREAK_FORCE, MAX_BREAK_FORCE), ForceMode.Impulse);

        SetColor(BROKEN_COLOR);
        UpdateDamageVisuals();
        VFXManager.Instance.DoBreakPuffVFX(transform.position + transform.up * 0.5f);
    }

    public void PickUp(Transform holdPoint)
    {
        m_thrower = null;
        m_isBeingCarried = true;
        transform.parent = holdPoint;
        transform.DOLocalMove(Vector3.zero, 0.25f, true).SetDelay(0.25f);
        transform.DOLocalRotate(Vector3.zero, 0.25f);
        RemoveRigidbody();
    }
    
    public void Place(BaseTile placementTile, PlayerData.PlayerId owner)
    {
        m_isBeingCarried = false;
        transform.parent = null;
        RestoreRigidbody();
        DOTween.Kill(transform);

        placementTile.AddBuildingPart(this);
        placementTile.Claim(owner);

        VFXManager.Instance.DoPlacePuffVFX(transform.position);
    }

    public void Drop()
    {
        m_isBeingCarried = false;
        transform.parent = null;
        DOTween.Kill(transform);
        Break(Vector3.up);
    }

    public void Throw(BaseActor thrower, Vector3 throwForce)
    {
        m_thrower = thrower;
        m_isBeingCarried = false;
        transform.parent = null;
        RestoreRigidbody();
        DOTween.Kill(transform);

        m_rigidbody.AddForce(throwForce, ForceMode.Impulse);
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if(m_thrower == null)
        {
            return;
        }
        
        if(collision.collider == LevelGenerator.Instance.m_groundCollider)
        {
            m_thrower = null;
            return;
        }


        BaseBuildingPart part = collision.collider.GetComponentInParent<BaseBuildingPart>();
        if(part != null)
        {
            part.TakeDamage(3, -collision.contacts[0].normal);
        }
        else
        {
            BaseActor actor = collision.collider.GetComponentInParent<BaseActor>();
            if(actor != null && m_thrower != actor)
            {
                actor.TakeDamage(2, -collision.contacts[0].normal);
            }
        }

    }
}
