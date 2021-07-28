using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public Vector3 velocity = Vector3.zero;
    public BoidManager manager;
    public Color initColour;
    public Color currColour;
    public float hunger = 0;
    public float hungerPerSecond = 0.01f;
    public Transform cameraPos;

    public float groupingWeight;
    public float avoidanceWeight;
    public float velMatchWeight;
    public float boundPosWeight;
    public float avoidObjWeight;
    public float findFoodWeight;
    public float lifeSpan;



    public FoodManager foodManager;
    public SkinnedMeshRenderer meshRenderer;


    private void Start()
    {
        initColour = Random.ColorHSV();
        currColour = initColour;
        lifeSpan = 0;
    }
    private void Update()
    {
        lifeSpan += Time.deltaTime;

        if (currColour != meshRenderer.material.color)
        {
            meshRenderer.material.color = currColour;
        }
        hunger = Mathf.Clamp(hunger + Time.deltaTime*hungerPerSecond, 0, 1);

        if (hunger == 1)
        {
            manager.RemoveFromList(gameObject);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.tag == "Food")
        {
            foodManager.foodList.Remove(collider.gameObject.gameObject);
            GameObject.Destroy(collider.gameObject.gameObject);
            manager.CreateNewDragon(gameObject);
            hunger = 0;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag != "Dragon")
        {
            hunger += 0.1f;
            //manager.RemoveFromList(gameObject);
            //Destroy(gameObject);
        }
        
    }

}
