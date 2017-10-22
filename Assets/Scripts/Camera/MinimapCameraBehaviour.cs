using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraBehaviour : MonoBehaviour
{
    [SerializeField]
    private Transform m_target = null;

    private Vector3 m_offset;

    private void Start()
    {
        m_offset = m_target.transform.position - transform.position;
    }

    private void LateUpdate ()
    {
        transform.position = m_target.position - m_offset;
	}
}
