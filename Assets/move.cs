using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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
    public float raycastDists;
    public float[] leftRightOffsets;
    public LayerMask ground;
    public float groundCheckYOffset;
    public GameObject camOffset;
    public float yMult;
    public float yPos;
    public Animator anim;
    bool jumpable = true;
    public float jumpTime;
    bool checkable = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        camOffset.transform.position = new Vector2(camOffset.transform.position.x, yPos);
        grounded = checkGrounded();
        if (Input.GetKey(globalKey))
        {
            if (grounded)
            {
                groundedMoveCheck();
            }
            
        }
        if (Input.GetKeyUp(globalKey))
        {
            if (grounded)
            {
                groundedKeyUp();
            }
        }
    }
    public bool checkGrounded()
    {
        bool anyGround = false;
        foreach(float f in leftRightOffsets)
        {
            Vector3 pos = transform.position + new Vector3(f, groundCheckYOffset, 0);
            Color draw = Color.blue;
           
            if (Physics2D.Raycast(pos, Vector2.down, raycastDists, ground).collider != null){
                anyGround = true;
                if (checkable)
                {
                    anim.SetBool("hitGround", true);
                }
                draw = Color.red;
            }
            Debug.DrawRay(pos, Vector2.down, draw);
        }

        return anyGround;
    }
    public void groundedKeyUp()
    {
        if (!jumpable)
        {
            return;
        }
        jumpable = false;
        pivot.SetActive(false);
       
        StartCoroutine(jump());

    }
    public IEnumerator jump()
    {
        checkable = false;
        anim.SetBool("hitGround", false);
        anim.Play("jump");
  
        yield return new WaitForSeconds(jumpTime);
        Vector2 dir = pivot.transform.right;
        dir = dir.normalized;
        dir.y *= yMult;
        Debug.Log("direction: " + dir);
        rb2d.AddForce(dir * jumpForce);
        yield return new WaitForSeconds(0.2f);
        checkable = true;
        jumpable = true;
    }
    public void groundedMoveCheck()
    {
        if (!jumpable)
        {
            return;
        }
        pivot.SetActive(true);
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
