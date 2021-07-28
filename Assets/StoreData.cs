using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StoreData : MonoBehaviour
{
    
    public string fileName = "test.csv";
    StreamWriter writer;



    public void Write(float group, float avoid, float match, float avoidObj, float food, int totalBoids, float lifeSpan) 
    {
        if (writer == null)
        {
            writer = new StreamWriter(fileName);
            writer.WriteLine("group,avoid,match,avoidObj,food,totalBoids");
        }
        writer.WriteLine(group + "," + avoid + "," + match + "," + avoidObj + "," + food + "," + totalBoids + "," + lifeSpan);
    }

    public void OnApplicationQuit()
    {
        writer.Close();
    }
}
