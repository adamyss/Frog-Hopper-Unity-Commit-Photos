using System.Collections;
using UnityEngine;

public class testGrapple : MonoBehaviour
{
    public float jumpForce;
    public Rigidbody2D rb2d;
    public Vector2 jumpDir;
    public float timeBeforeGrapple;
    public float grappleTime;
    public float timeBeforeReset;
    Vector2 startPos;
    public Animator anim;
    public float jumpTime;
    public GameObject targetGrapple;
    public DistanceJoint2D j;
    public float outTime;
    public GameObject tongueStart;
    public float moveOutTime;
    public LineRenderer lr;
    public GameObject tongue;
    public Rigidbody2D targetGrapplerb2d;
    public float inTime;
    public float upBoost;
    public float afterBoost;
    public LayerMask ground;
    public float raycastDists;
    public GameObject launchUI1;
    public GameObject launchUI2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = transform.position;
        StartCoroutine(animationThingy());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // animation code for tutorial thingy literally just reusing the jump and grapple code from move script
    public IEnumerator animationThingy()
    {
        anim.Play("jump");
        yield return new WaitForSeconds(jumpTime);
        Vector2 diry = jumpDir;
        diry += Vector2.up * upBoost;
        rb2d.AddForce(diry * jumpForce);
       
        yield return new WaitForSeconds(timeBeforeGrapple);
        float timer = 0;
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.linearVelocity = Vector3.zero;
        Vector2 prev = rb2d.linearVelocity;
        anim.Play("toungeOut");
        targetGrapple.GetComponent<SpriteRenderer>().color = Color.blue;
        yield return new WaitForSeconds(outTime);
        tongue.SetActive(true);
        lr.enabled = true;
        lr.SetPosition(0, tongueStart.transform.position);
        Vector3 movyPos;
        lr.SetPosition(1, tongueStart.transform.position);
        Vector3 lookyObjPos = targetGrapple.transform.position;
        Vector3 dir = lookyObjPos - transform.position;
        transform.right = dir;
        launchUI1.SetActive(true);
        launchUI2.SetActive(true);
        while (timer < moveOutTime)
        {
            movyPos = Vector3.Lerp(tongueStart.transform.position, targetGrapple.transform.position, timer / moveOutTime);
            lr.SetPosition(1, movyPos);
            tongue.transform.position = movyPos;
            tongue.transform.position = movyPos;
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        j.connectedBody = targetGrapplerb2d;
        tongue.transform.position = targetGrapple.transform.position;
        rb2d.bodyType = RigidbodyType2D.Dynamic;
        rb2d.linearVelocity = prev;
        yield return new WaitForEndOfFrame();
        j.distance = Vector2.Distance(transform.position, targetGrapple.transform.position);
        j.enabled = true;
        targetGrapple.GetComponent<SpriteRenderer>().color = Color.yellow;
        targetGrapplerb2d = targetGrapple.GetComponent<Rigidbody2D>();
        float grapTimer = 0;
        Debug.Log("going to grap: " + grapTimer + "time: " + grappleTime);
        while (grapTimer < grappleTime)
        {
            Debug.Log("grappling: " + grapTimer);
            tongue.transform.position = targetGrapple.transform.position;
             lookyObjPos = targetGrapple.transform.position;
             dir = lookyObjPos - transform.position;
            transform.right = dir;
            lr.SetPosition(1, tongue.transform.position);
            lr.SetPosition(0, tongueStart.transform.position);
            grapTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        targetGrapple.GetComponent<SpriteRenderer>().color = Color.red;
        j.connectedBody = null;
        j.enabled = false;
        Vector2 prev2 = rb2d.linearVelocity;
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.linearVelocity = Vector3.zero;
        float inTimer = 0;
        launchUI1.SetActive(false);
        launchUI2.SetActive(false);
        while (inTimer < inTime)
        {
            movyPos = Vector3.Lerp(tongueStart.transform.position, tongue.transform.position, Mathf.Lerp(1, 0, timer / inTime));
            tongue.transform.position = movyPos;
            lr.SetPosition(1, movyPos);
            tongue.transform.position = movyPos;
            inTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        anim.Play("toungeIn");
        lr.enabled = false;
        tongue.SetActive(false);
        yield return new WaitForSeconds(moveOutTime);
        transform.eulerAngles = new Vector3(0, 0, 0);
        rb2d.bodyType = RigidbodyType2D.Dynamic;
        rb2d.linearVelocity = prev2;
        rb2d.AddForce(prev2 * afterBoost);
        float resetTimer = 0;
        bool hit = false;
        while(hit == false)
        {
            if (Physics2D.Raycast(transform.position, Vector2.down, raycastDists, ground).collider != null)
            {
                if (hit == false)
                {
                    anim.SetBool("hitGround", true);
                    hit = true;
                }
            }
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(timeBeforeReset);
        rb2d.linearVelocity = Vector2.zero;
        transform.position = startPos;
        StartCoroutine(animationThingy());
    }
}
