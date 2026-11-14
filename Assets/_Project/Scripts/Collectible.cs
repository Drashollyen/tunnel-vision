using System;
using System.Collections;
using System.Diagnostics.Metrics;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Collectible : MonoBehaviour
{
    // [SerializeField] int intInstanceQuantity = 1;
    [SerializeField] private Boolean boolInstaniateCheck = true;
    //[SerializeField] private float flCounter = 0f;
    [SerializeField] private float flCollectibleSpeed = 0.15f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {


    }

    IEnumerator CollectibleInstantiateCoroutine()
    {

        if (boolInstaniateCheck)
        {
            yield return new WaitForSeconds(flCollectibleSpeed);
            Instantiate(this, new UnityEngine.Vector3(transform.position.x + (0.1f), transform.position.y + (0.4f), this.transform.position.z), UnityEngine.Quaternion.identity);
            Instantiate(this, new UnityEngine.Vector3(transform.position.x + (0.1f), transform.position.y + (0.4f), this.transform.position.z), UnityEngine.Quaternion.identity);
            Instantiate(this, new UnityEngine.Vector3(transform.position.x + (0.1f), transform.position.y + (0.4f), this.transform.position.z), UnityEngine.Quaternion.identity);
        }


    }

    // Update is called once per frame
    void Update()
    {

        if (boolInstaniateCheck)
        {
            StartCoroutine(CollectibleInstantiateCoroutine());
            boolInstaniateCheck = false;

        }

    }
}
