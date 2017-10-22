using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Pointer : MonoBehaviour
{
    [SerializeField]
    private Vector3 m_worldSpaceTarget = Vector3.zero;

    [SerializeField]
    private Image m_pointerImage = null;

    private const float MINIMAP_THRESHOLD_SQR = 18500f;

    private void Update()
    {
        Vector3 direction = m_worldSpaceTarget - transform.parent.position;
        direction = new Vector3(direction.x, 0, direction.z);
        if (direction.sqrMagnitude < MINIMAP_THRESHOLD_SQR)
        {
            m_pointerImage.enabled = false;
            return;
        }
        else m_pointerImage.enabled = true;


        Vector3 normalizedDirection = direction.normalized;
        transform.localPosition = new Vector3(normalizedDirection.x, normalizedDirection.z) * 112;
        float angle = Mathf.Atan2(normalizedDirection.z, normalizedDirection.x) * Mathf.Rad2Deg - 90f;
        transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
