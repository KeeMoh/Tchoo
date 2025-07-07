using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private float mouseMovementThreshold = 0.5f;
    [SerializeField] private float controllerMovementThreshold = 0.2f;

    private Vector3 lastMousePosition;
    private bool isUsingController = true;

    void Start()
    {
        lastMousePosition = Input.mousePosition;
    }

    void Update()
    {
        CheckInputType();
    }

    private void CheckInputType()
    {
        // Check for controller input
        bool controllerInput = Mathf.Abs(Input.GetAxis("Horizontal")) > controllerMovementThreshold ||
                              Mathf.Abs(Input.GetAxis("Vertical")) > controllerMovementThreshold;

        // Check for mouse movement
        float mouseMovement = Vector3.Distance(Input.mousePosition, lastMousePosition);
        bool mouseInput = mouseMovement > mouseMovementThreshold;

        // Update cursor state
        if (controllerInput && !mouseInput)
        {
            if (!isUsingController)
            {
                HideCursor();
                isUsingController = true;
            }
        }
        else if (mouseInput)
        {
            if (isUsingController)
            {
                ShowCursor();
                isUsingController = false;
            }
        }

        lastMousePosition = Input.mousePosition;
    }

    private void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Debug.Log("Controller detected - Cursor hidden");
    }

    private void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Mouse detected - Cursor shown");
    }

    // Optional: Force cursor state
    public void ForceCursorState(bool showCursor)
    {
        if (showCursor)
            ShowCursor();
        else
            HideCursor();
    }
}