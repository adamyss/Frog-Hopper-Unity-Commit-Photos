using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
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
    public DistanceJoint2D j;
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
    Vector3 offset;
    public Vector3 lastCheckPoint;
    public float minForce;
    public float maxForce;
    public GameObject forceBarPivot;
    public bool scaleyUpy;
    public float forceChangeSpeed;
    public GameObject pivotDisplayer;
    public float maxScale = 0.35f;
    public PlayableDirector slow;
    public float speedy = 0.75f;
    public float[] possibleYPoints;
    public float nextYPos;
    public bool dieing;
    public float afterBoost;
    public Collider2D coly;
    public LineRenderer trajectoryLine;
    public int lineAmount = 20;
    public float adder = 0.27f;
    public GameObject landMarker;
    public bool assist;
    public bool hovering;
    public RawImage rimage;
    public Color selected;
    public Color hover;
    public ParticleSystem jumpy;
    public ParticleSystem toungeHit;
    public Vector3 offsety;

    public void startHover()
    {
        hovering = true;
    }
    public void endHover()
    {
        hovering = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slow.playableGraph.GetRootPlayable(0).SetSpeed(speedy);
        offset = camOffset.transform.position;
        offsety = jumpy.transform.position - transform.position;
    }
    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(transform.position,grappleRadius);
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
  

    public void drawTrajectory(Vector2 startPos, Vector2 vel, float gravy)
    {
        if(assist == false)
        {
            trajectoryLine.enabled = false;
            return;
        }
        trajectoryLine.positionCount = lineAmount;
        Vector3[] points = new Vector3[lineAmount];
        trajectoryLine.positionCount = lineAmount;
        int breakInt = 900;
        for (int i = 0; i < lineAmount; i++)
        {
            // got some of this math of stack overflow and just editted it abit
            float stepy = i / 20f;
            float dx = vel.x * stepy;
            float dy = (vel.y * stepy) - (0.5f * Mathf.Abs(Physics2D.gravity.y) * gravy * stepy * stepy);
            points[i] = new Vector3(startPos.x + dx, startPos.y + dy, 0f);
            if(breakInt == i)
            {
                trajectoryLine.positionCount = i;
                landMarker.transform.position = points[i];
                break;
            }
            if (Physics2D.OverlapCircle(points[i],1/lineAmount,ground) == true)
            {
                breakInt = i + 1;
               
               
            }
        }
        trajectoryLine.SetPositions(points);
    }
    // Update is called once per frame
    void Update()
    {
        if (dieing)
        {
            return;
        }
        if (hovering)
        {
            rimage.color = hover;
        }
        else
        {
            if (assist)
            {
                rimage.color = selected;
            }
            else
            {
                rimage.color = Color.white;
            }
        }
        if(hovering && Input.GetKeyDown(KeyCode.Space))
        {
            assist = !assist;
            return;
            
        }
        if (grounded)
        {
  
           
            forceBarPivot.transform.localScale = new Vector3(forceBarPivot.transform.localScale.x,Mathf.Lerp(0,maxScale,Mathf.InverseLerp(minForce,maxForce,jumpForce)),1);
        }


        // :D 
        if(j.enabled == true)
        {
            tongue.transform.position = targetGrapple.transform.position;
            float dist = Vector2.Distance(transform.position,targetGrapple.transform.position);
            if(dist+adder <j.distance )
            {
                j.distance = dist + adder;
            }
            if(j.distance < 0.5f)
            {
                j.distance = 0.5f;
            }
        }
        if (targetGrapple == null && grounded == false)
        {
            closest = checkGrapplePoints();
            if (closest != null)
            {
                closest.GetComponent<SpriteRenderer>().color = Color.blue;
            }
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
                if(vecy.x < 0)
                {
                    transform.eulerAngles = new Vector3(0, 180, 0);
                } else if (vecy.x > 0){
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }
            }
        }
        // more math I stole from unity forumns >:D
        yPos = yOffsety();
        if (transform.eulerAngles.y == 0 || j.enabled == true && rb2d.linearVelocityX > 0)
        {
            camOffset.transform.position = new Vector2(transform.position.x + offset.x, yPos);
        } else if(transform.eulerAngles.y == 180 || j.enabled == true && rb2d.linearVelocityX < 0)
        {
            camOffset.transform.position = new Vector2(transform.position.x - offset.x, yPos);
        }
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
    public float yOffsety()
    {
        
        float yy = possibleYPoints[0];
        foreach(float f in possibleYPoints)
        {
            if(transform.position.y - nextYPos < f)
            {
                yy = f;
            }
        }
        return yy;
    }
    public IEnumerator die()
    {
        dieing = true;
        anim.Play("die");
        yield return new WaitForSeconds(0.45f);
        transform.position = lastCheckPoint;
        anim.Play("IDLE");
        dieing = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("hit"))
        {
            StartCoroutine(die());
        }
      
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("checkpoint"))
        {
            lastCheckPoint = collision.gameObject.transform.position;
        }
    }
    public IEnumerator startyGraply()
    {
        grappleLook();

        Vector2 prev = rb2d.linearVelocity;
        rotFreeze = true;
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.linearVelocity = Vector3.zero;
        anim.Play("toungeOut");
        yield return new WaitForSeconds(outTime);
        transform.parent = targetGrapple.transform;
        Vector3 movyPos;
        lr.SetPosition(0, tongueStart.transform.position);
        lr.SetPosition(1, tongueStart.transform.position);
        float timer = 0;
        lr.enabled = true;
        tongue.SetActive(true);
        tongue.transform.position = tongueStart.transform.position;
        while(timer < moveOutTime)
        {
            movyPos = Vector3.Lerp(tongueStart.transform.position, targetGrapple.transform.position, timer / moveOutTime);
            lr.SetPosition(1, movyPos);
            tongue.transform.position = movyPos;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
      
        rb2d.bodyType = RigidbodyType2D.Dynamic;
        rb2d.linearVelocity = prev;
        j.distance = Vector2.Distance(transform.position,targetGrapple.transform.position);
          j.enabled = true;
        targetGrapple.GetComponent<SpriteRenderer>().color = Color.yellow;
        targetGrapplerb2d = targetGrapple.GetComponent<Rigidbody2D>();
        j.connectedBody = targetGrapplerb2d;
        tongue.transform.position = targetGrapple.transform.position;
        toungeHit.Play();
        rotFreeze = false;
    }
    public IEnumerator unGrapply()
    {
    
        toungeHit.Stop();
        toungeHit.Clear();
        transform.parent = null;
        rotFreeze = true;
        Vector2 prev = rb2d.linearVelocity;
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.linearVelocity = Vector3.zero;
        Vector3 movyPos;
        float timer = 0;
        lr.enabled = true;
        while (timer < inTime)
        {
            movyPos = Vector3.Lerp(tongueStart.transform.position, tongue.transform.position, Mathf.Lerp(1,0,timer / inTime));
            tongue.transform.position = movyPos;
            lr.SetPosition(1, movyPos);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        tongue.SetActive(false);
        anim.Play("toungeIn");
        lr.enabled = false;
        rb2d.bodyType = RigidbodyType2D.Dynamic;
        rb2d.linearVelocity = prev;
        rb2d.AddForce(prev * afterBoost);
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
       
       
        StartCoroutine(jump());

    }
    public IEnumerator jump()
    {
        landMarker.SetActive(true);
        rotFreeze = true;
        forceBarPivot.SetActive(true);
        pivotDisplayer.SetActive(true);
        trajectoryLine.enabled = true;
        jumpForce = minForce;
        scaleyUpy = true;
        Vector2 dir = pivot.transform.right;
        dir = dir.normalized;
        dir.y *= yMult;
        if (assist)
        {
            pivot.SetActive(false);
        }
        while (!Input.GetKey(globalKey))
        {
            drawTrajectory(transform.position, dir * jumpForce * Time.fixedDeltaTime, rb2d.gravityScale);
            if (scaleyUpy)
            {
                jumpForce += forceChangeSpeed * Time.deltaTime;
            }
            else
            {
                jumpForce -= forceChangeSpeed * Time.deltaTime;
            }
            jumpForce = Mathf.Clamp(jumpForce, minForce, maxForce);
            if (jumpForce == minForce)
            {
                scaleyUpy = true;
            }
            else if (jumpForce == maxForce)
            {
                scaleyUpy = false;
            }
            yield return new WaitForEndOfFrame();
        }
        landMarker.SetActive(false);
        trajectoryLine.enabled = false;
        pivotDisplayer.SetActive(false);
        forceBarPivot.SetActive(false);
        checkable = false;
        anim.SetBool("hitGround", false);
        anim.Play("jump");
        pivot.SetActive(false);
        yield return new WaitForSeconds(jumpTime);
        jumpy.transform.eulerAngles = new Vector3(0,transform.eulerAngles.y,jumpy.transform.eulerAngles.z);
        if(transform.eulerAngles.y == 180)
        {
            jumpy.transform.position = transform.position + offsety;
        }
        else
        {
            jumpy.transform.position = transform.position - offsety;
        }
        jumpy.Play();
        Debug.Log("direction: " + dir);
        coly.enabled = false;
        
        rb2d.AddForce(dir * jumpForce);
        yield return new WaitForSeconds(0.05f);
        coly.enabled = true;
        yield return new WaitForSeconds(0.15f);
        rotFreeze = false;
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
