using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PerceptionOutput
{
    public float Distance;
    public GameObject hitGo;
}

public class RayCaster
{
    private Color m_GizmoColor = Color.red;
    Transform m_RobotTrans;
    float m_OffsetHeight;
    float m_CastingDistance;
    float m_CastSphereSize;

    float m_AngleDeg;
    List<string> m_DetectableTags;

    RaycastHit m_Hit;
    Vector3 m_CastingDir;
    bool m_HitDetect;

    public RayCaster(Transform robotTrans, float castingDistance, float offsetHeight, float angle, float castSphereSize, List<string> detectableTags)
    {
        m_RobotTrans = robotTrans;
        m_CastingDistance = castingDistance;
        m_OffsetHeight = offsetHeight;
        m_AngleDeg = angle;
        m_CastSphereSize = castSphereSize;
        m_DetectableTags = detectableTags;
    }

    public void UpdateCastingDistance(float castingDistance)
    {
        m_CastingDistance = castingDistance;
    }

    public float[] GetObservations()
    {
        float[] observations = new float[m_DetectableTags.Count + 2];
        PerceptionOutput output = Cast();
        
        if (output.hitGo != null)
        {
            // Find the index of the tag of the object that was hit.
            for (var i = 0; i < m_DetectableTags.Count; i++)
            {
                if (output.hitGo.CompareTag(m_DetectableTags[i]))
                {
                    observations[i] = 1.0f;
                    observations[observations.Length - 1] = output.Distance / m_CastingDistance;
                    break;
                }
            }
        }
        else
        {
            observations[observations.Length - 2] = 1.0f;
        }

        // Debug.LogFormat(
        //     "m_StartAngleDeg: {0:0.0}, observations: {1}",
        //     m_StartAngleDeg,
        //     string.Join(":", observations));
        return observations;
    }
 
    public PerceptionOutput Cast()
    {
        m_CastingDir = Quaternion.Euler(0, m_AngleDeg, 0) * m_RobotTrans.forward;

        m_HitDetect = Physics.SphereCast(
            m_RobotTrans.position + new Vector3(0.0f, m_OffsetHeight, 0.0f),
            m_CastSphereSize,
            m_CastingDir,
            out m_Hit,
            m_CastingDistance);

        PerceptionOutput output = new PerceptionOutput();
        if (m_HitDetect)
        {
            float hitDistance = Vector3.Distance(m_RobotTrans.position, m_Hit.point);
            output.Distance = hitDistance;
            output.hitGo = m_Hit.collider.gameObject;
        }
        else
        {
            output.Distance = 0.0f;
            output.hitGo = null;
        }

        return output;
    }

    public void DrawCasterGizmos()
    {
        m_CastingDir = Quaternion.Euler(0, m_AngleDeg, 0) * m_RobotTrans.forward;
        Gizmos.color = m_GizmoColor;

        // Matrix4x4 cubeTransform = Matrix4x4.TRS(
        //     m_RobotTrans.position + m_CastingDir * m_CastingDistance + new Vector3(0.0f, m_OffsetHeight, 0.0f),
        //     m_RobotTrans.rotation * Quaternion.Euler(0, m_AngleDeg, 0),
        //     m_RobotTrans.lossyScale);

        // Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
        // Gizmos.matrix *= cubeTransform;
        Gizmos.DrawWireSphere(m_RobotTrans.position + m_CastingDir * m_CastingDistance + new Vector3(0.0f, m_OffsetHeight, 0.0f), m_CastSphereSize);

        // Gizmos.matrix = oldGizmosMatrix;
        // float sideLength = m_MaxDistance;
        Vector3 startVec = Quaternion.Euler(0, m_AngleDeg, 0) * m_RobotTrans.forward * m_CastingDistance;
        Gizmos.DrawRay(m_RobotTrans.position + new Vector3(0.0f, m_OffsetHeight, 0.0f), startVec);
        // Vector3 endVec = Quaternion.Euler(0, m_EndAngleDeg, 0) * m_RobotTrans.forward * sideLength;
        // Gizmos.DrawRay(m_RobotTrans.position + new Vector3(0.0f, m_OffsetHeight, 0.0f), endVec);
    }
}