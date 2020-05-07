using UnityEngine;

public static class CustomGravity
{

    public static Vector3 GetGravity(Vector3 position)
    {
        return Physics.gravity;
        //return position.normalized * Physics.gravity.y;//Physics.gravity;

    }

    public static Vector3 GetUpAxis(Vector3 position)
    {
        return -Physics.gravity.normalized;

        //Vector3 up = position.normalized;
        //return Physics.gravity.y < 0f ? up : -up;
    }

    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        upAxis = -Physics.gravity.normalized;
                  return Physics.gravity;
        //position.normalized;
        //return upAxis * Physics.gravity.y;

        //Vector3 up = position.normalized;
        //upAxis = Physics.gravity.y < 0f ? up : -up;
        //return up * Physics.gravity.y;
    }

}