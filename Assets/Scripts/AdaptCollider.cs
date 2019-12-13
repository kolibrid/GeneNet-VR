using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptCollider : MonoBehaviour
{
    private ParticleSystem ps;
    BoxCollider m_Collider;

    // Start is called before the first frame update
    void Start()
    {
        // Current Particle System
        ps = GetComponent<ParticleSystem>();
        //Fetch the Collider from the GameObject
        m_Collider = GetComponent<BoxCollider>();

        // Get position and scale from the Particle System
        var pos = ps.transform.position;
        var scl = ps.transform.localScale;

        m_Collider.transform.position = pos;
        m_Collider.transform.localScale = scl;
    }
}
