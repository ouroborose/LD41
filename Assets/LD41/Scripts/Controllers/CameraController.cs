using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class CameraController : Singleton<CameraController> {
    public List<Transform> m_targets;

    public float m_movementTime = 1.0f;
    public float m_minDistance = 5.0f;

    public float m_xViewDistScaler = 1.0f;
    public float m_yViewDistScaler = 1.5f;


    public PostProcessingProfile m_postProcessingProfile;

    protected void LateUpdate()
    {
        int targetCount = m_targets.Count;
        if (targetCount <= 0)
        {
            return;
        }

        Vector3 center = Vector3.zero;
        Bounds bounds = new Bounds();
        for (int i = 0; i < targetCount; ++i)
        {
            center += m_targets[i].position;
            Vector3 localPos = transform.InverseTransformPoint(m_targets[i].position);
            bounds.Encapsulate(localPos);
            /*
            if(localPos.x < minLocalX)
            {
                minLocalX = localPos.x;
            }

            if(localPos.x > maxLocalX)
            {
                maxLocalX = localPos.x;
            }
            */
        }
        center /= targetCount;

        float xViewDist = (bounds.max.x - bounds.min.x) * m_xViewDistScaler;
        float yViewDist = (bounds.max.y - bounds.min.y) * m_yViewDistScaler;
        float dist = Mathf.Max(m_minDistance, xViewDist, yViewDist);
        Vector3 goalPos = center - transform.forward * dist;
        var depthOfFieldSettings = m_postProcessingProfile.depthOfField.settings;
        depthOfFieldSettings.focusDistance = dist + 0.5f;
        m_postProcessingProfile.depthOfField.settings = depthOfFieldSettings;
        transform.position = Vector3.Lerp(transform.position, goalPos, Time.deltaTime / m_movementTime);
    }
}
