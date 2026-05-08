using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
public class VRAnim : MonoBehaviour
{
    [NonSerialized]public NavMeshAgent agent;
    [NonSerialized]public Animator anim;
    // Start is called before the first frame update
    void Awake() {
        agent = GetComponentInChildren<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
    }

    public void SetPoint(Vector3 point) {
        StartCoroutine(PointTest(point));
    }

    IEnumerator PointTest(Vector3 point) {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1,3));
        agent.SetDestination(point);
    }
}
