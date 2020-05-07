using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtendedMathmatics
{
    public static float Map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    public static Vector3 PredictPositionLinearly(Vector3 currentPosition, Vector3 velocity, float time)
    {
        return currentPosition + velocity * 0.5f * time;
    }

    public static Vector3 PredictAeroPosition(Rigidbody body, Vector3 currentGravity, float time)
    {
        Vector3 velocity = body.velocity;
        Vector3 position = body.transform.position;

        int i = 0;
        for (/*int*/ i = 0; i < (int)(time * (1 / Time.fixedDeltaTime)); ++i)
        {

            velocity *= (1.0f - Time.fixedDeltaTime * body.drag); // drag
            velocity += currentGravity * Time.fixedDeltaTime; // gravity
            position += velocity * Time.fixedDeltaTime;// move point

        }
        return position;
    }

    public static Vector3 PredictPositionSimpleGroundCheck(Rigidbody body, Vector3 currentGravity, float time, LayerMask groundCheck, CapsuleCollider capsule, out Vector3 maxHeight, out Vector3 caculLow)
    {
        Vector3 velocity = body.velocity;
        Vector3 position = body.transform.position;
        Vector3 firstPos = position;
        bool reachHigh = false;
        bool isUp = body.velocity.y > 0 ? true : false;
        maxHeight = position;
        caculLow = position;
        float lowHeight = position.y;
        int i = 0;
        for (/*int*/ i = 0; i < (int)(time * (1 / Time.fixedDeltaTime)); ++i)
        {
            velocity *= (1.0f - Time.fixedDeltaTime * body.drag); // drag
            velocity += currentGravity * Time.fixedDeltaTime; // gravity
            position += velocity * Time.fixedDeltaTime;// move point

            if (velocity.y < 0)
            {
                if (!reachHigh)
                {
                    maxHeight = position;
                    reachHigh = true;
                    Vector3 p1 = position + capsule.center + Vector3.up * (capsule.height * 0.5f - capsule.radius);
                    Vector3 p2 = position + capsule.center - Vector3.up * (capsule.height * 0.5f - capsule.radius);
                    Vector3 dir = isUp ? Vector3.Reflect((position - firstPos).normalized, Vector3.down) : velocity.normalized;
                    RaycastHit hit;
                    if (Physics.CapsuleCast(p1, p2, capsule.radius, dir, out hit, 1000f, groundCheck))
                    {
                        if (hit.normal.y < 0.9)
                        { return hit.point; }

                        lowHeight = hit.point.y + capsule.height * 0.5f * body.transform.localScale.y;
                        caculLow = hit.point;
                    }
                }
                position.y = position.y < lowHeight ? lowHeight : position.y;
            }

        }
        //  Debug.Log(currentGravity);
        return position;
    }

    public static Vector3 PredictPositionSimpleGroundCheck(Rigidbody body, Vector3 currentGravity, float time, LayerMask groundCheck, CapsuleCollider capsule)
    {
        Vector3 velocity = body.velocity;
        Vector3 position = body.transform.position;
        Vector3 firstPos = position;
        bool reachHigh = false;
        bool isUp = body.velocity.y > 0 ? true : false;
        float lowHeight = position.y;
        int i = 0;
        for (/*int*/ i = 0; i < (int)(time * (1 / Time.fixedDeltaTime)); ++i)
        {
            velocity *= (1.0f - Time.fixedDeltaTime * body.drag); // drag
            velocity += currentGravity * Time.fixedDeltaTime; // gravity
            position += velocity * Time.fixedDeltaTime;// move point

            if (velocity.y < 0)
            {
                if (!reachHigh)
                {
                    reachHigh = true;
                    Vector3 p1 = position + capsule.center + Vector3.up * (capsule.height * 0.5f - capsule.radius);
                    Vector3 p2 = position + capsule.center - Vector3.up * (capsule.height * 0.5f - capsule.radius);
                    Vector3 dir = isUp ? Vector3.Reflect((position - firstPos).normalized, Vector3.down) : velocity.normalized;
                    RaycastHit hit;
                    if (Physics.CapsuleCast(p1, p2, capsule.radius, dir, out hit, 1000f, groundCheck))
                    {
                        if (hit.normal.y < 0.9)
                        { return hit.point; }

                        lowHeight = hit.point.y + capsule.height * 0.5f * body.transform.localScale.y;
                    }
                }
                position.y = position.y < lowHeight ? lowHeight : position.y;
            }

        }
        return position;
    }

    public static Vector3 PredictPositionSimpleGroundCheck(Vector3 position, Vector3 velocity, Vector3 currentGravity, float time, LayerMask groundCheck, CapsuleCollider capsule, float targetLocalScaleY = 1, float drag = 0)
    {
        Vector3 firstPos = position;
        bool reachHigh = false;
        bool isUp = velocity.y > 0 ? true : false;
        float lowHeight = position.y;
        int i = 0;
        for (/*int*/ i = 0; i < (int)(time * (1 / Time.fixedDeltaTime)); ++i)
        {
            velocity *= (1.0f - Time.fixedDeltaTime * drag); // drag
            velocity += currentGravity * Time.fixedDeltaTime; // gravity
            position += velocity * Time.fixedDeltaTime;// move point

            if (velocity.y < 0)
            {
                if (!reachHigh)
                {
                    reachHigh = true;
                    Vector3 p1 = position + capsule.center + Vector3.up * (capsule.height * 0.5f - capsule.radius);
                    Vector3 p2 = position + capsule.center - Vector3.up * (capsule.height * 0.5f - capsule.radius);
                    Vector3 dir = isUp ? Vector3.Reflect((position - firstPos).normalized, Vector3.down) : velocity.normalized;
                    RaycastHit hit;
                    if (Physics.CapsuleCast(p1, p2, capsule.radius, dir, out hit, 1000f, groundCheck))
                    {
                        if (hit.normal.y < 0.9)
                        { return hit.point; }

                        lowHeight = hit.point.y + capsule.height * 0.5f * targetLocalScaleY;
                    }
                }
                position.y = position.y < lowHeight ? lowHeight : position.y;
            }

        }
        //  Debug.Log(currentGravity);
        return position;
    }

    public static int AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir > 0.0)
            return 1;
        else if (dir < 0.0)
            return -1;
        else
            return 0;
    }

    public static Vector3 XYZAngleForwardToTargetPosition(Vector3 forward, Vector3 targetPosition, Vector3 currentPosition)
    {
        Vector3 angleToTarget = Vector3.zero;
        Vector3 targetDir = (targetPosition - currentPosition).normalized;
        angleToTarget.y = Vector3.Angle(
          new Vector3(targetDir.x, 0, targetDir.z), new Vector3(forward.x, 0, forward.z).normalized)
                * AngleDir(forward, targetDir, Vector3.up);

        Vector3 horizontalTurnedDirection = Quaternion.AngleAxis(angleToTarget.y, Vector3.up) * forward;
        angleToTarget.x = Vector3.Angle(horizontalTurnedDirection, targetDir)
            * AngleDir(horizontalTurnedDirection.normalized, targetDir, Quaternion.AngleAxis(90, Vector3.up) * new Vector3(horizontalTurnedDirection.x, 0, horizontalTurnedDirection.z).normalized);

        return angleToTarget;
    }

    public static Vector3 XYZAngleBetweenTwoDirection(Vector3 forward, Vector3 targetDir)
    {
        Vector3 angleToTarget = Vector3.zero;

        if (targetDir.magnitude != 1) targetDir.Normalize();

        angleToTarget.y = Vector3.Angle(
          new Vector3(targetDir.x, 0, targetDir.z), new Vector3(forward.x, 0, forward.z).normalized)
                * AngleDir(forward, targetDir, Vector3.up);

        Vector3 horizontalTurnedDirection = Quaternion.AngleAxis(angleToTarget.y, Vector3.up) * forward;
        angleToTarget.x = Vector3.Angle(horizontalTurnedDirection, targetDir)
            * AngleDir(horizontalTurnedDirection.normalized, targetDir, Quaternion.AngleAxis(90, Vector3.up) * new Vector3(horizontalTurnedDirection.x, 0, horizontalTurnedDirection.z).normalized);

        return angleToTarget;
    }

    public static Vector3 AnglesToTargetPosition(Vector3 targetPos, Vector3 currentPos)
    {
        return XYZAngleForwardToTargetPosition(Vector3.forward, targetPos, currentPos);
    }

    public static Vector3 ConstrainAngles(Vector3 orbitAngles, float minVerticalAngle, float maxVerticalAngle)
    {
        Vector3 result = new Vector3(orbitAngles.x, orbitAngles.y, orbitAngles.z);
        result.x =
            Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        result.y = orbitAngles.y;
        if (result.y < 0f)
        {
            result.y += 360f;
        }
        else if (result.y >= 360f)
        {
            result.y -= 360f;
        }
        return result;
    }

    public static Vector3 ProjectionOnLine(Vector3 targetPos, Vector3 startLinePos, Vector3 lineDirection)
    {
        return startLinePos + Vector3.Project(targetPos - startLinePos, lineDirection);
    }

    public static float AngleOfLaunch(float x, float y, float initVelocity)
    {
        float g = -Physics.gravity.y;//9.8f;
        float v = initVelocity*0.85f;
        float a = 45 * Mathf.Deg2Rad;

        float vSin = v * Mathf.Cos(a);
        float vCos = v * Mathf.Sin(a);
        float sqrt = Mathf.Sqrt(vSin * vSin + 2 * g * y);
        float maxRange = Mathf.Abs((vSin / g) * (vCos + sqrt));

        

        // formula courtesy wikipedia:

        float v2 = v * v;
        float v4 = v2 * v2;
        float x2 = x * x;

        sqrt = v4 - g * (g * x2 + 2 * y * v2);

        if(sqrt < maxRange)
        {
            return -45f;
        }

        float theta = Mathf.Atan2(v2 - Mathf.Sqrt(sqrt), g * x);

        // adjust aim angle upwards by theta:

        return -theta * Mathf.Rad2Deg;
    }
}
