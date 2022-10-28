using System.Security.Cryptography.X509Certificates;
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
    public LayerMask ViewBlockingLayers;
    public float DetectionRadius = 1f;
    public List<Vector3> pivotPointsEnded = new List<Vector3>
                {
                    Vector3.zero,
                    new Vector3(0, 0.5f, 0),
                    new Vector3(0, -0.5f, 0),
                    new Vector3(0.5f, 0, 0),
                    new Vector3(-0.5f, 0, 0),
                };
    public List<Vector3> pivotPointsStarted = new List<Vector3>
                {
                    Vector3.zero,
                    new Vector3(0, 0.5f, 0),
                    new Vector3(0, -0.5f, 0),
                    new Vector3(0.5f, 0, 0),
                    new Vector3(-0.5f, 0, 0),
                };

    public float SphereCastRadius = 0.25f;
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
            CalculatePivotPoint();
            CalculatePivotPointStarted();
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
            CameraCollisionNew();
        }
    }
    private float CalculateOverallGizmoRadius()
    {
        DetectionRadius = (SphereCastRadius) * 3;
        return ((SphereCastRadius) * 3);
    }
    private float CalculateDetectionRadius()
    {
        return (((SphereCastRadius * 2) * 3) - SphereCastRadius * 3) - SphereCastRadius;
    }
    private float CalculateDetectionStartPoints()
    {
        return (((SphereCastRadius * 2) * 3) - SphereCastRadius * 3) - SphereCastRadius;
    }

    private void CalculatePivotPoint()
    {
        for (int i = 0; i < pivotPointsEnded.Count; i++)
        {
            switch (i)
            {

                case 0:
                    pivotPointsEnded[i] = Vector3.zero;
                    break;
                case 1:
                    pivotPointsEnded[i] = new Vector3(0, CalculateDetectionRadius(), 0); //Up
                    break;
                case 2:
                    pivotPointsEnded[i] = new Vector3(0, -(CalculateDetectionRadius()), 0); //Down
                    break;
                case 3:
                    pivotPointsEnded[i] = new Vector3(CalculateDetectionRadius(), 0, 0); //Right
                    break;
                case 4:
                    pivotPointsEnded[i] = new Vector3(-(CalculateDetectionRadius()), 0, 0); //Left
                    break;

            }
        }
    }
    private void CalculatePivotPointStarted()
    {
        for (int i = 0; i < pivotPointsStarted.Count; i++)
        {
            switch (i)
            {

                case 0:
                    pivotPointsStarted[i] = Vector3.zero;
                    break;
                case 1:
                    pivotPointsStarted[i] = new Vector3(0, CalculateDetectionStartPoints(), 0); //Up
                    break;
                case 2:
                    pivotPointsStarted[i] = new Vector3(0, -(CalculateDetectionStartPoints()), 0); //Down
                    break;
                case 3:
                    pivotPointsStarted[i] = new Vector3(CalculateDetectionStartPoints(), 0, 0); //Right
                    break;
                case 4:
                    pivotPointsStarted[i] = new Vector3(-(CalculateDetectionStartPoints()), 0, 0); //Left
                    break;

            }
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
    private void CameraCollisionNew()
    {
        Vector3 backTowardsCamera = -this.transform.forward;
        for (int i = 0; i < pivotPointsEnded.Count; i++)
        {
            Vector3 startingPosition = targetPos;
            startingPosition += this.transform.right * pivotPointsStarted[i].x;
            startingPosition += this.transform.up * pivotPointsStarted[i].y;
            startingPosition += this.transform.forward *pivotPointsStarted[i].z;

            Vector3 endedPosition = this.transform.position;
            endedPosition += (this.transform.right * pivotPointsEnded[i].x);
            endedPosition += this.transform.up *  pivotPointsEnded[i].y;
            endedPosition += this.transform.forward *  pivotPointsEnded[i].z;

            Ray ray = new Ray(startingPosition, endedPosition);
            RaycastHit raycastHit;
            Debug.DrawLine(startingPosition, endedPosition, UnityEngine.Color.green);


            if (Physics.SphereCast(ray, SphereCastRadius, out raycastHit, Distance, ViewBlockingLayers))
            {
                Debug.DrawLine(startingPosition, endedPosition, cameraRayColor);

                float distance = Vector3.Distance(targetPos, raycastHit.point);

                transform.position = targetPos - (transform.rotation * Vector3.forward * distance);
            }
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
    private void DrawWireSphere(Vector3 pos, float radius, UnityEngine.Color color, int subDivNum = 64)
    {
        Gizmos.color = color;
        float stepAngle = 90f / subDivNum;

        Vector3 lp1 = DrawSubWireSpherePart(pos, radius, Vector3.forward, Vector3.right, stepAngle, subDivNum);
        Vector3 lp2 = DrawSubWireSpherePart(pos, radius, Vector3.forward, -Vector3.right, stepAngle, subDivNum);

        Vector3 lp3 = DrawSubWireSpherePart(pos, radius, -Vector3.forward, Vector3.right, stepAngle, subDivNum);
        Vector3 lp4 = DrawSubWireSpherePart(pos, radius, -Vector3.forward, -Vector3.right, stepAngle, subDivNum);

        Vector3 lp5 = DrawSubWireSpherePart(pos, radius, Vector3.right, Vector3.forward, stepAngle, subDivNum);
        Vector3 lp6 = DrawSubWireSpherePart(pos, radius, Vector3.right, -Vector3.forward, stepAngle, subDivNum);

        Vector3 lp7 = DrawSubWireSpherePart(pos, radius, -Vector3.right, Vector3.forward, stepAngle, subDivNum);
        Vector3 lp8 = DrawSubWireSpherePart(pos, radius, -Vector3.right, -Vector3.forward, stepAngle, subDivNum);

        Gizmos.DrawLine(lp1, lp2);
        Gizmos.DrawLine(lp3, lp4);
        Gizmos.DrawLine(lp5, lp6);
        Gizmos.DrawLine(lp7, lp8);
    }

    private Vector3 DrawSubWireSpherePart(Vector3 pos,
                                                 float radius,
                                                 Vector3 axis,
                                                 Vector3 rotationAxis,
                                                 float stepAngle,
                                                 int subDivNum)
    {
        Vector3 dirVector = axis * radius;

        Vector3 lastPoint = pos;

        for (int i = subDivNum - 1; i > 1; i--)
        {
            Vector3 prevPoint = pos + Quaternion.AngleAxis((i + 1) * stepAngle, rotationAxis) * dirVector;
            lastPoint = pos + Quaternion.AngleAxis(i * stepAngle, rotationAxis) * dirVector;

            Gizmos.DrawLine(prevPoint, lastPoint);
        }

        return lastPoint;
    }

    void OnDrawGizmos()
    {
        DrawWireSphere(this.transform.position, CalculateOverallGizmoRadius(), UnityEngine.Color.red);

        Vector3 startingPosition_Center = this.transform.position;
        startingPosition_Center += this.transform.right * pivotPointsEnded[0].x;
        startingPosition_Center += this.transform.up * pivotPointsEnded[0].y;
        startingPosition_Center += this.transform.forward * pivotPointsEnded[0].z;
        DrawWireSphere(startingPosition_Center, SphereCastRadius, UnityEngine.Color.green);

        Vector3 startingPosition_Up = this.transform.position;
        startingPosition_Up += this.transform.right * pivotPointsEnded[1].x;
        startingPosition_Up += this.transform.up * pivotPointsEnded[1].y;
        startingPosition_Up += this.transform.forward * pivotPointsEnded[1].z;
        DrawWireSphere(startingPosition_Up, SphereCastRadius, UnityEngine.Color.green);

        Vector3 startingPosition_Down = this.transform.position;
        startingPosition_Down += this.transform.right * pivotPointsEnded[2].x;
        startingPosition_Down += this.transform.up * pivotPointsEnded[2].y;
        startingPosition_Down += this.transform.forward * pivotPointsEnded[2].z;
        DrawWireSphere(startingPosition_Down, (SphereCastRadius), UnityEngine.Color.green);

        Vector3 startingPosition_Right = this.transform.position;
        startingPosition_Right += this.transform.right * pivotPointsEnded[3].x;
        startingPosition_Right += this.transform.up * pivotPointsEnded[3].y;
        startingPosition_Right += this.transform.forward * pivotPointsEnded[3].z;
        DrawWireSphere(startingPosition_Right, (SphereCastRadius), UnityEngine.Color.green);

        Vector3 startingPosition_Right_Started = targetPos;
        startingPosition_Right_Started += this.transform.right * pivotPointsStarted[3].x;
        startingPosition_Right_Started += this.transform.up * pivotPointsStarted[3].y;
        startingPosition_Right_Started += this.transform.forward * pivotPointsStarted[3].z;
        DrawWireSphere(startingPosition_Right_Started, (SphereCastRadius), UnityEngine.Color.green);

        Vector3 startingPosition_Left = this.transform.position;
        startingPosition_Left += this.transform.right * pivotPointsEnded[4].x;
        startingPosition_Left += this.transform.up * pivotPointsEnded[4].y;
        startingPosition_Left += this.transform.forward * pivotPointsEnded[4].z;
        DrawWireSphere(startingPosition_Left, SphereCastRadius, UnityEngine.Color.green);

        Vector3 startingPosition_Left_Started = targetPos;
        startingPosition_Left_Started += this.transform.right * pivotPointsStarted[4].x;
        startingPosition_Left_Started += this.transform.up * pivotPointsStarted[4].y;
        startingPosition_Left_Started += this.transform.forward * pivotPointsStarted[4].z;
        DrawWireSphere(startingPosition_Left_Started, SphereCastRadius, UnityEngine.Color.green);
    }
}
