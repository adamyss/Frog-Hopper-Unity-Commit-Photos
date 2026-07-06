using UnityEngine;

public class move : MonoBehaviour
{
    public static KeyCode globalKey = KeyCode.Space;
    public GameObject pivot;
    public Rigidbody2D rb2d;
    public float minAngle;
    public float maxAngle;
    public float jumpForce;
    public bool up;
    public float angleAddSpeed;
    public float angle;
    public bool grounded = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(globalKey))
        {
            if (grounded)
            {
                groundedMoveCheck();
            }
            
        }
        if (Input.GetKeyUp(globalKey))
        {
            rb2d.AddForce(pivot.transform.up * jumpForce);
        }
    }
    public void groundedMoveCheck()
    {
        if (up)
        {
            angle += angleAddSpeed * Time.deltaTime;
        }
        else
        {
            angle -= angleAddSpeed * Time.deltaTime;
        }
        angle = Mathf.Clamp(angle, minAngle, maxAngle);
        if (angle == minAngle)
        {
            up = true;
        }
        else if (angle == maxAngle)
        {
            up = false;
        }
        pivot.transform.localEulerAngles = new Vector3(0,0,angle);
    }
}
