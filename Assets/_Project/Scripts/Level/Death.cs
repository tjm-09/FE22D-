using UnityEngine;

public class Death : MonoBehaviour
{
    public GameObject StartPoint;
    public GameObject Player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player Died!");
           Player.transform.position = StartPoint.transform.position;
        }
    }
}
