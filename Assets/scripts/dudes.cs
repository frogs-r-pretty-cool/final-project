using UnityEngine;
using System.Collections;

public class dudes : MonoBehaviour
{
    public Transform arm;
    public GameObject ballPrefab;
    public Transform spawnPoint;
    public Rigidbody2D playerRb;
    public float jumpForce = 10f;
    public float armSpeed = 30f;
    public float minAngle = 10f;
    public float maxAngle = 80f;
    public float throwPower = 7f;
    private float currentAngle;
    private bool hasJumped = false;
    private GameObject heldBall = null;
    public bool holdingBall = false;
    private GameObject currentBall;
    private bool isLanding = false;
    private Vector2 initialPosition;
    private Coroutine landingCoroutine = null;  // Reference to the landing oscillation coroutine

    void Start()
    {
        currentAngle = minAngle;
        arm.rotation = Quaternion.Euler(0, 0, currentAngle);
        SpawnBall();
        initialPosition = transform.position; // Store starting position
    }

    void Update()
    {
        // Allow jump input whether or not we are in landing oscillation.
        if (!hasJumped && Input.GetKeyDown(KeyCode.Space))
        {
            // If we're currently leaning/oscillating, cancel that oscillation.
            if (isLanding && landingCoroutine != null)
            {
                StopCoroutine(landingCoroutine);
                landingCoroutine = null;
                isLanding = false;
            }
            Jump();
        }

        if (holdingBall && Input.GetKeyUp(KeyCode.Space))
        {
            ThrowBall();
        }

        // Keep feet planted when not jumping (even during oscillation)
        if (!hasJumped)
        {
            transform.position = new Vector2(initialPosition.x, transform.position.y);
            playerRb.velocity = new Vector2(0, playerRb.velocity.y);
        }
    }

    // Modified Jump() so that the jump force is applied in the direction of the current lean.
    void Jump()
    {
        hasJumped = true;
        // Jump in the direction the character is leaning (using transform.up)
        playerRb.velocity = transform.up * jumpForce;
    }

    void ThrowBall()
    {
        if (heldBall == null) return;

        heldBall.transform.SetParent(null);
        Rigidbody2D rb = heldBall.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
            float throwAngle = arm.rotation.eulerAngles.z * Mathf.Deg2Rad;
            Vector2 throwDirection = new Vector2(Mathf.Cos(throwAngle), Mathf.Sin(throwAngle));
            rb.velocity = throwDirection * throwPower;
        }
        heldBall = null;
        holdingBall = false;
        Invoke("SpawnBall", 1.5f);
    }

    void SpawnBall()
    {
        if (spawnPoint == null) return;
        GameObject newBall = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
        GrabBall(newBall);
        currentBall = newBall;
    }

    public void GrabBall(GameObject ball)
    {
        holdingBall = true;
        heldBall = ball;
        if (currentBall != null)
        {
            Destroy(currentBall);
        }
        heldBall.transform.SetParent(arm);
        heldBall.transform.localPosition = Vector3.zero;
        Rigidbody2D rb = heldBall.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // When landing after a jump, trigger the oscillation effect.
        if (hasJumped)
        {
            hasJumped = false;
            isLanding = true;
            landingCoroutine = StartCoroutine(OscillateToStop());
        }
    }

    private IEnumerator OscillateToStop()
    {
        // Use a sinusoidal oscillation with exponential damping
        float amplitude = 45f;  // Maximum lean angle in degrees
        float speed = 4f;       // Oscillation speed
        int cycles = 6;         // Total oscillation cycles
        Vector3 initPos = transform.position; // Lock position
        float timeElapsed = 0f;
        float totalDuration = cycles * Mathf.PI / speed; // Total oscillation duration

        while (timeElapsed < totalDuration)
        {
            float progress = timeElapsed / totalDuration;
            // Calculate the angle using a cosine wave damped over time
            float angle = amplitude * Mathf.Exp(-2f * progress) * Mathf.Cos(speed * timeElapsed);
            transform.rotation = Quaternion.Euler(0, 0, angle);
            transform.position = initPos;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        // Reset rotation to neutral when finished oscillating.
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.position = initPos;
        isLanding = false;
    }
}
