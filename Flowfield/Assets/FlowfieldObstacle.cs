using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowfieldObstacle : MonoBehaviour
{

    new public Collider collider
    {
        get
        {
            if (_collider == null)
                _collider = GetComponent<Collider>();
            return _collider;
        }
    }
    Collider _collider;

    private void OnDrawGizmos()
    {
        if (FlowfieldController.instance.costField == null)
            return;
        Vector3Int pos = FlowfieldController.instance.WorldToGrid(transform.position);
        if (!FlowfieldController.instance.IsInBounds(pos))
            return;
        if (FlowfieldController.instance.costField[pos.x][pos.z] > 0)
        {
            Gizmos.color = new Color((float)FlowfieldController.instance.costField[pos.x][pos.z] / 255.0f, 0, 0);
            Gizmos.DrawSphere(transform.position, .3f);
        }

    }
}
