using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraBehaviour : MonoBehaviour
{
    [SerializeField]
    private Transform m_target = null;

    [SerializeField]
    private float m_damping = 1f;

    private Vector3 m_offset;

    void Start()
    {
        m_offset = m_target.position - transform.position;
    }

    void LateUpdate()
    {
        float desiredAngle = m_target.eulerAngles.y;
        float currentAngle = transform.eulerAngles.y;
        float angle = Mathf.LerpAngle(currentAngle, desiredAngle, Time.deltaTime * m_damping);
        Quaternion rotation = Quaternion.Euler(0, angle, 0);
        transform.position = m_target.position - (rotation * m_offset);
        
        transform.LookAt(m_target);
    }
}
