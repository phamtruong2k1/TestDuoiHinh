using System.Collections;
using UnityEngine;

public class HuskHandler : MonoBehaviour //Component class for husks that are made by the spatula
{
    private void OnEnable()
    {
        ParticleSystem.MinMaxCurve lifetime = GetComponent<ParticleSystem>().main.startLifetime;
        float time = lifetime.constantMax;
        StartCoroutine(ReturnToPool(time)); //For optimization purposes instead of destroying husks prefabs are returning to the object pool
    }
    private IEnumerator ReturnToPool(float lifetime) //For optimization purposes instead of destroying husks prefabs are returning to the object pool after its animation over
    {
        yield return new WaitForSeconds(lifetime);
        ErasureManager.ReturnToPool(gameObject);
    }
}
