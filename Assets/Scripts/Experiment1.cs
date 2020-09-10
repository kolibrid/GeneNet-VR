using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Experiment1 : MonoBehaviour
{
    public Transform network;
    public GameObject line;
    private List<GameObject> lines;

    private IEnumerator selectNodeCoroutine;
    string[] keys;
    private int num;
    private bool execution;
    private int numKeys;

    private int qty = 0;
    private float currentAvgFPS = 0;

    private float[] fpsDict;

    // Start is called before the first frame update
    void Start()
    {
        // Inizialize lines for the eges of the network
        lines = new List<GameObject>();

        keys = LoadFile.particlesBlood.Keys.ToArray();
        numKeys = keys.Length;
        num = 0;

        execution = false;

        fpsDict = new float[20];
    }

    // Update is called once per frame
    void Update()
    {
        //network.transform.Translate(Vector3.back * -0.25f * Time.deltaTime);

        if (execution == false && num < 19){
            selectNodeCoroutine = selectNode(keys[num], 0.5f);
            StartCoroutine(selectNodeCoroutine);

            Debug.Log(num);

            qty = 0;
        }else if( num == 18)
        {
            string results = "";
            int j = 1;
            foreach(float i in fpsDict)
            {
                results = results + " (" + j + ", " + i + ")";
                j++;
            }
            Debug.Log(results);
            num += 1;
        }
    }

    private IEnumerator selectNode(string gene_string, float waitTime)
    {
        execution = true; // set the flag

        yield return new WaitForSeconds(waitTime);

        // Haptics right controller vibration
        StartCoroutine(Haptics(0.5f, 0.5f, 0.2f, true, false));

        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();

        int maxLines = LoadFile.networkBlood[gene_string].Count/2;
        int currentNumLines = 0;

        // CPU usage - Evaluation
        foreach (string remote_gene in LoadFile.networkBlood[gene_string])
        {
            if (++currentNumLines == maxLines) break;
            try
            {
                Vector3[] vs = new Vector3[2];
                GameObject clone;
                LineRenderer clone_line;

                vs[0] = transform.TransformPoint(LoadFile.particlesBlood[remote_gene].position);
                vs[1] = transform.TransformPoint(LoadFile.particlesBlood[gene_string].position);

                clone = Instantiate(line);
                clone.transform.parent = network.transform;
                clone_line = clone.GetComponent<LineRenderer>();

                clone_line.SetPositions(vs);

                lines.Add(clone);

                fpsDict[num] = UpdateCumulativeMovingAverageFPS(1 / Time.deltaTime);
            }
            catch (InvalidCastException e)
            {
                Debug.Log($"There was an error adding a line: {e}");
            }

        }

        execution = false; // clear the flag before returning

        num += 1;
    }

    private IEnumerator Haptics(float frequency, float amplitude, float duration, bool rightHand, bool leftHand)
    {
        if (rightHand) OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
        if (leftHand) OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.LTouch);

        yield return new WaitForSeconds(duration);

        if (rightHand) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        if (leftHand) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }

    private float UpdateCumulativeMovingAverageFPS(float newFPS)
    {
        ++qty;
        currentAvgFPS += (newFPS - currentAvgFPS) / qty;

        return currentAvgFPS;
    }
}
