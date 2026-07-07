using UnityEngine;

public class movingobstacle : MonoBehaviour
{
    public float moveSpeed;
    public float moveAmount;
    float min;
    float max;
    bool up;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        min = transform.position.y - moveAmount;
        max = transform.position.y + moveAmount;
    }

    // Update is called once per frame
    void Update()
    {
        if (up)
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        }
        else
        {
            transform.position -= Vector3.up * moveSpeed * Time.deltaTime;
        }
        if(transform.position.y >= max)
        {
            up = false;
        }
        if(transform.position.y <= min)
        {
            up = true;
        }
    }
}
