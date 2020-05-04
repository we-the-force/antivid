using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BacteriaAnim : MonoBehaviour
{
    public Animator anim;
    public int Bacteria;

    // Start is called before the first frame update
    void Start()
    {
        anim.SetInteger("Bacteria", Bacteria);
    }


}
