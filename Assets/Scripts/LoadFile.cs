using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.UI;
using Zinnia.Cast;
using System.Collections;

public class LoadFile : MonoBehaviour
{
    public Transform rightControllerAlias;
    public Transform headsetAlias;
    public Transform playArea;
    public TextMesh textController;
    public TextMesh textNetwork;
    public GameObject line;
    public Slider slider;

    private ParticleSystem ps;
    public static Dictionary<string, ParticleSystem.Particle> particlesBlood;
    private Dictionary<string, ParticleSystem.Particle> particlesBiopsy;
    public static Dictionary<string, ParticleSystem.Particle> particlesFusion;
    public static Dictionary<string, List<string>> networkBlood;
    private Dictionary<string, List<string>> networkBiopsy;
    private Dictionary<string, Color32> geneColorBlood;
    private Dictionary<string, Color32> geneColorBiopsy;
    public List<GameObject> lines;
    private Dictionary<string, string[]> oncoGroups;
    private Dictionary<string, Color32> cat_color;
    private bool isBlood = true;
    private string currentNode;
    private float previousSliderValue;

    void Start()
    {
        // Inizialize lines for the eges of the network
        lines = new List<GameObject>();
        
        // Initialize Dictionary to store information about each gene and its color
        geneColorBlood = new Dictionary<string, Color32>();
        geneColorBiopsy = new Dictionary<string, Color32>();

        // Slider Value
        previousSliderValue = 0.0f;

        // Initialize OncoGroups
        InitializeOncoGroups();
        
        // Initialize Color Dictionary
        InitializeColors();

        // Get Particle System
        ps = GetComponent<ParticleSystem>();
        
        //Initialize network dictionaries
        particlesBlood = new Dictionary<string, ParticleSystem.Particle>();
        particlesBiopsy = new Dictionary<string, ParticleSystem.Particle>();
        particlesFusion = new Dictionary<string, ParticleSystem.Particle>();

        // Initialize network and categories from Blood sample
        particlesBlood = InitializeNetwork("blood-network", "blood-categories");
        // Initialize network and categories from Biopsy sample
        particlesBiopsy = InitializeNetwork("biopsy-network", "biopsy-categories");

        MergeDatasets();
        
        // Start network with the particles from the blood dataset
        var main = ps.main;
        main.maxParticles = particlesBlood.Values.Count();
        ps.SetParticles(particlesBlood.Values.ToArray());
    }

    void Update(){
        Vector3 rightHeader = Vector3.Cross(playArea.up, headsetAlias.forward);
        Vector3 forwardHeader = Vector3.Cross(rightHeader, playArea.up);
        Quaternion headsetRotation = Quaternion.LookRotation(forwardHeader, playArea.up);

        textNetwork.transform.rotation = headsetRotation;
    }

    // Filter genes method for the toggle menu
    public void FilterGenes(Text label)
    {
        if (slider.value != 0 && slider.value != slider.maxValue)
            return;

        Color32 changeColor = new Color32(0, 0, 0, 255);
        string[] keys = Enumerable.ToArray(isBlood ? particlesBlood.Keys : particlesBiopsy.Keys);
        ParticleSystem.Particle[] m_Particles;
        int numParticlesAlive;
        string[] group = oncoGroups[label.text];

        m_Particles = new ParticleSystem.Particle[ps.main.maxParticles];
        numParticlesAlive = ps.GetParticles(m_Particles);

        for (int i = 0; i < group.Count(); i++)
        {
            string gene = group[i];
            if (keys.Contains(gene))
            {
                int index = Array.IndexOf(keys, gene);
                if (m_Particles[index].startColor.r == 0)
                {
                    changeColor = isBlood ? geneColorBlood[gene] : geneColorBiopsy[gene];
                    m_Particles[index].startColor = changeColor;
                }
                else
                {
                    m_Particles[index].startColor = changeColor;
                }
            }
        }

        ps.SetParticles(m_Particles, numParticlesAlive);
    }

    public void ChangeDataset(){
        var main = ps.main;
        if(isBlood){
            int numParticles = particlesBiopsy.Values.Count();
            main.maxParticles = numParticles;
            ps.SetParticles(particlesBiopsy.Values.ToArray(), numParticles);
        }else{
            int numParticles = particlesBlood.Values.Count();
            main.maxParticles = numParticles;
            ps.SetParticles(particlesBlood.Values.ToArray(), numParticles);
        }

        isBlood = !isBlood;

        resetToggleMenu();

        // Remove lines
        foreach (GameObject line in lines)
        {
            Destroy(line);
        }
        lines.Clear();

        textController.text = "";

        textNetwork.text = "";
    }

    public void MergeDatasets()
    {
        foreach (KeyValuePair<string, ParticleSystem.Particle> itemB in particlesBlood)
        {
            particlesFusion.Add(itemB.Key, itemB.Value);
        }

        foreach (KeyValuePair<string, ParticleSystem.Particle> itemBi in particlesBiopsy)
        {
            if (!particlesFusion.ContainsKey(itemBi.Key))
            {
                particlesFusion.Add(itemBi.Key, itemBi.Value);
            }
        }
    }

    public void TransformDataset()
    {
        float newSliderValue = slider.value;

        if(slider.value == 0.0f || slider.value == slider.maxValue)
        {
            if (isBlood && newSliderValue == 0.0f)
            {
                ps.SetParticles(Enumerable.ToArray(particlesBlood.Values), Enumerable.ToArray(particlesBlood.Values).Length);
                return;
            }else if (!isBlood && newSliderValue == 10.0f)
            {
                ps.SetParticles(Enumerable.ToArray(particlesBiopsy.Values), Enumerable.ToArray(particlesBiopsy.Values).Length);
                return;
            }

            ChangeDataset();
        }
        else
        {
            ParticleSystem.Particle[] fusion = Enumerable.ToArray(particlesFusion.Values);
            string[] keysFusion = Enumerable.ToArray(particlesFusion.Keys);

            foreach (KeyValuePair<string, ParticleSystem.Particle> item in particlesFusion)
            {
                int index = Array.IndexOf(keysFusion, item.Key);

                if (particlesBlood.ContainsKey(item.Key) && particlesBiopsy.ContainsKey(item.Key))
                {
                    Vector3 pos1 = particlesBlood[item.Key].position;
                    Vector3 pos2 = particlesBiopsy[item.Key].position;

                    Vector3 vectorDiff = (previousSliderValue < newSliderValue) ? (pos2 - pos1) : (pos1 - pos2);

                    Vector3 direction = vectorDiff.normalized;
                    float magnitude = vectorDiff.magnitude;
                    float newMagnitude = magnitude / slider.maxValue * newSliderValue;

                    Vector3 newVector = direction * newMagnitude;

                    fusion[index].position = newVector;

                    Color lerpedColor = Color.Lerp(particlesBlood[item.Key].color, particlesBiopsy[item.Key].color, 1 / (slider.maxValue / newSliderValue));
                    fusion[index].color = lerpedColor;
                }
                else
                {
                    if (particlesBlood.ContainsKey(item.Key))
                    {
                        Color lerpedColorBlood = Color.Lerp(particlesBlood[item.Key].color, new Color(0.0f, 0.0f, 0.0f), 1 / (slider.maxValue / newSliderValue));
                        fusion[index].color = lerpedColorBlood;
                    }
                    else
                    {
                        Color lerpedColorBiopsy = Color.Lerp(new Color(0.0f, 0.0f, 0.0f), particlesBiopsy[item.Key].color, 1 / (slider.maxValue / newSliderValue));
                        fusion[index].color = lerpedColorBiopsy;
                    }
                }
            }

            // Update the particle system
            ps.SetParticles(fusion, fusion.Length);

            // Update previous slider value
            previousSliderValue = newSliderValue;
        }
    }

    // Reset checkboxes in 2D Menu
    private void resetToggleMenu()
    {
        GameObject toggleMenu = GameObject.Find("CanvasMenu/Panel/Toggles");
        foreach (Transform toggle in toggleMenu.transform)
        {
            Toggle t = toggle.GetComponent<Toggle>();
            t.isOn = true;
        }
    }

    //Similar to HandleData from Zinnia ObjectPointer
    public void SelectNode(PointsCast.EventData data)
    {
        if (slider.value != 0 && slider.value != slider.maxValue)
            return;

        Dictionary<string, ParticleSystem.Particle> particles;
        Dictionary<string, List<string>> particle_relations;

        Vector3 controllerPosition = rightControllerAlias.transform.position;
        Vector3 right = Vector3.Cross(playArea.up, rightControllerAlias.forward);
        Vector3 forward = Vector3.Cross(right, playArea.up);
        Quaternion controllerRotation = Quaternion.LookRotation(forward, playArea.up);
        //Vector3 controllerDirection = controllerRotation * Vector3.forward * 20;

        string gene_string = "";
        Vector3 gene_pos = new Vector3();
        float min_distance = 8.0f;

        Vector3 rayVector = data.Points[1] - data.Points[0];
        Vector3 forwardRay = rayVector;

        Ray controllerRay = new Ray(data.Points[0], forwardRay);

        particles = isBlood ? particlesBlood : particlesBiopsy;
        particle_relations = isBlood ? networkBlood : networkBiopsy;

        foreach (KeyValuePair<string, ParticleSystem.Particle> item in particles)
        {
            Vector3 pos = transform.TransformPoint(item.Value.position);
            float size = item.Value.startSize * 4;

            float distance = Vector3.Cross(rayVector, pos - controllerRay.origin).magnitude;

            if (distance < size)
            {
                float current_distance = (pos - controllerRay.origin).magnitude;
                if (current_distance < min_distance)
                {
                    min_distance = current_distance;
                    gene_pos = pos;
                    gene_string = item.Key;
                }
            }
        }

        if (min_distance < 50.0f && gene_string != "" && particle_relations.ContainsKey(gene_string) && currentNode != gene_string)
        {
            // Controller gene text
            textController.text = gene_string;

            textNetwork.text = gene_string;
            textNetwork.transform.position = gene_pos;

            currentNode = gene_string;

            // Haptics right controller vibration
            StartCoroutine(Haptics(0.5f, 0.5f, 0.2f, true, false));

            foreach (GameObject line in lines)
            {
                Destroy(line);
            }
            lines.Clear();

            // CPU usage - Evaluation
            foreach (string remote_gene in particle_relations[gene_string])
            {
                try
                {
                    Vector3[] vs = new Vector3[2];
                    GameObject clone;
                    LineRenderer clone_line;

                    vs[0] = transform.TransformPoint(particles[remote_gene].position);
                    vs[1] = transform.TransformPoint(particles[gene_string].position);

                    clone = Instantiate(line);
                    clone.transform.parent = this.transform;
                    clone_line = clone.GetComponent<LineRenderer>();

                    clone_line.SetPositions(vs);

                    lines.Add(clone);
                }
                catch (InvalidCastException e)
                {
                    Debug.Log($"There was an error adding a line: {e}");
                }

            }
        }
    }

    // Initialize array with color names and RGBA code
    private void InitializeColors()
    {
        // Cluster colours
        cat_color = new Dictionary<string, Color32>
        {
            { "blue", new Color32(0, 0, 255, 255) },
            { "white", new Color32(255, 255, 255, 255) },
            { "yellow", new Color32(255, 255, 0, 255) },
            { "cyan", new Color32(0, 255, 255, 255) },
            { "lightcyan", new Color32(244, 255, 255, 255) },
            { "darkgreen", new Color32(0, 128, 0, 255) },
            { "darkgrey", new Color32(50, 50, 50, 255) },
            { "darkred", new Color32(128, 0, 0, 255) },
            { "darkturquoise", new Color32(0, 128, 128, 255) },
            { "green", new Color32(0, 255, 0, 255) },
            { "grey60", new Color32(128, 128, 128, 255) },
            { "magenta", new Color32(255, 0, 255, 255) },
            { "midnightblue", new Color32(0, 0, 128, 255) },
            { "lightyellow", new Color32(255, 255, 128, 255) },
            { "pink", new Color32(255, 128, 255, 255) },
            { "purple", new Color32(128, 0, 128, 255) },
            { "violet", new Color32(238, 130, 238, 255) },
            { "saddlebrown", new Color32(139, 69, 19, 255) },
            { "brown", new Color32(165, 42, 42, 255) },
            { "tan", new Color32(210, 180, 140, 255) },
            { "salmon", new Color32(233, 150, 122, 255) },
            { "greenyellow", new Color32(173, 255, 47, 255) },
            { "turquoise", new Color32(64, 224, 208, 255) },
            { "darkorange", new Color32(255, 140, 0, 255) },
            { "royalblue", new Color32(0, 35, 102, 255) },
            { "crimsom", new Color32(220, 20, 60, 255) }
        };
    }

    // Initialize oncoligc groups for the filtering
    private void InitializeOncoGroups()
    {
        TextAsset oncoGroups_text = Resources.Load<TextAsset>("oncoGroups");
        string[] oncoGroups_array = oncoGroups_text.text.Split('\n');

        oncoGroups = new Dictionary<string, string[]>();

        for (int group = 0; group < oncoGroups_array.Length - 1; group++)
        {
            string[] content = oncoGroups_array[group].Split(',');
            string[] genes = content[1].Split(' ');
            oncoGroups[content[0]] = genes;
        }
    }

    private IEnumerator Haptics(float frequency, float amplitude, float duration, bool rightHand, bool leftHand)
    {
        if (rightHand) OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
        if (leftHand) OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.LTouch);

        yield return new WaitForSeconds(duration);

        if (rightHand) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        if (leftHand) OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
    }

    private Dictionary<string, ParticleSystem.Particle> InitializeNetwork(string networkName, string networkCategories){
        // We load the network and categories files and save the information in arrays
        string[] network = Resources.Load<TextAsset>(networkName).text.Split('\n');
        string[] categories = Resources.Load<TextAsset>(networkCategories).text.Split('\n');
        Dictionary<string, List<string>> particle_relations = new Dictionary<string, List<string>>();
        Dictionary<string, ParticleSystem.Particle> particleDict = new Dictionary<string, ParticleSystem.Particle>();
        bool isBiopsy = (networkName == "biopsy-network");
        int maxIt = 10;
        int ranPos = 50;
        int norm = 10;

        for(int cat = 0; cat < categories.Length - 1; cat++) {
            string[] content = categories[cat].Split(',');
            string[] genes = content[1].Split(' ');
            Color32 particle_color = cat_color[content[0]];
            
            for(int gene = 0; gene < genes.Length; gene++) {
                ParticleSystem.Particle new_particle = new ParticleSystem.Particle
                {
                    remainingLifetime = 100000.0f,
                    startLifetime = 100000.0f,
                    startSize = 0.1f,
                    startColor = particle_color,
                    position = new Vector3((UnityEngine.Random.value * ranPos) - 25, (UnityEngine.Random.value * 10) - 5, (UnityEngine.Random.value * ranPos) - 25)
                };
                
                particleDict[genes[gene]] = new_particle;
                if (isBiopsy)
                {
                    geneColorBiopsy[genes[gene]] = particle_color;
                }
                else
                {
                    geneColorBlood[genes[gene]] = particle_color;
                }
            }
        }
        
        List<string> keys = Enumerable.ToList(particleDict.Keys);
        int size = keys.Count;
        for(int it = 0; it < maxIt; it++) {
            for(int conn = 1; conn < network.Length - 1; conn++) {
                string[] elems = network[conn].Split(',');
                string gene1 = elems[0].Replace("\"", string.Empty);
                gene1 = gene1.Replace(",", string.Empty);

                string gene2 = elems[1].Replace("\"", string.Empty);
                gene2 = gene2.Replace(",", string.Empty);

                if(!particle_relations.ContainsKey(gene1)) {
                    particle_relations[gene1] = new List<string>();
                }
                particle_relations[gene1].Add(gene2);

                if(!particle_relations.ContainsKey(gene2)) {
                    particle_relations[gene2] = new List<string>();                    
                }
                particle_relations[gene2].Add(gene1);


                try {
                    int randint = (int)(UnityEngine.Random.value * size);
                    ParticleSystem.Particle particle = particleDict[gene1];
                    Vector3 avoid_direction = particle.position - particleDict[keys[randint]].position;
                    particle.position += avoid_direction.normalized / norm;
                    Vector3 direction = particleDict[gene2].position - particle.position;                    
                    particle.position += direction.normalized/5;
                    particleDict[gene1] = particle;
                } catch {
                    continue;
                }
            }
        }

        if (isBiopsy)
            networkBiopsy = particle_relations;
        else
            networkBlood = particle_relations;

        Dictionary<string, ParticleSystem.Particle> particlesReal = new Dictionary<string, ParticleSystem.Particle>();
        foreach(var p in particleDict.Keys) {
            if(particle_relations.ContainsKey(p)) {
                particlesReal[p] = particleDict[p];
            }
        }

        return particlesReal;
    }
        
}
