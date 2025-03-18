using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DemoScript : MonoBehaviour
{
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isDragging = false;

    private Vector2 lastTouchPosition; // To track the previous touch position

    // Store the original rotation
    private Quaternion originalRotation;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        originalColor = objectRenderer.material.color;

        // Store the original rotation at the start
        originalRotation = transform.rotation;
    }

    private void Update()
    {
#if ENABLE_INPUT_SYSTEM
        if (isDragging)
        {
            if (Mouse.current != null)
            {
                // Get the mouse or touch position using the new Input System
                Vector3 touchPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                touchPosition.z = 0; // Ensure the object stays on the same plane

                // Move the object to the touch position
                transform.position = touchPosition;
            }
        }
#else
        if (isDragging)
        {
            // Get the mouse or touch position
            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            touchPosition.z = 0; // Ensure the object stays on the same plane

            // Move the object to the touch position
            transform.position = touchPosition;
        }
#endif

        // Gradually rotate the cube back to its original rotation
        RotateCubeBackToOriginal();
    }

    void OnTouchTap(Touch t)
    {
        ChangeColorToRed();
    }

    void OnTouchHold(Touch t)
    {
        isDragging = true;
        Debug.Log("tap Hold");
    }

    void OnTouchUntap(Touch t)
    {
        isDragging = false;
        Debug.Log("Untapped");
    }

    void OnSwipeEnter(Touch t)
    {
        Debug.Log(gameObject.name + " was swiped!");
        lastTouchPosition = t.position; // Store the initial position on swipe start
    }

    void OnSwipeExit(Touch t)
    {
        isDragging = false;
    }

    void OnSwipeStay(Touch t)
    {
        // Calculate the swipe direction
        Vector2 swipeDirection = t.position - lastTouchPosition;

        // Check if the swipe is going left or right based on the x-component
        if (swipeDirection.x > 0)
        {
            RotateCube(-45, 400);
        }
        else if (swipeDirection.x < 0)
        {
            RotateCube(45, 400);
        }

        // Update the lastTouchPosition to the current touch position for the next frame
        lastTouchPosition = t.position;
    }

    void RotateCube(float rotationAngle, float rotationSpeed)
    {
        // Target rotation
        Quaternion targetRotation = Quaternion.Euler(0, 0, rotationAngle);

        // Rotate towards the target rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void RotateCubeBackToOriginal()
    {
        // Rotate towards the original rotation
        transform.rotation = Quaternion.RotateTowards(transform.rotation, originalRotation, 100 * Time.deltaTime);
    }

    private void ChangeColorToRed()
    {
        objectRenderer.material.color = Color.red;
        StartCoroutine(RevertColorAfterTime(0.1f));
    }

    private IEnumerator RevertColorAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        objectRenderer.material.color = originalColor;
    }
}
