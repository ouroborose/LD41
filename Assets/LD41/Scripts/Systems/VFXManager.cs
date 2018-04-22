using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : Singleton<VFXManager> {
    public ParticleSystem m_hitVFX;
    public ParticleSystem m_breakPuffVFX;
    public int m_minBreakPuffs = 30;
    public int m_maxBreakPuffs = 40;

    public ParticleSystem m_placePuffVFX;
    public int m_minPlacePuffs = 5;
    public int m_maxPlacePuffs = 10;

    public void DoHitVFX(Vector3 pos, Vector3 normal, int count)
    {
        m_hitVFX.transform.position = pos;
        m_hitVFX.transform.forward = normal;
        m_hitVFX.Emit(count);
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
}
