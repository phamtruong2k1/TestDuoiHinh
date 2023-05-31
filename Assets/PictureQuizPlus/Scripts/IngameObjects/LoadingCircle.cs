using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCircle : MonoBehaviour
{
    public Transform circle;
    private int counter;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        counter++;
        if (counter % 5 == 0 && circle)
        {
            circle.Rotate(0, 0, -36f, Space.World);
        }
    }
}
