﻿using UnityEngine;
using UnityEngine.Events;

public class NetworkManipulator : MonoBehaviour
{
    public Transform network;
    public Transform rightControllerAlias;
    public Transform leftControllerAlias;

    private float currentDistance = 0;
    private float newDistance = 0;
    private Vector3 rControllerInitialPosition;
    UnityEvent m_ScaleEvent;
    UnityEvent m_MoveEvent;

    // Start is called before the first frame update
    void Start()
    {
        rControllerInitialPosition = rightControllerAlias.transform.position;

        if (m_ScaleEvent == null)
            m_ScaleEvent = new UnityEvent();

        m_ScaleEvent.AddListener(Scale);

        if (m_MoveEvent == null)
            m_MoveEvent = new UnityEvent();

        m_MoveEvent.AddListener(Move);
    }

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) > 0.75 && OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.75){
            m_ScaleEvent.Invoke();            
        }else if(OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) < 0.75 && OVRInput.Get(OVRInput.Axis1D.SecondaryHandTrigger) > 0.75){
            m_MoveEvent.Invoke();
        }
        else{
            currentDistance = 0;
            rControllerInitialPosition = rightControllerAlias.transform.position;
        }
    }

    void Scale(){
        Vector3 lControllerPosition = transform.TransformDirection(leftControllerAlias.transform.position);
        Vector3 rControllerPosition = transform.TransformDirection(rightControllerAlias.transform.position);

        // Scale
        float dist = Vector3.Distance(lControllerPosition, rControllerPosition);

        if(currentDistance == 0){
            currentDistance = dist;
        }

        newDistance = dist - currentDistance;

        if(newDistance < 0){
            network.transform.localScale *= 0.99f;
        }else{
            network.transform.localScale *= 1.01f;
        }
    }

    // Change position of the network
    void Move(){
        Vector3 rControllerPosition = rightControllerAlias.transform.position;
        Vector3 vectorPosition = (rControllerPosition - rControllerInitialPosition).normalized;
        network.transform.position += vectorPosition * Time.deltaTime;
    }
}
