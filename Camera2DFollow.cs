using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Camera2DFollow : MonoBehaviour
{
    public Transform target;
    private float Xoffset;
    public Transform Camera;
    private Vector2 newPosition;
    float lerpDuration = 2f;
    float endValue = 1.9f;
    private bool isJumping = false;
    private bool isUmbrella = false;
    private bool isDashing = false;

    private void Awake()
    {
        Xoffset = 0.5f;
    }
    private void Update()
    {
        newPosition.x = target.position.x + Xoffset;
        Camera.position = newPosition;

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
            Xoffset = Mathf.Lerp(Xoffset, endValue, lerpDuration * Time.deltaTime);
        }
        else
        {
            Xoffset = Mathf.Lerp(Xoffset, 1f, 3f * Time.deltaTime);
        }
    }

    public void OnJump()
    {
        isJumping = true;
    }
    public void OnUmbrella()
    {
        StartCoroutine(PopUmbrellaSlow());
    }
    public void OnDash()
    {
        isDashing = true;
    }
    IEnumerator PopUmbrellaSlow()
    {
        yield return new WaitForSeconds(0.3f);
        isUmbrella = true;
    }

    IEnumerator OnLand()
    {
        yield return new WaitForSeconds(0.3f);
        isJumping = false;
        isUmbrella = false;
    }
    IEnumerator OnLandUmbrella()
    {
        yield return new WaitForSeconds(0.45f);
        isJumping = false;
        isUmbrella = false;
    }
    IEnumerator AfterDash()
    {
        yield return new WaitForSeconds(0.2f);
        isDashing = false;
    }
}


