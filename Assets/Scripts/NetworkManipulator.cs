using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NetworkManipulator : MonoBehaviour
{
    public Transform network;
    public float scaleRate = 30;

    private float currentDistance = 0;
    private float newDistance = 0;
    private Vector3 scaleChange;
    private Vector3 lControllerPosition;
    private Vector3 rControllerPosition;
    private Vector3 initialNetworkPosition;
    private Vector3 rControllerInitialPosition;
    private float dist;
    UnityEvent m_ScaleEvent;
    UnityEvent m_MoveEvent;

    // Start is called before the first frame update
    void Start()
    {
        initialNetworkPosition = network.transform.position;

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
            initialNetworkPosition = network.transform.position;
            rControllerInitialPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTrackedRemote);
        }
    }

    void Scale(){
        lControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTrackedRemote);
        rControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTrackedRemote);

        // Scale
        dist = Vector3.Distance(lControllerPosition, rControllerPosition);

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
        lControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTrackedRemote);
        rControllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTrackedRemote);

        network.transform.position = initialNetworkPosition + (rControllerPosition - rControllerInitialPosition);
    }
}
