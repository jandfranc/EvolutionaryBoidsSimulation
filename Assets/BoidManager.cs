using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public GameObject boidObject;
    public Transform dragonParent;

    public int currentDragons;

    public int totalBoids;
    private List<GameObject> allBoids;
    public Vector2 xSpawnRange;
    public Vector2 ySpawnRange;
    public Vector2 zSpawnRange;
    public float avoidanceDistance;
    public float groupingWeight;
    public float avoidanceWeight;
    public float velMatchWeight;
    public float boundPosWeight;
    public float avoidObjWeight;
    public float findFoodWeight;
    public float senseRange;
    public float maxVel;
    public float minVel;
    public float forwardWeight;
    public float wallAvoid;
    public Vector2 xRange;
    public Vector2 yRange;
    public Vector2 zRange;
    public float objectAvoidanceMin;
    public bool similarColours;
    public float colourChangeTime;
    public FoodManager foodManager;

    public float groupEvoWeight = 0.5f;
    public float avoidEvoWeight = 0.5f;
    public float velMatchEvoWeight = 0.5f;
    public float avoidObjEvoWeight = 0.5f;
    public float findFoodEvoWeight = 0.5f;


    public StoreData storeData;

    public LayerMask mask;


    // Start is called before the first frame update
    void Start()
    {
        allBoids = new List<GameObject>();
        InitialiseBoids();
        currentDragons = totalBoids;
        StartCoroutine(RecordCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        currentDragons = allBoids.Count;
        MoveBoids();

    }

    IEnumerator RecordCoroutine()
    {
        for (; ; )
        {
            
            RecordData();
            yield return new WaitForSeconds(1f);
        }
    }

    void RecordData()
    {
        float group = 0;
        float avoid = 0;
        float match = 0;
        float avoidObj = 0;
        float food = 0;
        float lifeSpan = 0;

        foreach (GameObject boid in allBoids)
        {
            if (boid == null) { continue; }

            Movement movement = boid.GetComponent<Movement>();

            group += movement.groupingWeight;
            avoid += movement.avoidanceWeight;
            match += movement.velMatchWeight;
            avoidObj += movement.avoidObjWeight;
            food +=  movement.findFoodWeight;
            lifeSpan += movement.lifeSpan;
        }

        group /= allBoids.Count;
        avoid /= allBoids.Count;
        match /= allBoids.Count;
        avoidObj /= allBoids.Count;
        food /= allBoids.Count;
        lifeSpan /= allBoids.Count;

        storeData.Write(group,avoid,match,avoidObj,food, currentDragons, lifeSpan);
    }

    void InitialiseBoids()
    {
        for (int i = 0; i < totalBoids; i++)
        {
            Vector3 position = new Vector3(Random.Range(xSpawnRange[0], xSpawnRange[1]), Random.Range(ySpawnRange[0], ySpawnRange[1]), Random.Range(zSpawnRange[0], zSpawnRange[1]));

            GameObject dragon = GameObject.Instantiate(boidObject, position, Quaternion.identity, dragonParent);

            allBoids.Add(dragon);
            dragon.transform.Rotate(0, Random.Range(0, 360), 0);
            Movement movement = dragon.GetComponent<Movement>();
            movement.velocity = dragon.transform.forward * maxVel / 2;
            movement.manager = this;



            movement.groupingWeight = Mathf.Max(0.01f, groupingWeight + Random.Range(-groupingWeight,groupingWeight)*groupEvoWeight);
            movement.avoidanceWeight = Mathf.Max(0.01f, avoidanceWeight + Random.Range(-avoidanceWeight, avoidanceWeight) * avoidEvoWeight);
            movement.velMatchWeight = Mathf.Max(0.01f, velMatchWeight + Random.Range(-velMatchWeight, velMatchWeight) * velMatchEvoWeight);
            movement.avoidObjWeight = Mathf.Max(0.01f, avoidObjWeight + Random.Range(-avoidObjWeight, avoidObjWeight) * avoidObjEvoWeight);
            movement.findFoodWeight = Mathf.Max(0.01f, findFoodWeight + Random.Range(-findFoodWeight, findFoodWeight) * findFoodEvoWeight);
            movement.foodManager = foodManager;

            movement.boundPosWeight = boundPosWeight;

        }
    }

    void MoveBoids()
    {
        foreach (GameObject boid in allBoids)
        {
            if (boid == null) { continue; }

            Movement movement = boid.GetComponent<Movement>();

            Vector3 v1 = GroupBoids(boid) * movement.groupingWeight;
            Vector3 v2 = AvoidBoids(boid) * movement.avoidanceWeight;
            Vector3 v3 = MatchVelocity(boid) * movement.velMatchWeight;
            Vector3 v4 = BoundPosition(boid) * movement.boundPosWeight;
            Vector3 v5 = AvoidObjects(boid) * movement.avoidObjWeight;
            Vector3 v6 = FindFood(boid) * boid.GetComponent<Movement>().hunger* boid.GetComponent<Movement>().hunger * movement.findFoodWeight;
            movement.velocity += (v1 + v2 + v3 + v4 + v5 + v6) * Time.deltaTime;
            //print("v1 "+v1 + " v2 "+ v2+" v3 " + v3 + " v4 " + v4);

            LimitVelocity(boid);
            if (boid.GetComponent<Movement>().velocity != Vector3.zero)
            {
                boid.transform.rotation = Quaternion.LookRotation(movement.velocity, Vector3.up);
            }

            boid.transform.position += movement.velocity;

            SetBoidColour(boid);
        }
    }

    Vector3 FindFood(GameObject boid)
    {
        GameObject closestFood= null;
        float currClosest = Mathf.Infinity;

        Vector3 findFoodVector = Vector3.zero;

        foreach (GameObject food in foodManager.foodList)
        {
            if (food != null)
            {
                float foodDistance = Vector3.Magnitude(boid.transform.position - food.transform.position);
                if (foodDistance < currClosest)
                {
                    closestFood = food;
                    currClosest = foodDistance;
                }
            }
        }

        if (closestFood != null)
        {
            findFoodVector = (closestFood.transform.position - boid.transform.position) / 100;
        }

        

        return findFoodVector;
    }

    Vector3 GroupBoids(GameObject boid)
    {
        Vector3 boidCenter = Vector3.zero;
        int nearbyBoids = 0;
        foreach (GameObject comparisonBoid in allBoids)
        {
            if (boid == null) { continue; }
            if (boid != comparisonBoid && Vector3.Magnitude(boid.transform.position - comparisonBoid.transform.position) < senseRange)
            {
                boidCenter = boidCenter + comparisonBoid.transform.position;
                nearbyBoids += 1;
            }
        }
        if (nearbyBoids > 0)
        {
            boidCenter /= nearbyBoids;

        }
        if (boidCenter != Vector3.zero)
        {
            return (boidCenter - boid.transform.position) / 100;
        }
        return Vector3.zero;

    }

    Vector3 AvoidBoids(GameObject boid)
    {
        Vector3 avoidanceVector = Vector3.zero;
        foreach (GameObject comparisonBoid in allBoids)
        {
            if (boid == null) { continue; }
            if (boid != comparisonBoid && Vector3.Magnitude(boid.transform.position - comparisonBoid.transform.position) < senseRange)
            {
                if (Vector3.Magnitude(boid.transform.position - comparisonBoid.transform.position) < avoidanceDistance)
                {

                    avoidanceVector -= (comparisonBoid.transform.position - boid.transform.position);
                }
            }
        }
        return avoidanceVector;
    }

    Vector3 MatchVelocity(GameObject boid)
    {
        if (allBoids.Count <= 1)
        {
            return Vector3.zero;
        }
        Vector3 matchVector = Vector3.zero;
        Vector3 boidVel = boid.GetComponent<Movement>().velocity;
        foreach (GameObject comparisonBoid in allBoids)
        {
            if (boid == null) { continue; }

            if (boid != comparisonBoid && Vector3.Magnitude(boid.transform.position - comparisonBoid.transform.position) < senseRange)
            {
                if (Vector3.Magnitude(boid.transform.position - comparisonBoid.transform.position) < avoidanceDistance)
                {
                    matchVector = matchVector + boidVel;
                }
            }
            matchVector /= allBoids.Count - 1;
        }
        return (matchVector - boidVel) / 8;
    }

    void LimitVelocity(GameObject boid)
    {
        Vector3 vel = boid.GetComponent<Movement>().velocity;
        float velMagnitude = Vector3.Magnitude(vel);
        if (velMagnitude > maxVel)
        {
            boid.GetComponent<Movement>().velocity = (vel / velMagnitude) * maxVel;
        }
        else if (velMagnitude < minVel)
        {
            boid.GetComponent<Movement>().velocity = (vel / velMagnitude) * minVel;
        }
    }

    Vector3 BoundPosition(GameObject boid)
    {
        Vector3 boundVel = Vector3.zero;
        if (boid.transform.position.x < xRange[0] + objectAvoidanceMin)
        {
            boundVel.x = wallAvoid * GetWallPercent(xRange[0], boid.transform.position.x);
        }
        else if (boid.transform.position.x > xRange[1] - objectAvoidanceMin)
        {
            boundVel.x = -wallAvoid * GetWallPercent(xRange[1], boid.transform.position.x);
        }

        if (boid.transform.position.y < yRange[0] + objectAvoidanceMin)
        {
            boundVel.y = wallAvoid * GetWallPercent(yRange[0], boid.transform.position.y);
        }
        else if (boid.transform.position.y > yRange[1] - objectAvoidanceMin)
        {
            boundVel.y = -wallAvoid * GetWallPercent(yRange[1], boid.transform.position.y);
        }

        if (boid.transform.position.z < zRange[0] + objectAvoidanceMin)
        {
            boundVel.z = wallAvoid * GetWallPercent(zRange[0], boid.transform.position.z);
        }
        else if (boid.transform.position.z > zRange[1] - objectAvoidanceMin)
        {
            boundVel.z = -wallAvoid * GetWallPercent(zRange[1], boid.transform.position.z);
        }
        //print(boundVel);
        return boundVel;
    }


    
    Vector3 AvoidObjects(GameObject boid)
    {
        Vector3 avoidanceVector = Vector3.zero;

        RaycastHit[] hits;

        hits = Physics.SphereCastAll(boid.transform.position, 20, boid.transform.forward, 30, ~mask);

        foreach (RaycastHit hit in hits)
        {
            avoidanceVector -= (hit.transform.position - boid.transform.position);
        }

        return avoidanceVector;
    }

    float GetWallPercent(float range, float position)
    {
        float percent = Mathf.Abs((range - position) / objectAvoidanceMin);
        //print(percent);
        return percent;
    }

    void SetBoidColour(GameObject boid)
    {
        if (similarColours)
        {
            int counter = 1;
            Color outputColour = boid.GetComponent<Movement>().initColour;
            foreach (GameObject comparisonBoid in allBoids)
            {
                if (boid == null) { continue; }
                if (boid != comparisonBoid && Vector3.Magnitude(boid.transform.position - comparisonBoid.transform.position) < senseRange)
                {
                    outputColour += comparisonBoid.GetComponent<Movement>().initColour;
                    counter += 1;
                }
            }
            boid.GetComponent<Movement>().currColour = Color.Lerp(boid.GetComponent<Movement>().currColour, outputColour/counter, Time.deltaTime * colourChangeTime);
        }
        else
        {
            Color outputColour = boid.GetComponent<Movement>().initColour;
            foreach (GameObject comparisonBoid in allBoids)
            {
                if (Vector3.Magnitude(boid.transform.position - comparisonBoid.transform.position) < senseRange)
                {
                    outputColour = comparisonBoid.GetComponent<Movement>().initColour;
                }
            }


            boid.GetComponent<Movement>().currColour = outputColour;
        }
    }

    public void RemoveFromList(GameObject boid)
    {
        allBoids.Remove(boid);
    }

    public void CreateNewDragon(GameObject boid)
    {
        Vector3 position = new Vector3(Random.Range(xSpawnRange[0], xSpawnRange[1]), Random.Range(ySpawnRange[0], ySpawnRange[1]), Random.Range(zSpawnRange[0], zSpawnRange[1]));
        GameObject dragon = GameObject.Instantiate(boidObject, position, Quaternion.identity, dragonParent);
        allBoids.Add(dragon);
        dragon.transform.Rotate(0, Random.Range(0, 360), 0);

        Movement movement = dragon.GetComponent<Movement>();
        Movement movementOld = boid.GetComponent<Movement>();

        movement.velocity = dragon.transform.forward * maxVel / 2;
        movement.manager = this;
        movement.initColour = boid.GetComponent<Movement>().initColour;

        movement.boundPosWeight = movementOld.boundPosWeight;

        movement.groupingWeight = Mathf.Max(0.01f, movementOld.groupingWeight + Random.Range(-groupingWeight, groupingWeight) * groupEvoWeight);
        movement.avoidanceWeight = Mathf.Max(0.01f, movementOld.avoidanceWeight + Random.Range(-avoidanceWeight, avoidanceWeight) * avoidEvoWeight);
        movement.velMatchWeight = Mathf.Max(0.01f, movementOld.velMatchWeight + Random.Range(-velMatchWeight, velMatchWeight) * velMatchEvoWeight);
        movement.avoidObjWeight = Mathf.Max(0.01f, movementOld.avoidObjWeight + Random.Range(-avoidObjWeight, avoidObjWeight) * avoidObjEvoWeight);
        movement.findFoodWeight = Mathf.Max(0.01f, movementOld.findFoodWeight + Random.Range(-findFoodWeight, findFoodWeight) * findFoodEvoWeight);

        movement.foodManager = foodManager;

    }


}
