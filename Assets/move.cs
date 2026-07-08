using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class move : MonoBehaviour
{
    // just the variables and stuffs
    public KeyCode globalKey = KeyCode.Space;
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
    public SpriteRenderer sr;
    // function called by assist ui object to let code know that the button is being hovered over :D
    public void startHover()
    {
        hovering = true;
    }
    // function called by assist ui object to let code know that the button is NOT being hovered over
    public void endHover()
    {
        hovering = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // just slowing down one of the tutorial animations cause it was too fast
        slow.playableGraph.GetRootPlayable(0).SetSpeed(speedy);
        // sets camera offset
        offset = camOffset.transform.position;
        // sets offset for jumping particle system
        offsety = jumpy.transform.position - transform.position;
    }
    private void OnDrawGizmos()
    {
        //Gizmos.DrawSphere(transform.position,grappleRadius);
    }
    // function that changes direction player is facing based off if mouse is on left or right side of screen
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
    // function that sets player to look at grappling hook
    public void grappleLook()
    {
        Vector3 lookyObjPos = targetGrapple.transform.position;
        Vector3 dir = lookyObjPos - transform.position;
        transform.right = dir;
    }
  
    // function to draw the trajectory of where player will land in assist mode
    public void drawTrajectory(Vector2 startPos, Vector2 vel, float gravy)
    {
        if(assist == false)
        {
            // does not draw trajectory if assist mode is not active
            trajectoryLine.enabled = false;
            return;
        }
        // sets up line renderer for line
        trajectoryLine.positionCount = lineAmount;
        Vector3[] points = new Vector3[lineAmount];
        trajectoryLine.positionCount = lineAmount;
        int breakInt = int.MaxValue;
        // for loop that calculates points to put on line renderer based off trajectory player will have
        for (int i = 0; i < lineAmount; i++)
        {
            // got some of this math of stack overflow and just editted it abit
            float stepy = i / 20f;
            float dx = vel.x * stepy;
            float dy = (vel.y * stepy) - (0.5f * Mathf.Abs(Physics2D.gravity.y) * gravy * stepy * stepy);
            points[i] = new Vector3(startPos.x + dx, startPos.y + dy, 0f);
            if(breakInt == i)
            {
                // ends loop early if breakInt is that same as i
                trajectoryLine.positionCount = i;
                landMarker.transform.position = points[i];
                break;
            }
            // checks if line hits an object and if so sets breakInt to be equal to i on the next itteration
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
        // dont do anything if dead lol
        if (dieing)
        {
            return;
        }
        // sets assist mode ui color based of being selected hovered or off
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
        // changes assist mode if ui is hovered over and space is pressed
        if(hovering && Input.GetKeyDown(KeyCode.Space))
        {
            assist = !assist;
            return;
            
        }
        // sets the scale of a spritemask to be based off of current jumpforce compared to minimum and maximum which basically makes the animation of the bar going up and down
        if (grounded)
        {
            forceBarPivot.transform.localScale = new Vector3(forceBarPivot.transform.localScale.x,Mathf.Lerp(0,maxScale,Mathf.InverseLerp(minForce,maxForce,jumpForce)),1);
        }


        // :D 
        /* if grappling hook enabled set tounge position to the target grappling point aswell as moving player closer to it by lowering 
         distance joint distance if close enough idk if said distance code actually helps or if its just the distance joint doing it but im
         to scared to mess it up so code stays >:D */
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
        // if in air check closest grappling point and set its color so player knows
        if (targetGrapple == null && grounded == false)
        {
            closest = checkGrapplePoints();
            if (closest != null)
            {
                closest.GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }
        // resets closest grapple point color when landing on ground
        if (grounded == true && closest != null)
        {
            closest.GetComponent<SpriteRenderer>().color = Color.red;
            closest = null;
        }
        // runs grapple logic if in air when space is pressed
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
        // if not freezing rotation rotate player based off on ground, in air, or attatched to grapple
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
        // sets y offset for camera
        yPos = yOffsety();
        // sets camera offset objects position
        if (transform.eulerAngles.y == 0 || j.enabled == true && rb2d.linearVelocityX > 0)
        {
            camOffset.transform.position = new Vector2(transform.position.x + offset.x, yPos);
        } else if(transform.eulerAngles.y == 180 || j.enabled == true && rb2d.linearVelocityX < 0)
        {
            camOffset.transform.position = new Vector2(transform.position.x - offset.x, yPos);
        }
        // checks if on ground
        grounded = checkGrounded();
        // if on ground runs either grappling hook logic or jump movement if possible
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
        // runs function for jumpy stuff if letting go of space on ground otherwhise ungrapple if grappled
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
        // calculates y offset point for cam
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
        // plays animation and sets player back to checkpoint
        dieing = true;
        anim.Play("die");
        yield return new WaitForSeconds(0.45f);
        transform.position = lastCheckPoint;
        anim.Play("IDLE");
        dieing = false;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if hit die
        if (collision.gameObject.CompareTag("hit"))
        {
            StartCoroutine(die());
        }
      
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // sets checkpoint position to hit checkpoint
        if (collision.gameObject.CompareTag("checkpoint"))
        {
            lastCheckPoint = collision.gameObject.transform.position;
        }
    }
    // code to set up rigidbody, linerenderer, and distance joint for grappling
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
    // code to reset all grappling related stuff (like the rotation and particlesystem), plays neccesary animations,  and add some force to rigidbody after leaving grappling state
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
    // calls the unGrapply courotuining aswell as well as disabling stuff like the distance joint 
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
    // finds closest grapple point in range
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
    // uses a series of raycasts to check if playerr is on the ground


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
    // calls when space is put up and starts jump corutine if possible
    public void groundedKeyUp()
    {
        if (!jumpable)
        {
            return;
        }
        jumpable = false;
       
       
        StartCoroutine(jump());

    }
    // code to scale the jump force, calls draw trajectory function if in assist mopde, aswell as add force to the player to launch them after
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
    // runs code if holding space to move the pivot (arrow bar) to adjust player launch direction
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
