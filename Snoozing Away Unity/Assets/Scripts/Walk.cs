using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walk : MonoBehaviour
{
    private Animator animator;

    private Cuboid cuboid;

    private int currentPos;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        cuboid = FindObjectOfType<Cuboid>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
