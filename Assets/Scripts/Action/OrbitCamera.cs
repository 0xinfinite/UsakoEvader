﻿using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField]
    Transform focus = default;
    [SerializeField, Range(1f, 20f)]
    float distance = 5f;
    [SerializeField, Min(0f)]
    float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)]
    float focusCentering = 0.5f;
    [SerializeField, Range(1f, 360f)]
    float rotationSpeed = 90f;
    [SerializeField, Range(-89f, 89f)]
    float minVerticalAngle = -30f, maxVerticalAngle = 60f;
    [SerializeField, Min(0f)]
    float alignDelay = 5f;
    [SerializeField, Range(0f, 90f)]
    float alignSmoothRange = 45f;
    [SerializeField]
    LayerMask obstructionMask = -1;
    [SerializeField]
    LayerMask heightObstructionMask = -1;

    Vector3 focusPoint, previousFocusPoint;
    public Vector2 orbitAngles = new Vector2(45f, 0f);
    [SerializeField]
    Vector3 focusOffset;
    float lastManualRotationTime;
    Camera regularCamera;
    public Camera RegularCamera { get { return regularCamera; } }

    void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
    }

    void Awake()
    {
        regularCamera = GetComponent<Camera>();
        focusPoint = focus.position;
        transform.localRotation = Quaternion.Euler(orbitAngles);

       // origHeight = focus.position.y;
    }

    void LateUpdate()
    {
        UpdateFocusPoint();
        Quaternion lookRotation;
        /* if (ManualRotation() || AutomaticRotation())
         {
             ConstrainAngles();
             lookRotation = Quaternion.Euler(orbitAngles);
         }
         else
         {
             lookRotation = transform.localRotation;
         }*/
        lookRotation = transform.rotation;
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;

        Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        if (Physics.BoxCast(
            castFrom, CameraHalfExtends, castDirection, out RaycastHit hit, lookRotation,
               castDistance/*distance - regularCamera.nearClipPlane*/, obstructionMask)
        )
        {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }

        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }
    //float origHeight;
    float prevHeight;
    void UpdateFocusPoint()
    {
        previousFocusPoint = focusPoint;

        RaycastHit hit;

        float height = Physics.Raycast(focus.position, Vector3.down, out hit, 100f, heightObstructionMask) ? hit.point.y : focus.position.y;

        Vector3 targetPoint = /*focus.position+*/focus.TransformPoint(focusOffset);// -new Vector3(0, focus.position.y,0));
        targetPoint.y = Mathf.Lerp(prevHeight, Mathf.Max( height+focusOffset.y,1f),0.1f);
        prevHeight = targetPoint.y;
        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            if (distance > focusRadius)
            {
                focusPoint = Vector3.Lerp(
                    targetPoint, focusPoint, focusRadius / distance
                );
            }
            if (distance > 0.01f && focusCentering > 0f)
            {
                focusPoint = Vector3.Lerp(
                    targetPoint, focusPoint,
                    Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime)
                );
            }
        }
        else
        {
            focusPoint = targetPoint;
        }


    }

    Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y =
                regularCamera.nearClipPlane *
                Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    bool ManualRotation()
    {
        Vector2 input = new Vector2(
            Input.GetAxis("Vertical Camera"),
            Input.GetAxis("Horizontal Camera")
        );
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            lastManualRotationTime = Time.unscaledTime;
            return true;
        }
        return false;
    }
    void ConstrainAngles()
    {
        orbitAngles.x =
            Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        }
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }

    bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
        {
            return false;
        }
        Vector2 movement = new Vector2(
           focusPoint.x - previousFocusPoint.x,
           focusPoint.z - previousFocusPoint.z
       );
        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.000001f)
        {
            return false;
        }
        float headingAngle = GetAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);
        if (deltaAbs < alignSmoothRange)
        {
            rotationChange *= deltaAbs / alignSmoothRange;
        }
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }
        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
        return true;
    }

    static float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Phase2"))
        transform.parent.GetComponent<Animator>().Play("CameraRotatingToPhase2", 1);
    }
}
