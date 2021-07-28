using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour
{
    public GameObject foodObject;

    public float spawnChance;

    public List<GameObject> foodList;

    private Bounds bounds;

    public Transform foodParent;

    // Update is called once per frame
    void Update()
    {
        bounds = gameObject.GetComponent<BoxCollider>().bounds;

        if (foodList.Count < 100 && Random.Range(0f,1f) <= spawnChance)
        {
            float offsetX = Random.Range(-bounds.extents.x + bounds.center.x, bounds.extents.x + bounds.center.x);
            float offsetY = Random.Range(-bounds.extents.y + bounds.center.y, bounds.extents.y + bounds.center.y);
            float offsetZ = Random.Range(-bounds.extents.z + bounds.center.z, bounds.extents.z + bounds.center.z);
            foodList.Add(GameObject.Instantiate(foodObject, new Vector3(offsetX, offsetY, offsetZ), Quaternion.identity, foodParent));
        }
    }
}
