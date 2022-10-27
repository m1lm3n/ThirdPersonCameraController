using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class ThirdPersonCameraController : MonoBehaviour
{
    public Transform Target;
    public int MouseButton = 1; // right button by default
    public bool RotationX = true;
    public bool RotationY = true;
    public float Distance = 20;
    public float MinDistance = 1;
    public float MaxDistance = 20;
    public float ZoomSpeedMouse = 1;
    public float ZoomSpeedTouch = 0.2f;
    public float RotationSpeed = 10;
    public float xMinAngle = -90;
    public float xMaxAngle = 90;
    public Vector3 Camera_offset = Vector3.zero;
    public LayerMask viewBlockingLayers;
    public float DetectionRadius = 0.25f;

    private UnityEngine.Color cameraRayColor = UnityEngine.Color.red;
    private Vector3 rotation = Vector3.zero;
    private Vector3 targetPos = Vector3.zero;
    private bool rotationInitialized;
    private bool cursorLocked = false;
    private bool cursorPositionSaved = false;

    #region Set Cursor Position
    // private MousePosition mp;
    // [DllImport("user32.dll")]
    // public static extern bool SetCursorPos(int X, int Y);
    // [DllImport("user32.dll")]
    // [return: MarshalAs(UnmanagedType.Bool)]
    // private static extern bool GetCursorPos(out MousePosition lpMousePosition);

    // [StructLayout(LayoutKind.Sequential)]
    // public struct MousePosition
    // {
    //     public int x;
    //     public int y;
    // }
    #endregion

    void Start()
    {
        InitialRotation();
    }

    void Update()
    {
        if (Target != null)
        {
            ApplyCameraOffset();

            if (!CameraUtilities.IsCursorOverUserInterface())
            {
                if (Input.mousePresent)
                {
                    if (Input.GetMouseButton(MouseButton))
                    {
                        CameraRotation();
                        LockCursor();
                    }
                    else
                    {
                        UnlockCursor();
                    }
                }
                else
                {
                    InitialRotation();
                }

                CameraZoom();
            }

            ApplyCameraPosition();
            CameraCollision();
        }
    }

    private void InitialRotation()
    {
        transform.rotation = Quaternion.Euler(new Vector3(45, 0, 0));
    }
    private void CameraZoom()
    {
        float speed = Input.mousePresent ? ZoomSpeedMouse : ZoomSpeedTouch;
        float step = CameraUtilities.GetZoomUniversal() * speed;
        Distance = Mathf.Clamp(Distance - step, MinDistance, MaxDistance);
    }
    private void CameraCollision()
    {
        Ray ray = new Ray(targetPos, -this.transform.forward);
        RaycastHit raycastHit;

        Debug.DrawLine(targetPos, transform.position, cameraRayColor);

        if (Physics.SphereCast(ray, DetectionRadius, out raycastHit, Distance, viewBlockingLayers))
        {
            float distance = Vector3.Distance(targetPos, raycastHit.point);

            transform.position = targetPos - (transform.rotation * Vector3.forward * distance);
        }
    }
    private void CameraRotation()
    {
        if (!rotationInitialized)
        {
            rotation = transform.eulerAngles;
            rotationInitialized = true;
        }
        if (RotationX)
        {
            rotation.y += Input.GetAxis("Mouse X") * RotationSpeed;
        }
        else
        {
            rotation.y = 45;
        }
        if (RotationY)
        {
            rotation.x -= Input.GetAxis("Mouse Y") * RotationSpeed;
        }
        else
        {
            rotation.x = 45;
        }
        rotation.x = Mathf.Clamp(rotation.x, xMinAngle, xMaxAngle);
        transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
    }
    private void ApplyCameraPosition()
    {
        transform.position = targetPos - (transform.rotation * Vector3.forward * Distance);
    }
    private void ApplyCameraOffset()
    {
        targetPos = Target.position + Camera_offset;
    }
    private void LockCursor()
    {
        cursorLocked = true;
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        SaveCursorPosition();
    }
    private void UnlockCursor()
    {
        if (cursorLocked)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Confined;
            cursorLocked = false;
            UnityEngine.Cursor.visible = true;
            SetCursorPosition();
        }
    }
    private void SaveCursorPosition()
    {
        if (!cursorPositionSaved)
        {
            // GetCursorPos(out mp);
            cursorPositionSaved = true;
        }
    }
    private void SetCursorPosition()
    {
        // SetCursorPos(mp.x, mp.y);
        cursorPositionSaved = false;
    }
    // Draw a Gizmo around where the camera has been projected to.
    void OnDrawGizmos()
    {
        Gizmos.color = UnityEngine.Color.cyan;
        Gizmos.DrawWireSphere(transform.position, DetectionRadius);
    }
}
