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
    private float currentAvgFPS = 0;
    private float[] fpsDict;
    private int[] qty;
    private int maxEdges = 0;

    private List<float> frameTime;
    private Dictionary<int, int> nodesEdges;

    // Start is called before the first frame update
    void Start()
    {
        // Inizialize lines for the eges of the network
        lines = new List<GameObject>();

        keys = LoadFile.particlesBlood.Keys.ToArray();

        num = 0;

        execution = false;

        //fpsDict = new float[20];
        //qty = new int[20];

        frameTime = new List<float>();

        nodesEdges = new Dictionary<int, int>();

        EdgesData();
    }

    // Update is called once per frame
    void Update()
    {
        // 1. Translate network
        //Vector3 translate = new Vector3(0.0f, Mathf.Sin(Time.time * 3) * Time.deltaTime, 0.3f * Time.deltaTime);
        //network.transform.Translate(translate);

        // 2. Scale network
        //if ((int)Math.Round(Mathf.Sin(Time.time * 3f)) < 0)
        //{
        //    network.transform.localScale *= 0.99f;
        //}
        //else
        //{
        //    network.transform.localScale *= 1.01f;
        //}

        // 3. Node selection and line rendering
        //if (execution == false && num < LoadFile.networkBlood.Count - 1)
        //{
        //    selectNodeCoroutine = selectNode(keys[num], 0.1f);
        //    StartCoroutine(selectNodeCoroutine);
        //}
        //else if (num == 20)
        //{
        //    string results = "";
        //    int j = 1;
        //    foreach (float i in fpsDict)
        //    {
        //        results = results + " (" + j + ", " + i + ")";
        //        j++;
        //    }
        //    Debug.Log(results);
        //    num += 1;
        //}

        // Calculate distribution for number of edges

        // Calculate average low
        //if (Time.frameCount >= 101 && Time.frameCount <= 301)
        //{
        //    frameTime.Add(Time.deltaTime * 1000);
        //}


        //if (Time.frameCount == 301)
        //{
        //    CalculatePercentage();
        //    return;
        //}

    }

    private IEnumerator selectNode(string gene_string, float waitTime)
    {
        execution = true; // set the flag
        int numLines = 0;

        yield return new WaitForSeconds(waitTime);

        // Haptics right controller vibration
        StartCoroutine(Haptics(0.5f, 0.5f, 0.2f, true, false));

        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();

        int maxLines = LoadFile.networkBlood[gene_string].Count;
        //Debug.Log("Maxlines for " + num + " is " + maxLines);

        List<string> DoubleGenes = LoadFile.networkBlood[gene_string];
        int maxParticle = LoadFile.particlesBlood.Count;
        List<string> geneNames = new List<string>(LoadFile.particlesBlood.Keys);
        //List<string> newNetwork = new List<string>(geneNames.GetRange(maxParticle - 30, maxParticle - 1));

        //foreach(string name in newNetwork)
        //{
        //    if (!newNetwork.Contains(name))
        //    {
        //        DoubleGenes.Add(name);
        //    }
        //}

        foreach (string remote_gene in LoadFile.networkBlood[gene_string])
        //foreach (string remote_gene in DoubleGenes)
        {
            //fpsDict[num] = UpdateCumulativeMovingAverageFPS(1 / Time.deltaTime, num);
            //if (++currentNumLines == maxLines) break;
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

                numLines++;
            }
            catch (InvalidCastException e)
            {
                Debug.Log($"There was an error adding a line: {e}");
                continue;
            }

        }

        execution = false; // clear the flag before returning

        Debug.Log(gene_string + " " + numLines + " lines");

        // Update node id
        num ++;
    }

    private IEnumerator Haptics(float frequency, float amplitude, float duration, bool rightHand, bool leftHand)
    {
        if (rightHand) OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
        if (leftHand) OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.LTouch);

        yield return new WaitForSeconds(duration);

        if (rightHand) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        if (leftHand) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }

    private float UpdateCumulativeMovingAverageFPS(float newFPS, int nodeID)
    {
        ++qty[nodeID];
        currentAvgFPS += (newFPS - currentAvgFPS) / qty[nodeID];

        return currentAvgFPS;
    }

    private void CalculatePercentage()
    {
        float average = 0.0f;

        Debug.Log("Calculate percentage");
        Debug.Log("We sort the items first in a descending way");

        frameTime.Sort();
        frameTime.Reverse();

        Debug.Log("Total number of item is " + frameTime.Count);
        Debug.Log("We get now the 25% of the highest times, which is the 50 first items from the list");

        var subList = frameTime.GetRange(0, 50);

        Debug.Log("These are the items:");

        foreach (float item in subList)
        {
            Debug.Log(item);
            average += item;
        }

        Debug.Log("Now we calculate the average of these times");

        average /= 50;

        Debug.Log("The average of the 25% is " + average);
    }

    private void EdgesData()
    {
        int maxLines = 0;
        string maxGene = "";

        // Calculate edges
        foreach (string gene in LoadFile.particlesBlood.Keys)
        {
            var lineCount = 0;
            foreach (string remote_gene in LoadFile.networkBlood[gene])
            {
                try
                {
                    if (!LoadFile.particlesBlood.ContainsKey(remote_gene))
                        continue;   

                    Vector3[] vs = new Vector3[2];

                    vs[0] = transform.TransformPoint(LoadFile.particlesBlood[remote_gene].position);
                    vs[1] = transform.TransformPoint(LoadFile.particlesBlood[gene].position);

                    lineCount++;
                }
                catch (InvalidCastException e)
                {
                }
            }

            if(lineCount == 10 || lineCount == 110 || lineCount == 900 || lineCount == 2900 || lineCount == 5860 || lineCount == 11450 || lineCount == 7560){
                Debug.Log(lineCount + " - " + gene);
            }

            if(maxLines < lineCount)
            {
                maxLines = lineCount;
                maxGene = gene;
            }

            if(nodesEdges.ContainsKey(lineCount)){
                var val = nodesEdges[lineCount];
                val++;
                nodesEdges[lineCount] = val;
            }else{
                nodesEdges[lineCount] = 1;
            }
        }

        // foreach (string remote_gene in LoadFile.networkBlood[maxGene])
        // {
        //     try
        //     {
        //         if (!LoadFile.particlesBlood.ContainsKey(remote_gene))
        //             continue;

        //         Vector3[] vs = new Vector3[2];
        //         GameObject clone;
        //         LineRenderer clone_line;

        //         vs[0] = transform.TransformPoint(LoadFile.particlesBlood[remote_gene].position);
        //         vs[1] = transform.TransformPoint(LoadFile.particlesBlood[maxGene].position);

        //         clone = Instantiate(line);
        //         clone.transform.parent = network.transform;
        //         clone_line = clone.GetComponent<LineRenderer>();

        //         clone_line.SetPositions(vs);

        //         lines.Add(clone);
        //     }
        //     catch (InvalidCastException e)
        //     {
        //     }
        // }

        Debug.Log("the gene with max num of edges is " + maxGene);
        Debug.Log("Max num of edges in Blood dataset is " + maxLines);

        string data = "";
        
        foreach(KeyValuePair<int, int> item in nodesEdges){
            data = data + item.Key.ToString() + " " + item.Value.ToString() + "\n";
        }

        Debug.Log(data);
    }
}
