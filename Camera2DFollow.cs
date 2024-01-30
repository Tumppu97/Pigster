using System.Collections;
using UnityEngine;

public class Camera2DFollow : MonoBehaviour
{
    public Transform target; // The target object the camera follows (usually the player).
    private float Xoffset; // Horizontal offset from the target position.
    public Transform Camera; // The camera transform.
    private Vector2 newPosition; // The new position for the camera.
    float lerpDuration = 2f; // Duration for the lerp operation.
    float endValue = 1.9f; // End value for the lerp operation when the player stops.
    private bool isJumping = false; // Flag to indicate if the player is jumping.
    private bool isUmbrella = false; // Flag for umbrella state.
    private bool isDashing = false; // Flag for dashing state.

    private void Awake()
    {
        Xoffset = 0.5f; // Initialize the horizontal offset.
    }

    private void Update()
    {
        // Update the camera's position based on the player's position and the current offset.
        newPosition.x = target.position.x + Xoffset;
        Camera.position = newPosition;

        // Adjust the camera offset based on the player's current state.
        if (isUmbrella)
        {
            Xoffset = Mathf.Lerp(Xoffset, 1.11f, 5f * Time.deltaTime);
            StartCoroutine(OnLandUmbrella());
        }
        else if (isJumping)
        {
            Xoffset = Mathf.Lerp(Xoffset, 0.93f, 4f * Time.deltaTime);
            StartCoroutine(OnLand());
        }
        else if (isDashing)
        {
            Xoffset = Mathf.Lerp(Xoffset, 0.4f, 3f * Time.deltaTime);
            StartCoroutine(AfterDash());
        }
        else if (PlayerMovement2.topSpeed == 0)
        {
            // When the player stops, gradually move the camera to the end position.
            Xoffset = Mathf.Lerp(Xoffset, endValue, lerpDuration * Time.deltaTime);
        }
        else
        {
            // Return to the default offset when the player is moving normally.
            Xoffset = Mathf.Lerp(Xoffset, 1f, 3f * Time.deltaTime);
        }
    }

    public void OnJump()
    {
        isJumping = true; // Set flag when the player jumps.
    }

    public void OnUmbrella()
    {
        StartCoroutine(PopUmbrellaSlow()); // Start the umbrella animation.
    }

    public void OnDash()
    {
        isDashing = true; // Set flag when the player dashes.
    }

    // Coroutine to handle camera behavior after umbrella usage.
    IEnumerator PopUmbrellaSlow()
    {
        yield return new WaitForSeconds(0.3f);
        isUmbrella = true;
    }

    // Coroutine to reset flags after landing from a jump.
    IEnumerator OnLand()
    {
        yield return new WaitForSeconds(0.3f);
        isJumping = false;
        isUmbrella = false;
    }

    // Coroutine to reset flags after landing from an umbrella jump.
    IEnumerator OnLandUmbrella()
    {
        yield return new WaitForSeconds(0.45f);
        isJumping = false;
        isUmbrella = false;
    }

    // Coroutine to reset the dashing flag after dashing.
    IEnumerator AfterDash()
    {
        yield return new WaitForSeconds(0.2f);
        isDashing = false;
    }
}
