using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class wall : MonoBehaviour {

    public List<Transform> attacker;
    object locker = new object();
    private void Start()
    {
        attacker = new List<Transform>();
        StartCoroutine(TrsnportRandom());
    }

    IEnumerator TrsnportRandom() {
        while (true)
        {
            lock (locker)
            {
                foreach (var item in attacker)
                {
                    item.transform.position = new Vector3(Random.Range(250, 550), 0, Random.Range(250, 550));
                }
            }
            yield return new WaitForSeconds(3f);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.GetComponent<Attacker>())
        {
            attacker.Add(other.transform);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (attacker.Contains(other.transform))
        {
            lock (locker)
            {
                attacker.Remove(other.transform);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
       
    }
    private void OnCollisionExit(Collision collision)
    {
        
    }
}
