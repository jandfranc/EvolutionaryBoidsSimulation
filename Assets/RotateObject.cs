using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float oscillationMul;
    public float oscillationPeriod;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * -1 * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, Mathf.Sin(Time.time*oscillationPeriod)*oscillationMul + 150, transform.position.z);
    }
}
