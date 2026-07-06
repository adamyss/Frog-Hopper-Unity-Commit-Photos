using System.Collections;
using TMPro.EditorUtilities;
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
    public float grappleRadius;
    public GameObject targetGrapple;
    public LineRenderer lr;
    public LayerMask grapple;
    public Joint2D j;
    Rigidbody2D targetGrapplerb2d;
    public GameObject tongue;
    public GameObject tongueStart;
    public float lookOffset;
    GameObject closest;
    public float outTime;
    public float inTime;
    public float furthestMult;
    public float moveTime;
    public bool rotFreeze = false;
    public float moveOutTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position,grappleRadius);
    }
    public void mouseLook()
    {
        Vector3 mousePos = Input.mousePosition;
        if(mousePos.x < Screen.width / 2)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }
    public void grappleLook()
    {
       
        Vector3 lookyObjPos = targetGrapple.transform.position;
        Vector3 dir = lookyObjPos - transform.position;
        transform.right = dir;
    }

    // Update is called once per frame
    void Update()
    {
        // :D 
        if (targetGrapple == null && grounded == false)
        {
            closest = checkGrapplePoints();
            closest.GetComponent<SpriteRenderer>().color = Color.blue;
        }
        if (grounded == true && closest != null)
        {
            closest.GetComponent<SpriteRenderer>().color = Color.red;
            closest = null;
        }
        if (Input.GetKeyDown(globalKey))
        {
            if (!grounded)
            {
                targetGrapple =closest;
                if(targetGrapple != null)
                {
                    StartCoroutine(startyGraply());
                }
            }
        }
        if (rotFreeze == false)
        {
            if (!grounded && targetGrapple != null)
            {
                grappleLook();
            }
            else if (grounded)
            {
                mouseLook();
            }
            else
            {
                Vector3 vecy = (Vector3)rb2d.linearVelocity.normalized;
                vecy.y = 0;
                transform.right = transform.position + vecy;
            }
        }
        camOffset.transform.position = new Vector2(camOffset.transform.position.x, yPos);
        grounded = checkGrounded();
        if (Input.GetKey(globalKey))
        {
            if (grounded)
            {
                groundedMoveCheck();
            }
            else if(targetGrapple != null && j.enabled == true)
            {
                lr.SetPosition(0, tongueStart.transform.position);
                lr.SetPosition(1, targetGrapple.transform.position);
            }
            
        }
        if (Input.GetKeyUp(globalKey))
        {
            if (grounded)
            {
                groundedKeyUp();
            }
            unGrapple();
        }
    }
    public IEnumerator startyGraply()
    {
        rotFreeze = true;
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        anim.Play("toungeOut");
        yield return new WaitForSeconds(outTime);
        Vector3 movyPos;
        lr.SetPosition(0, tongueStart.transform.position);
        lr.SetPosition(1, tongueStart.transform.position);
        float timer = 0;
        lr.enabled = true;
        while(timer < moveOutTime)
        {
            movyPos = Vector3.Lerp(tongueStart.transform.position, targetGrapple.transform.position, timer / moveOutTime);
            lr.SetPosition(1, movyPos);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        rb2d.bodyType = RigidbodyType2D.Dynamic;
        tongue.SetActive(true);
        j.enabled = true;
        targetGrapple.GetComponent<SpriteRenderer>().color = Color.yellow;
        targetGrapplerb2d = targetGrapple.GetComponent<Rigidbody2D>();
        j.connectedBody = targetGrapplerb2d;
        tongue.transform.position = targetGrapple.transform.position;
        rotFreeze = false;
    }
    public IEnumerator unGrapply()
    {
        rotFreeze = true;
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        Vector3 movyPos;
        float timer = 0;
        lr.enabled = true;
        while (timer < inTime)
        {
            movyPos = Vector3.Lerp(tongueStart.transform.position, tongue.transform.position, Mathf.Lerp(1,0,timer / inTime));
            lr.SetPosition(1, movyPos);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        anim.Play("toungeIn");
        lr.enabled = false;
        rb2d.bodyType = RigidbodyType2D.Dynamic;
        rotFreeze = false;
        yield return null;
    }
    public void unGrapple()
    {
        if (targetGrapple != null)
        {
     
            targetGrapple.GetComponent<SpriteRenderer>().color = Color.red;
            targetGrapple = null;
            j.connectedBody = null;
            targetGrapplerb2d = null;
            j.enabled = false;
            StartCoroutine(unGrapply());
        }
    }
    public GameObject checkGrapplePoints()
    {
        GameObject closest = null;
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, grappleRadius,grapple);
        float closestDist = float.MaxValue;
        foreach(Collider2D col in cols)
        {
            col.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            float dist = Vector2.Distance(transform.position, col.transform.position);
            if(dist < closestDist)
            {
                closestDist = dist;
                closest = col.gameObject;
            }
        }
        return closest;
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
                    unGrapple();
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
