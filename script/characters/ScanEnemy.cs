using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(SphereCollider))]
public class ScanEnemy : MonoBehaviour {
    object locker = new object();
    private ChracterBase chara;
    public List<Transform> Enemies;
    private void Awake()
    {
        
        Enemies = new List<Transform>();
        chara = this.GetComponentInParent<ChracterBase>();
    }
    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
	}
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Attacker>() && !other.GetComponent<Attacker>().IsDead)
        {
            lock (locker)
            {
                Enemies.Add(other.transform.root);                
            }
            this.chara.SetAttackTarget(other.transform.root);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (Enemies.Contains(other.transform))
        {
            lock (locker)
            {
                Enemies.Remove(other.transform.root);                
            }
            if (this.chara.target == other.transform.root)
                this.chara.OnTargetLeave();
        }
    }
    public void RemoveTarget(Transform target) {
        lock(locker)
            this.Enemies.Remove(target);
    }
    public bool GetNextTarget() {
        while (Enemies.Contains(null))
            Enemies.Remove(null);
        if (Enemies.Count == 0)
            return false;
        lock (locker)
        {            
            float minDistance = 200f;
            int tarInd = 0;
            for (int i = 0; i < Enemies.Count; i++)
            {
                if (Enemies[i] == GameManager.gameMain.GetBoss())
                {
                    if (this.chara is skeleton)
                    {
                        this.chara.SetAttackTarget(Enemies[i]);
                        return true;
                    }
                }
                float d = Vector3.Distance(this.transform.position, Enemies[i].position);
                if (d < minDistance)
                {
                    tarInd = i;
                    minDistance = d;
                }
            }
            this.chara.SetAttackTarget(Enemies[tarInd]);
        }
        return true;
    }
}
