using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Close : MonoBehaviour
{
    private float time = 0;
    void Update()
    {
        time += Time.deltaTime;

        if (time >= 3)
            gameObject.SetActive(false);
    }
}
