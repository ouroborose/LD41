﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class CameraController : Singleton<CameraController> {
    public List<Transform> m_targets;

    public float m_movementTime = 1.0f;
    public float m_minDistance = 5.0f;

    public float m_xViewDistScaler = 1.0f;
    public float m_yViewDistScaler = 1.5f;

    public float m_viewDist = 1.0f;

    protected Bounds m_bounds = new Bounds();
    public PostProcessingProfile m_postProcessingProfile;
    protected DepthOfFieldModel.Settings m_depthOfFieldSettings;


    protected void Start()
    {
        m_depthOfFieldSettings = m_postProcessingProfile.depthOfField.settings;
    }

    protected void LateUpdate()
    {
        int targetCount = m_targets.Count;
        if (targetCount <= 0)
        {
            return;
        }

        Vector3 center = Vector3.zero;
        m_bounds.extents = Vector3.zero;
        for (int i = 0; i < targetCount; ++i)
        {
            center += m_targets[i].position;
            Vector3 localPos = transform.InverseTransformPoint(m_targets[i].position);
            m_bounds.Encapsulate(localPos);
        }
        center /= targetCount;

        Vector3 min = m_bounds.min;
        Vector3 max = m_bounds.max;
        
        float xViewDist = (max.x - min.x) * m_xViewDistScaler;
        float yViewDist = (max.y - min.y) * m_yViewDistScaler;
        m_viewDist = Mathf.Max(m_minDistance, Mathf.Max(xViewDist, yViewDist));
        Vector3 goalPos = center - transform.forward * m_viewDist;
        m_depthOfFieldSettings.focusDistance = m_viewDist + 0.5f;
        m_postProcessingProfile.depthOfField.settings = m_depthOfFieldSettings;
        transform.position = Vector3.Lerp(transform.position, goalPos, Time.deltaTime / m_movementTime);
    }
}
