using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Experiment : MonoBehaviour
{
    public GameObject myPs;
    public GameObject line;

    private List<GameObject> lines;
    private int num;
    private List<float> frameTime;
    private string[] experimentNodes = { "TGFBR3", "EPSTI1", "SMNDC1", "HNRNPH3", "ANGEL2", "FOPNL", "ACTR6", "ARGLU1" };
    private Dictionary<int, int> nodesEdges;
    private Dictionary<int, float> edgesTime;
    private Dictionary<int, string> scaltyNodes;
    private Dictionary<string, ParticleSystem.Particle> particles;
    private Dictionary<string, List<string>> network;
    private int numFrames = 301;

    
    private string experiment;
    private string average;
    private bool isBlood;
    private float sizeDataset;

    // Start is called before the first frame update
    void Start()
    {
        particles = new Dictionary<string, ParticleSystem.Particle>();
        network = new Dictionary<string, List<string>>();
        /*
         * Experiment that we are going to run. These are the opetions:
         * translate: translation of the network.
         * scale: scale the network.
         * selectNode: experiment for the selection of 8 nodes.
         * scalability: tests the scalability of the dataset.
         * pcHeadset: compares the performance between the PC and the Headset
         */
        experiment = "scalability";
        average = "scalability";

        // If it is true, the blood dataset is used, if false, the biopsy dataset is used.
        isBlood = true;

        // Size of the dataset, > 0.0f and <= 1.0f;
        sizeDataset = 1.0f;

        // Initialize dataset with blood or biopsy and with size of network;
        InitializeDataset();

        // Inizialize lines for the eges of the network
        lines = new List<GameObject>();

        num = 0;

        frameTime = new List<float>();

        nodesEdges = new Dictionary<int, int>();

        edgesTime = new Dictionary<int, float>();
        scaltyNodes = new Dictionary<int, string>();

        if (experiment == "scalability") EdgesData();

        // Calculate averages function
        CalculateAverage();
    }

    void Update()
    {
        switch (experiment)
        {
            // 1. Translate network
            case "translate":
                Vector3 translate = new Vector3(0.0f, Mathf.Sin(Time.time * 3) * Time.deltaTime, 0.3f * Time.deltaTime);
                myPs.transform.Translate(translate);
                if (Time.frameCount >= 501 && Time.frameCount <= 1200) frameTime.Add(Time.deltaTime * 1000);
                if (Time.frameCount == 1200) CalculatePercentage();
                break;
            // 2. Scale network
            case "scale":
                if ((int)Math.Round(Mathf.Sin(Time.time * 3f)) < 0) myPs.transform.localScale *= 0.99f;
                else myPs.transform.localScale *= 1.01f;
                if (Time.frameCount >= 501 && Time.frameCount <= 1200) frameTime.Add(Time.deltaTime * 1000);
                if (Time.frameCount == 1200) CalculatePercentage();
                break;
            // 3. Node selection and line rendering 
            case "selectNode":      
                if (Time.frameCount >= numFrames && num < experimentNodes.Length)
                {
                    numFrames += 100;
                    selectNode(experimentNodes[num]);
                }
                if (num == experimentNodes.Length)
                {
                    CalculatePercentage();
                    num++;
                }
                if (Time.frameCount >= 301 && num < experimentNodes.Length) frameTime.Add(Time.deltaTime * 1000);
                break;
            // 4. Scalability of the network
            case "scalability":
                if (Time.frameCount >= 300 && num < scaltyNodes.Count)
                {
                    selectNode(scaltyNodes.Values.ToArray()[num]);
                }
                if (num == scaltyNodes.Count)
                {
                    CalculateScalability();
                    num++;
                }
                break;
            // 5. PC vs Headset hardware
            case "pcHeadset":
                Vector3 translate2 = new Vector3(0.0f, Mathf.Sin(Time.time * 3) * Time.deltaTime, 0.3f * Time.deltaTime);
                myPs.transform.Translate(translate2);
                if ((int)Math.Round(Mathf.Sin(Time.time * 3f)) < 0) myPs.transform.localScale *= 0.99f;
                else myPs.transform.localScale *= 1.01f;
                if (Time.frameCount >= numFrames && num < experimentNodes.Length)
                {
                    numFrames += 10;
                    selectNode(experimentNodes[num]);
                }
                if (num == experimentNodes.Length)
                {
                    CalculateTimes();
                    num++;
                }
                if (Time.frameCount >= 301) frameTime.Add(Time.deltaTime * 1000);
                break;
        }
    }

    void CalculateAverage()
    {
        int maxRepetitions = 4;
        switch (average)
        {
            case "pcHeadset":
                float[] aValuesPC = new float[70];
                float[] aValuesVR = new float[70];

                for (int it = 1; it <= maxRepetitions; it++)
                {
                    string[] pcData = Resources.Load<TextAsset>("Experiments/PCHeadset/pc" + it).text.Split('\n');
                    string[] vrData = Resources.Load<TextAsset>("Experiments/PCHeadset/vr" + it).text.Split('\n');
                    for (int j = 0; j < 70; j++)
                    {
                        if (aValuesPC.ElementAtOrDefault(j) != null)
                            aValuesPC[j] = (float)aValuesPC[j] + float.Parse(pcData[j]);
                        else
                            aValuesPC[j] = float.Parse(pcData[j]);

                        if (aValuesVR.ElementAtOrDefault(j) != null)
                            aValuesVR[j] = (float)aValuesVR[j] + float.Parse(vrData[j]);
                        else
                            aValuesVR[j] = float.Parse(vrData[j]);
                    }
                }

                int frame = 301;
                string resultsPC = "";
                string resultsVR = "";
                foreach (float value in aValuesPC)
                {
                    resultsPC = resultsPC + frame + " " + value/maxRepetitions + "\n";
                    frame++;
                }

                frame = 301;
                foreach (float value in aValuesVR)
                {
                    resultsVR = resultsVR + frame + " " + value / maxRepetitions + "\n";
                    frame++;
                }

                Debug.Log(resultsPC);
                Debug.Log(resultsVR);
                break;
            case "scalability":
                Dictionary<int, float> scalabilityBlood = new Dictionary<int, float>();
                Dictionary<int, float> scalabilityBiopsy = new Dictionary<int, float>();

                for (int it = 1; it <= maxRepetitions; it++)
                {
                    string[] dataBlood = Resources.Load<TextAsset>("Experiments/Scalability/blood" + it).text.Split('\n');
                    string[] dataBiopsy = Resources.Load<TextAsset>("Experiments/Scalability/biopsy" + it).text.Split('\n');

                    for (int j = 0; j < dataBlood.Length; j++)
                    {
                        string[] content = dataBlood[j].Split(' ');
                        int frameNum = int.Parse(content[0]);
                        if (!scalabilityBlood.Keys.ToList().Contains(frameNum))
                            scalabilityBlood[frameNum] = float.Parse(content[1]);
                        else
                            scalabilityBlood[frameNum] = scalabilityBlood[frameNum] + float.Parse(content[1]);
                    }

                    for (int j = 0; j < dataBiopsy.Length; j++)
                    {
                        string[] content = dataBiopsy[j].Split(' ');
                        int frameNum = int.Parse(content[0]);
                        if (!scalabilityBiopsy.Keys.ToList().Contains(frameNum))
                            scalabilityBiopsy[frameNum] = float.Parse(content[1]);
                        else
                            scalabilityBiopsy[frameNum] = scalabilityBiopsy[frameNum] + float.Parse(content[1]);
                    }
                }

                string resultsBlood = "";
                string resultsBiopsy = "";
                foreach (KeyValuePair<int, float> item in scalabilityBlood)
                {
                    resultsBlood = resultsBlood + item.Key + " " + (item.Value / maxRepetitions) + "\n";
                }
                foreach (KeyValuePair<int, float> item in scalabilityBiopsy)
                {
                    resultsBiopsy = resultsBiopsy + item.Key + " " + (item.Value / maxRepetitions) + "\n";
                }

                Debug.Log(resultsBlood);
                Debug.Log(resultsBiopsy);
                break;
        } 
    }

    void InitializeDataset()
    {
        Dictionary<string, ParticleSystem.Particle> lParticles = isBlood ? LoadFile.particlesBlood : LoadFile.particlesBiopsy;
        network = isBlood ? LoadFile.networkBlood : LoadFile.networkBiopsy;

        if(sizeDataset < 1.0f)
        {
            Debug.Log("sizeDataset: " + sizeDataset);
            int numParticles = (int)(lParticles.Count * sizeDataset);
            List<string> lKeys = lParticles.Keys.ToList();
            List<string> nKeys = lKeys.GetRange(0, numParticles);
            foreach (string name in nKeys)
            {
                particles[name] = lParticles[name];
            }
            foreach(string name in experimentNodes)
            {
                if (!particles.Keys.ToList().Contains(name))
                {
                    particles[name] = lParticles[name];
                }
            }
        }
        else
        {
            particles = lParticles;
        }

        myPs.GetComponent <ParticleSystem>().SetParticles(particles.Values.ToArray(), particles.Count);
        myPs.GetComponent<ParticleSystem>().Stop();
        Debug.Log("There are in total " + particles.Count + " particles in the dataset");
    }

    private void selectNode(string gene_string)
    {
        int numLines = 0;

        // Haptics right controller vibration
        StartCoroutine(Haptics(0.5f, 0.5f, 0.2f, true, false));

        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();

        foreach (string remote_gene in network[gene_string])
        {
            try
            {
                if (!particles.ContainsKey(remote_gene))
                    continue;

                Vector3[] vs = new Vector3[2];
                GameObject clone;
                LineRenderer clone_line;

                vs[0] = particles[remote_gene].position;
                vs[1] = particles[gene_string].position;

                clone = Instantiate(line, myPs.transform);
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

    // Calculates the number of edges for each node. Used to draw a scatter plot.
    private void EdgesData()
    {
        int maxLines = 0;
        string maxGene = "";

        // Calculate edges
        foreach (string gene in particles.Keys)
        {
            var lineCount = 0;
            foreach (string remote_gene in network[gene])
            {
                try
                {
                    if (!particles.ContainsKey(remote_gene))
                        continue;   

                    Vector3[] vs = new Vector3[2];

                    vs[0] = transform.TransformPoint(particles[remote_gene].position);
                    vs[1] = transform.TransformPoint(particles[gene].position);

                    lineCount++;
                }
                catch (InvalidCastException e)
                {
                }
            }

            //if(lineCount == 10 || lineCount == 11 || lineCount == 90 || lineCount == 290 || lineCount == 586 || lineCount == 1145 || lineCount == 7560){
            //    Debug.Log(lineCount + " - " + gene);
            //}

            if (experimentNodes.Contains(gene))
            {
                Debug.Log(gene + " has " + lineCount + " lines");
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

            if (!scaltyNodes.ContainsKey(lineCount))
            {
                scaltyNodes[lineCount] = gene;
            }
        }

        Debug.Log("the gene with max num of edges is " + maxGene);
        Debug.Log("Max num of edges in Blood dataset is " + maxLines);

        string data = "";
        
        foreach(KeyValuePair<int, int> item in nodesEdges){
            data = data + item.Key.ToString() + " " + item.Value.ToString() + "\n";
        }

        Debug.Log(data);
    }

    private void CalculatePercentage()
    {
        float average1 = 0.0f;
        float average025 = 0.0f;
        float average = 0.0f;
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

        foreach (float item in frameTime)
        {
            average += item;
        }

        Debug.Log("Now we calculate the average of these times");

        average025 /= percent025;
        average1 /= percent1;
        average /= frameTime.Count;

        Debug.Log("The average low of the 1% is " + average1 + " milliseconds, the average of the 0,25% is " + average025 + " milliseconds and the average for the " + frameTime.Count + " total number of frames is " + average);
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
