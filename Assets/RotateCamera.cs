using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{

    public Transform cameraPos;

    public float rotationSpeed;
    public float positionSpeed;

    private bool followingDragon = false;

    private GameObject followedDragon;
    private Transform dragonCameraPos;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Application.LoadLevel(Application.loadedLevel);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
       
        
        if (followingDragon)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                followingDragon = false;
                followedDragon.GetComponentInChildren<BoxCollider>().enabled = true;
            }

            Vector3.Slerp(Camera.main.transform.position, dragonCameraPos.position, positionSpeed * Time.deltaTime);

            Camera.main.transform.position = dragonCameraPos.position;
            Camera.main.transform.rotation = Quaternion.Slerp(Camera.main.transform.rotation, dragonCameraPos.rotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.Rotate(Vector3.up * 5 * Time.deltaTime);
            Camera.main.transform.position = cameraPos.transform.position;
            Camera.main.transform.rotation = cameraPos.transform.rotation;
        }
        

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray.origin, ray.direction, out hit);
            if (hit.collider != null && hit.collider.gameObject.tag == "Dragon")
            {
                print(hit.transform.name);
                followedDragon = hit.transform.gameObject;

                dragonCameraPos = followedDragon.gameObject.GetComponent<Movement>().cameraPos.transform;

                followedDragon.GetComponentInChildren<BoxCollider>().enabled = false;

                Camera.main.transform.position = dragonCameraPos.position;
                Camera.main.transform.rotation = dragonCameraPos.rotation;
                followingDragon = true;
            }
        }
    }
}
