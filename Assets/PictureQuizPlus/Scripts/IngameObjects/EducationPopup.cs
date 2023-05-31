using UnityEngine;


public class EducationPopup : MonoBehaviour //This script is attached to the education arrow and serves only 
                                            //for destroying education stuff if any key is pressed
{
    float timer, timeToWait;
    bool timerSet;
    public GameObject textField;

    void Update()
    {
        if (timerSet && timer < timeToWait)
        {
            timer += Time.deltaTime;
        }
        else
        {
            timerSet = false;
        }
        if (Input.anyKey && !timerSet)
        {
            Destroy(textField);
            Destroy(gameObject);
            Education.StartTimer();
        }
    }

    private void Awake()
    {
        timeToWait = 1;
        timerSet = true;
    }
}
