using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : Singleton<VFXManager> {
    public ParticleSystem m_hitVFX;
    public int m_minHitPuffs = 20;
    public int m_maxHitPuffs = 30;

    public ParticleSystem m_breakPuffVFX;
    public int m_minBreakPuffs = 30;
    public int m_maxBreakPuffs = 40;

    public ParticleSystem m_placePuffVFX;
    public int m_minPlacePuffs = 5;
    public int m_maxPlacePuffs = 10;

    public ObjectPool m_dooberPool;

    public void DoHitVFX(Vector3 pos, Vector3 normal)
    {
        m_hitVFX.transform.position = pos;
        m_hitVFX.transform.forward = normal;
        m_hitVFX.Emit(Random.Range(m_minHitPuffs, m_maxHitPuffs));
    }

    public void DoBreakPuffVFX(Vector3 pos)
    {
        m_breakPuffVFX.transform.position = pos;
        m_breakPuffVFX.Emit(Random.Range(m_minBreakPuffs, m_maxBreakPuffs));
    }

    public void DoPlacePuffVFX(Vector3 pos)
    {
        m_placePuffVFX.transform.position = pos;
        m_placePuffVFX.Emit(Random.Range(m_minPlacePuffs, m_maxPlacePuffs));
    }

    public void ThrowDoober(Vector3 pos, string text)
    {
        ThrowDoober(pos, text, Color.white);
    }

    public void ThrowDoober(Vector3 pos, string text, Color color)
    {
        Doober doober = m_dooberPool.GetAvailable<Doober>();
        doober.Throw(pos, text, color);
    }

    public void ThrowScoreDoober(Vector3 pos, PlayerData.PlayerId target, int scoreDelta, bool roofed)
    {
        if(target == PlayerData.PlayerId.UNASSIGNED || scoreDelta <= 0)
        {
            return;
        }

        pos.y += 0.5f;
        ThrowDoober(pos, string.Format("{0}+{1}", (roofed)?"Roofed!\n":string.Empty, scoreDelta), Color.white);
    }
}
