using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float movSpeed;
    void Start()
    {
        movSpeed = 2f;
    }


    void Update()
    {
        Move();
    }


    private void Move()
    {
        Vector3 movDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 newDir = movDir * movSpeed;
        transform.position += newDir * Time.deltaTime;
    }
}
