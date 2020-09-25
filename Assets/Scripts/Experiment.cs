using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Experiment : MonoBehaviour
{
    public Transform network;
    public GameObject line;
    private List<GameObject> lines;

    private IEnumerator selectNodeCoroutine;
    private int num;
    private bool execution;
    private float currentAvgFPS = 0;
    private float[] fpsDict;
    private int[] qty;
    private int maxEdges = 0;

    private List<float> frameTime;
    private string[] experimentNodes = { "TGFBR3", "EPSTI1", "SMNDC1", "HNRNPH3", "ANGEL2", "FOPNL", "ACTR6", "ARGLU1" };
    private Dictionary<int, int> nodesEdges;
    private Dictionary<int, float> edgesTime;
    private Dictionary<int, string> scaltyNodes;
    private int numFrames = 301;

    // Start is called before the first frame update
    void Start()
    {
        // Inizialize lines for the eges of the network
        lines = new List<GameObject>();

        num = 0;

        execution = false;

        //fpsDict = new float[20];
        //qty = new int[20];

        frameTime = new List<float>();

        nodesEdges = new Dictionary<int, int>();

        edgesTime = new Dictionary<int, float>();
        scaltyNodes = new Dictionary<int, string>();

        //EdgesData();
    }

    // Update is called once per frame
    void Update()
    {
        // 1. Translate network
        Vector3 translate = new Vector3(0.0f, Mathf.Sin(Time.time * 3) * Time.deltaTime, 0.3f * Time.deltaTime);
        network.transform.Translate(translate);

        // 2. Scale network
        if ((int)Math.Round(Mathf.Sin(Time.time * 3f)) < 0)
        {
            network.transform.localScale *= 0.99f;
        }
        else
        {
            network.transform.localScale *= 1.01f;
        }

        // 3. Node selection and line rendering        
        if (Time.frameCount >= numFrames && num < experimentNodes.Length)
        {
            numFrames += 10;
            //Debug.Log(experimentNodes[num] + "num is " + num + " of total " + (experimentNodes.Length - 1));
            selectNode(experimentNodes[num]);
        }
        if (num == (experimentNodes.Length - 1))
        {
            //CalculatePercentage();
            CalculateTimes();
            num++;
            return;
        }

        if (Time.frameCount >= 301)
        {
            frameTime.Add(Time.deltaTime * 1000);
        }

        // 4. Scalability of the network
        //if (Time.frameCount >= 300 && num < scaltyNodes.Count)
        //{
        //    selectNode(scaltyNodes.Values.ToArray()[num]);
        //}
        //if (num == (scaltyNodes.Count - 1))
        //{
        //    CalculateScalability();
        //    num++;
        //    return;
        //}


        // Calculate average low
        //if (Time.frameCount >= 501 && Time.frameCount <= 1500)
        //{
        //    frameTime.Add(Time.deltaTime * 1000);
        //}


        //if (Time.frameCount == 1500)
        //{
        //    CalculatePercentage();
        //    return;
        //}

    }

    private void selectNode(string gene_string)
    {
        execution = true; // set the flag
        int numLines = 0;

        //yield return new WaitForSeconds(waitTime);

        // Haptics right controller vibration
        StartCoroutine(Haptics(0.5f, 0.5f, 0.2f, true, false));

        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();

        int maxLines = LoadFile.networkBlood[gene_string].Count;

        foreach (string remote_gene in LoadFile.networkBlood[gene_string])
        {
            try
            {
                if (!LoadFile.particlesBlood.ContainsKey(remote_gene))
                    continue;

                Vector3[] vs = new Vector3[2];
                GameObject clone;
                LineRenderer clone_line;

                vs[0] = LoadFile.particlesBlood[remote_gene].position;
                vs[1] = LoadFile.particlesBlood[gene_string].position;

                clone = Instantiate(line, network.transform);
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

        //Debug.Log(gene_string + " " + numLines + " lines");

        // Update node id
        // Update node id

        // Update dictionary with num edges and time to render
        edgesTime[numLines] = Time.deltaTime * 1000;

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

    // Calculates the number of edges for each node. Used to draw a scatter plot.
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

            //if(lineCount == 10 || lineCount == 110 || lineCount == 900 || lineCount == 2900 || lineCount == 5860 || lineCount == 11450 || lineCount == 7560){
            //    Debug.Log(lineCount + " - " + gene);
            //}

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

            if (!scaltyNodes.ContainsKey(lineCount))
            {
                scaltyNodes[lineCount] = gene;
            }
        }

        //Debug.Log("the gene with max num of edges is " + maxGene);
        //Debug.Log("Max num of edges in Blood dataset is " + maxLines);

        string data = "";
        
        foreach(KeyValuePair<int, int> item in nodesEdges){
            data = data + item.Key.ToString() + " " + item.Value.ToString() + "\n";
        }

        //Debug.Log(data);
    }

    private void CalculatePercentage()
    {
        float average1 = 0.0f;
        float average025 = 0.0f;
        int percent1 = (int)(frameTime.Count * 0.1);
        int percent025 = (int)(frameTime.Count * 0.025);

        Debug.Log("Calculate percentage");
        Debug.Log("We sort the items first in a descending way");

        frameTime.Sort();
        frameTime.Reverse();

        Debug.Log("Total number of items is " + frameTime.Count);
        Debug.Log("We get now the 1% and the 0,25% of the highest times, which is the " + percent1 + " and the " + percent025 + " first items from the list");

        var values1 = frameTime.GetRange(0, percent1);
        var values025 = frameTime.GetRange(0, percent025);

        Debug.Log("These are the items:");

        foreach (float item in values1)
        {
            Debug.Log("Item in 1% " + item);
            average1 += item;
        }

        foreach (float item in values025)
        {
            Debug.Log("Item in 0,25% " + item);
            average025 += item;
        }

        Debug.Log("Now we calculate the average of these times");

        average025 /= percent025;
        average1 /= percent1;

        Debug.Log("The average low of the 1% is " + average1 + " milliseconds and the average of the 0,25% is " + average025 + " milliseconds");
    }

    void CalculateScalability()
    {
        var list = edgesTime.Keys.ToList();
        list.Sort();
        var data = "";

        foreach (var key in list)
        {
            data = data + key + " " + edgesTime[key] + "\n";
        }

        Debug.Log(data);
    }

    void CalculateTimes()
    {
        string data = "";
        int frame = 301;
        foreach (float time in frameTime)
        {
            data = data + frame + " " + time.ToString() + "\n";
            frame++;
        }
        Debug.Log(data);
    }
}
