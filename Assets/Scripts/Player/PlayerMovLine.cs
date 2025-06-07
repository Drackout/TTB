using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PlayerMovLine : MonoBehaviour
{
    private LineRenderer LR;
    public LayerMask groundLayer;

    void Start()
    {
        LR = GetComponent<LineRenderer>();
        LR.positionCount = 2;
    }

    void Update()
    {
        LR.SetPosition(0, transform.position);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 direction = hit.point - transform.position;

            if (direction.magnitude > 5f)
                direction = direction.normalized * 5f;

            LR.SetPosition(1, transform.position + direction);
        }
        else
        {
            LR.SetPosition(1, transform.position);
        }
    }
}
