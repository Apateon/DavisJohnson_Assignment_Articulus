using UnityEngine;

public class Rotation : MonoBehaviour
{
    float speed = 50f;
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, speed * Time.deltaTime, 0);
    }
}
