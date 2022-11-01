using System;
using System.Security.Cryptography.X509Certificates;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Linq;

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
    public List<GameObject> forDirectionPointsStarted = new List<GameObject>();
    public List<GameObject> forDirectionPointsEnded = new List<GameObject>();

    public float SphereCastRadius = 0.23f;
    private UnityEngine.Color cameraRayColor = UnityEngine.Color.red;
    private float xMinAngle = -90;
    private float xMaxAngle = 90;
    private Vector3 rotation = Vector3.zero;
    private Vector3 targetPos = Vector3.zero;
    private bool rotationInitialized;
    private bool cursorLocked = false;
    private bool cursorPositionSaved = false;
    private RaycastHit raycastHit = new RaycastHit();
    private float[] hitPoints = new[] { Mathf.Infinity, Mathf.Infinity, Mathf.Infinity, Mathf.Infinity, Mathf.Infinity };
    private Vector3[] hitPointsPosition = new[] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
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
        CreateStartedGameObjects();
        CreateEndedGameObjects();
    }


    private float CalculateOverallGizmoRadius()
    {
        DetectionRadius = (SphereCastRadius) * 3;
        return ((SphereCastRadius) * 3);
    }
    private float CalculateDetectionRadius()
    {
        return (SphereCastRadius * 2);
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

    private void CreateStartedGameObjects()
    {
        GameObject go1 = new GameObject();
        go1.name = "StartedPoint_Zero";
        forDirectionPointsStarted.Add(go1);

        GameObject go2 = new GameObject();
        go2.name = "StartedPoint_Up";
        forDirectionPointsStarted.Add(go2);

        GameObject go3 = new GameObject();
        go3.name = "StartedPoint_Down";
        forDirectionPointsStarted.Add(go3);

        GameObject go4 = new GameObject();
        go4.name = "StartedPoint_Right";
        forDirectionPointsStarted.Add(go4);

        GameObject go5 = new GameObject();
        go5.name = "StartedPoint_Left";
        forDirectionPointsStarted.Add(go5);
    }
    private void CreateEndedGameObjects()
    {
        GameObject go1 = new GameObject();
        go1.name = "EndPoint_Zero";
        forDirectionPointsEnded.Add(go1);

        GameObject go2 = new GameObject();
        go2.name = "EndPoint_Up";
        forDirectionPointsEnded.Add(go2);

        GameObject go3 = new GameObject();
        go3.name = "EndPoint_Down";
        forDirectionPointsEnded.Add(go3);

        GameObject go4 = new GameObject();
        go4.name = "EndPoint_Right";
        forDirectionPointsEnded.Add(go4);

        GameObject go5 = new GameObject();
        go5.name = "EndPoint_Left";
        forDirectionPointsEnded.Add(go5);
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
    private void AssignStartedGameObjectsPosition()
    {
        for (int i = 0; i < pivotPointsStarted.Count; i++)
        {
            switch (i)
            {
                case 0:
                    pivotPointsStarted[i] = Vector3.zero;
                    Vector3 startingPosition = targetPos;
                    startingPosition += this.transform.right * pivotPointsStarted[i].x;
                    startingPosition += this.transform.up * pivotPointsStarted[i].y;
                    startingPosition += this.transform.forward * pivotPointsStarted[i].z;
                    forDirectionPointsStarted[i].transform.position = startingPosition;
                    forDirectionPointsStarted[i].transform.LookAt(forDirectionPointsEnded[i].transform);
                    break;
                case 1:
                    pivotPointsStarted[i] = new Vector3(0, CalculateDetectionStartPoints(), 0); //Up
                    Vector3 startingPosition1 = targetPos;
                    startingPosition1 += this.transform.right * pivotPointsStarted[i].x;
                    startingPosition1 += this.transform.up * pivotPointsStarted[i].y;
                    startingPosition1 += this.transform.forward * pivotPointsStarted[i].z;
                    forDirectionPointsStarted[i].transform.position = startingPosition1;
                    forDirectionPointsStarted[i].transform.LookAt(forDirectionPointsEnded[i].transform);
                    break;
                case 2:
                    pivotPointsStarted[i] = new Vector3(0, -(CalculateDetectionStartPoints()), 0); //Down
                    Vector3 startingPosition2 = targetPos;
                    startingPosition2 += this.transform.right * pivotPointsStarted[i].x;
                    startingPosition2 += this.transform.up * pivotPointsStarted[i].y;
                    startingPosition2 += this.transform.forward * pivotPointsStarted[i].z;
                    forDirectionPointsStarted[i].transform.position = startingPosition2;
                    forDirectionPointsStarted[i].transform.LookAt(forDirectionPointsEnded[i].transform);

                    break;
                case 3:
                    pivotPointsStarted[i] = new Vector3(CalculateDetectionStartPoints(), 0, 0); //Right
                    Vector3 startingPosition3 = targetPos;
                    startingPosition3 += this.transform.right * pivotPointsStarted[i].x;
                    startingPosition3 += this.transform.up * pivotPointsStarted[i].y;
                    startingPosition3 += this.transform.forward * pivotPointsStarted[i].z;
                    forDirectionPointsStarted[i].transform.position = startingPosition3;
                    forDirectionPointsStarted[i].transform.LookAt(forDirectionPointsEnded[i].transform);

                    break;
                case 4:
                    pivotPointsStarted[i] = new Vector3(-(CalculateDetectionStartPoints()), 0, 0); //Left
                    Vector3 startingPosition4 = targetPos;
                    startingPosition4 += this.transform.right * pivotPointsStarted[i].x;
                    startingPosition4 += this.transform.up * pivotPointsStarted[i].y;
                    startingPosition4 += this.transform.forward * pivotPointsStarted[i].z;
                    forDirectionPointsStarted[i].transform.position = startingPosition4;
                    forDirectionPointsStarted[i].transform.LookAt(forDirectionPointsEnded[i].transform);

                    break;
            }
        }
    }
    private void AssignEndedGameObjectsPosition()
    {
        for (int i = 0; i < pivotPointsEnded.Count; i++)
        {
            switch (i)
            {
                case 0:
                    pivotPointsEnded[i] = Vector3.zero;
                    Vector3 startingPosition = this.transform.position;
                    startingPosition += this.transform.right * pivotPointsEnded[i].x;
                    startingPosition += this.transform.up * pivotPointsEnded[i].y;
                    startingPosition += this.transform.forward * pivotPointsEnded[i].z;
                    forDirectionPointsEnded[i].transform.position = startingPosition;
                    break;
                case 1:
                    pivotPointsEnded[i] = new Vector3(0, CalculateDetectionStartPoints(), 0); //Up
                    Vector3 startingPosition1 = this.transform.position;
                    startingPosition1 += this.transform.right * pivotPointsEnded[i].x;
                    startingPosition1 += this.transform.up * pivotPointsEnded[i].y;
                    startingPosition1 += this.transform.forward * pivotPointsEnded[i].z;
                    forDirectionPointsEnded[i].transform.position = startingPosition1;
                    break;
                case 2:
                    pivotPointsEnded[i] = new Vector3(0, -(CalculateDetectionStartPoints()), 0); //Down
                    Vector3 startingPosition2 = this.transform.position;
                    startingPosition2 += this.transform.right * pivotPointsEnded[i].x;
                    startingPosition2 += this.transform.up * pivotPointsEnded[i].y;
                    startingPosition2 += this.transform.forward * pivotPointsEnded[i].z;
                    forDirectionPointsEnded[i].transform.position = startingPosition2;
                    break;
                case 3:
                    pivotPointsEnded[i] = new Vector3(CalculateDetectionStartPoints(), 0, 0); //Right
                    Vector3 startingPosition3 = this.transform.position;
                    startingPosition3 += this.transform.right * pivotPointsEnded[i].x;
                    startingPosition3 += this.transform.up * pivotPointsEnded[i].y;
                    startingPosition3 += this.transform.forward * pivotPointsEnded[i].z;
                    forDirectionPointsEnded[i].transform.position = startingPosition3;
                    break;
                case 4:
                    pivotPointsEnded[i] = new Vector3(-(CalculateDetectionStartPoints()), 0, 0); //Left
                    Vector3 startingPosition4 = this.transform.position;
                    startingPosition4 += this.transform.right * pivotPointsEnded[i].x;
                    startingPosition4 += this.transform.up * pivotPointsEnded[i].y;
                    startingPosition4 += this.transform.forward * pivotPointsEnded[i].z;
                    forDirectionPointsEnded[i].transform.position = startingPosition4;
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
    private void ApplyCameraOffset()
    {
        targetPos = Target.position + Camera_offset;
    }
    private float LowestHitPoint(params float[] inputs)
    {
        float lowest = inputs[0];
        foreach (var input in inputs)
            if (input < lowest) lowest = input;
        return lowest;
    }
    private void CameraCollisionNew()
    {
        Vector3 backTowardsCamera = -this.transform.forward;
        for (int i = 0; i < pivotPointsEnded.Count; i++)
        {
            Ray ray = new Ray(forDirectionPointsStarted[i].transform.position, forDirectionPointsStarted[i].transform.forward);
            Debug.DrawLine(forDirectionPointsStarted[i].transform.position, forDirectionPointsEnded[i].transform.position, UnityEngine.Color.green);
            float distanceToCheck = Distance - forDirectionPointsEnded[i].transform.position.z;

            if (i != 0)
            {
                if (Physics.SphereCast(ray, SphereCastRadius, out raycastHit, distanceToCheck, ViewBlockingLayers))
                {
                    Debug.DrawLine(forDirectionPointsStarted[i].transform.position, forDirectionPointsEnded[i].transform.position, cameraRayColor);
                    if (i == 1)
                    {
                        hitPoints[1] = Vector3.Magnitude(raycastHit.point);
                        hitPointsPosition[1] = raycastHit.point;
                    }
                    else if (i == 2)
                    {
                        hitPoints[2] = Vector3.Magnitude(raycastHit.point);
                        hitPointsPosition[2] = raycastHit.point;
                    }
                    else if (i == 3)
                    {
                        hitPoints[3] = Vector3.Magnitude(raycastHit.point);
                        hitPointsPosition[3] = raycastHit.point;
                    }
                    else if (i == 4)
                    {
                        hitPoints[4] = Vector3.Magnitude(raycastHit.point);
                        hitPointsPosition[4] = raycastHit.point;
                    }

                    float distance = Vector3.Distance(targetPos, hitPointsPosition[Array.IndexOf(hitPoints, hitPoints.Min())]);
                    if (distance <= Distance)
                    {
                        transform.position = targetPos - (transform.rotation * Vector3.forward * distance);
                    }
                }
                else
                {
                    if (i == 1)
                    {
                        hitPoints[1] = Mathf.Infinity;
                    }
                    else if (i == 2)
                    {
                        hitPoints[2] = Mathf.Infinity;
                    }
                    else if (i == 3)
                    {
                        hitPoints[3] = Mathf.Infinity;
                    }
                    else if (i == 4)
                    {
                        hitPoints[4] = Mathf.Infinity;
                    }
                    // transform.position = targetPos - (transform.rotation * Vector3.forward * Distance);
                }
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
        DrawWireSphere(targetPos, CalculateOverallGizmoRadius(), UnityEngine.Color.red);

        Vector3 startingPosition_Center = this.transform.position;
        startingPosition_Center += this.transform.right * pivotPointsEnded[0].x;
        startingPosition_Center += this.transform.up * pivotPointsEnded[0].y;
        startingPosition_Center += this.transform.forward * pivotPointsEnded[0].z;
        DrawWireSphere(startingPosition_Center, SphereCastRadius, UnityEngine.Color.green);

        Vector3 startingPosition_Center_Started = targetPos;
        startingPosition_Center_Started += this.transform.right * pivotPointsStarted[0].x;
        startingPosition_Center_Started += this.transform.up * pivotPointsStarted[0].y;
        startingPosition_Center_Started += this.transform.forward * pivotPointsStarted[0].z;
        DrawWireSphere(startingPosition_Center_Started, SphereCastRadius, UnityEngine.Color.green);

        Vector3 startingPosition_Up = this.transform.position;
        startingPosition_Up += this.transform.right * pivotPointsEnded[1].x;
        startingPosition_Up += this.transform.up * pivotPointsEnded[1].y;
        startingPosition_Up += this.transform.forward * pivotPointsEnded[1].z;
        DrawWireSphere(startingPosition_Up, SphereCastRadius, UnityEngine.Color.green);

        Vector3 startingPosition_Up_Started = targetPos;
        startingPosition_Up_Started += this.transform.right * pivotPointsStarted[1].x;
        startingPosition_Up_Started += this.transform.up * pivotPointsStarted[1].y;
        startingPosition_Up_Started += this.transform.forward * pivotPointsStarted[1].z;
        DrawWireSphere(startingPosition_Up_Started, SphereCastRadius, UnityEngine.Color.green);

        Vector3 startingPosition_Down = this.transform.position;
        startingPosition_Down += this.transform.right * pivotPointsEnded[2].x;
        startingPosition_Down += this.transform.up * pivotPointsEnded[2].y;
        startingPosition_Down += this.transform.forward * pivotPointsEnded[2].z;
        DrawWireSphere(startingPosition_Down, (SphereCastRadius), UnityEngine.Color.green);

        Vector3 startingPosition_Down_Started = targetPos;
        startingPosition_Down_Started += this.transform.right * pivotPointsStarted[2].x;
        startingPosition_Down_Started += this.transform.up * pivotPointsStarted[2].y;
        startingPosition_Down_Started += this.transform.forward * pivotPointsStarted[2].z;
        DrawWireSphere(startingPosition_Down_Started, (SphereCastRadius), UnityEngine.Color.green);

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
    void Update()
    {
        if (Target != null)
        {
            SphereCastRadius = Mathf.Clamp(SphereCastRadius, 0.23f, 0.33f); //Optimal for Field of View 60
            ApplyCameraOffset();
            CalculatePivotPoint();
            CalculatePivotPointStarted();
            AssignEndedGameObjectsPosition();
            AssignStartedGameObjectsPosition();

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
}
