using UnityEngine;

public class PlatformRotation : MonoBehaviour
{
    public float fRotationSpeed = 100f;
    public Transform transRotationPoint;




    void Update()
    {
        transform.Rotate(Vector3.forward, -fRotationSpeed * Time.deltaTime);
        transform.RotateAround(transRotationPoint.position, Vector3.forward, fRotationSpeed * Time.deltaTime);
    }

}
