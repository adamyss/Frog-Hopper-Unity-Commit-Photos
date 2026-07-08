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
        // setting up thresholds for how far up down obstacle moves
        min = transform.position.y - moveAmount;
        max = transform.position.y + moveAmount;
    }

    // Update is called once per frame
    void Update()
    {
        // moves obstacle up or down based of up boolean
        if (up)
        {
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        }
        else
        {
            transform.position -= Vector3.up * moveSpeed * Time.deltaTime;
        }
        // sets up boolean if passing thresholds
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
