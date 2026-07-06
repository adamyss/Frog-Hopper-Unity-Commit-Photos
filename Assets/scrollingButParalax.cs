using UnityEngine;

public class scrollingButParalax : MonoBehaviour
{
    public GameObject camy;
    public float startPos;
    public float lengthy;
    public float parylaxyEffecty;
    public SpriteRenderer sr;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        startPos = transform.position.x;
        lengthy = sr.bounds.size.x;

    }

    // Update is called once per frame
    void Update()
    {
        float tempy = (camy.transform.position.x * (1 - parylaxyEffecty));
        float disty = (camy.transform.position.x * parylaxyEffecty);
        transform.position = new Vector3(startPos+disty,transform.position.y,transform.position.z);
        if(tempy > startPos + lengthy)
        {
            startPos += lengthy;
        } else if(tempy < startPos - lengthy)
        {
            startPos -= lengthy;
        }
    }
}
