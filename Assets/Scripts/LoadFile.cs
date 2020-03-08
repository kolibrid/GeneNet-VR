using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.UI;

public class LoadFile : MonoBehaviour
{
    public Transform body;
    public TextMesh gene_name;
    public TextMesh gene_name2;
    public Transform gene_position;

    private ParticleSystem ps;
    private ParticleSystem.Particle[] alive_particles;
    private Dictionary<string, ParticleSystem.Particle> particles;
    private Dictionary<string, List<string>> particle_relations;
    private Dictionary<string, ParticleSystem.Particle> particles_real;
    private Dictionary<string, Color32> gene_color;
    private List<LineRenderer> lines;
    private int num_particles;
    private Dictionary<string, string[]> oncoGroups;
    private Dictionary<string, Color32> cat_color;
    
    UnityEvent m_RelationshipEvent;
    

    void Start()
    {
        // Inizialize lines for the eges of the network
        lines = new List<LineRenderer>();

        // Initialize particle dictionary to store informaiton about the gene name
        // and its particle in the network
        particles = new Dictionary<string, ParticleSystem.Particle>();
        
        // Initialize Dictionary to store information about each gene and its color
        gene_color = new Dictionary<string, Color32>();
        
        // Initialize OncoGroups
        InitializeOncoGroups();
        
        // Initialize Color Dictionary
        InitializeColors();

        // Get Particle System
        ps = GetComponent<ParticleSystem>();
        
        // Initialize Network
        InitializeNetwork();
    }

    void Update(){
        Vector3 controllerPosition = body.position;
        //Vector3 controllerPosition = rayController.transform.position + body.position;
        Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTrackedRemote) * body.rotation;
        Vector3 controllerDirection = controllerRotation * Vector3.forward * 10;
        Ray controllerRay = new Ray(controllerPosition, controllerDirection);

        Debug.DrawRay(controllerPosition, controllerDirection, Color.red);

        string gene_string = "";
        Vector3 gene_pos = new Vector3();
        float min_distance = 20.0f;

        foreach (KeyValuePair<string, ParticleSystem.Particle> item in particles) {
            Vector3 pos = transform.InverseTransformPoint(item.Value.position);
            float size = item.Value.startSize * 200;

            float distance = Vector3.Cross(controllerRay.direction, pos - controllerRay.origin).magnitude;

            if(distance < size){
                //Debug.Log($"distance is {distance} and size is {size}");
                float current_distance = (pos - controllerRay.origin).magnitude;
                if(current_distance < min_distance) {
                    min_distance = current_distance;
                    gene_pos = pos;
                    gene_string = item.Key;
                }
            }
        }

        if(min_distance < 50.0f) {
            gene_position.position = gene_pos;
            gene_position.LookAt(2 * gene_position.position - body.position);
            gene_name.text = gene_string;
            gene_name2.text = gene_string;
            gene_name2.transform.position = gene_position.position;
            if(lines.Count < 50) {
                try 
                {
                    foreach(string remote_gene in particle_relations[gene_string]) {
                        GameObject obj = new GameObject("line");
                        LineRenderer lr = obj.AddComponent<LineRenderer>() as LineRenderer;
                        lr.material = new Material(Shader.Find("Sprites/Default"));
                        Vector3[] vs = new Vector3[2];
                        vs[0] = particles[remote_gene].position;
                        vs[1] = particles[gene_string].position;
                        lr.positionCount = vs.Length;
                        lr.SetPositions(vs);
                        lines.Add(lr);
                    }   
                }
                catch
                {
                   
                
                }
            }
        } else {
            foreach(LineRenderer line in lines) {
                Destroy(line);
            }
            lines.Clear();
        } 
    }

    // Filter genes method for the toggle menu
    public void FilterGenes(Text label)
    {
        Color32 changeColor = new Color32(1, 1, 1, 255);
        string[] keys = Enumerable.ToArray(particles_real.Keys);
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
                if (m_Particles[index].startColor.r == 1)
                {
                    changeColor = gene_color[gene];
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

    private void InitializeColors(){
        // Cluster colours
        cat_color = new Dictionary<string, Color32>();

        cat_color.Add("black", new Color32(0,0,0,255));
        cat_color.Add("blue", new Color32(0,0,255,255));
        cat_color.Add("white", new Color32(255,255,255,255));
        cat_color.Add("yellow", new Color32(255,255,0,255));
        cat_color.Add("cyan", new Color32(0,255,255,255));
        cat_color.Add("lightcyan", new Color32(244,255,255,255));
        cat_color.Add("darkgreen", new Color32(0,128,0,255));
        cat_color.Add("darkgrey", new Color32(50,50,50,255));
        cat_color.Add("darkred", new Color32(128,0,0,255));
        cat_color.Add("darkturquoise", new Color32(0,128,128,255));
        cat_color.Add("green", new Color32(0,255,0,255));
        cat_color.Add("grey60", new Color32(128,128,128,255));
        cat_color.Add("magenta", new Color32(255,0,255,255));
        cat_color.Add("midnightblue", new Color32(0,0,128,255));
        cat_color.Add("lightyellow", new Color32(255,255,128,255));
        cat_color.Add("pink", new Color32(255,128,255,255));
        cat_color.Add("purple", new Color32(128,0,128,255));
        cat_color.Add("violet", new Color32(238,130,238,255));
        cat_color.Add("saddlebrown", new Color32(139,69,19,255));
        cat_color.Add("brown", new Color32(165,42,42,255));
        cat_color.Add("tan", new Color32(210,180,140,255));
        cat_color.Add("salmon", new Color32(233,150,122,255));
        cat_color.Add("greenyellow", new Color32(173,255,47,255));
        cat_color.Add("turquoise", new Color32(64,224,208,255));
        cat_color.Add("darkorange", new Color32(255,140,0,255));
        cat_color.Add("royalblue", new Color32(0,35,102,255));
    }

    private void InitializeOncoGroups(){
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

    private void InitializeNetwork(){
        // Initialize network and categories from Blood sample
        string[] network = Resources.Load<TextAsset>("blood-network").text.Split('\n');
        string[] categories = Resources.Load<TextAsset>("blood-categories").text.Split('\n');

        for(int cat = 0; cat < categories.Length - 1; cat++) {
            string[] content = categories[cat].Split(',');
            string[] genes = content[1].Split(' ');
            Color32 particle_color = cat_color[content[0]];
            
            for(int gene = 0; gene < genes.Length; gene++) {
                ParticleSystem.Particle new_particle = new ParticleSystem.Particle();
                new_particle.remainingLifetime = 100000.0f;
                new_particle.startLifetime = 100000.0f;
                new_particle.startSize = 0.1f;
                new_particle.startColor =  particle_color;
                new_particle.position = new Vector3(UnityEngine.Random.value * 50, UnityEngine.Random.value * 10, UnityEngine.Random.value * 50);
                particles[genes[gene]] = new_particle;
                gene_color[genes[gene]] = particle_color;
            }
        }
        
        List<string> keys = Enumerable.ToList(particles.Keys);
        particle_relations = new Dictionary<string, List<string>>();
        int size = keys.Count;
        for(int it = 0; it < 10; it++) {
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
                    ParticleSystem.Particle particle = particles[gene1];
                    Vector3 avoid_direction = particle.position - particles[keys[randint]].position;
                    particle.position += avoid_direction.normalized / 10;
                    Vector3 direction = particles[gene2].position - particle.position;                    
                    particle.position += direction.normalized/5;
                    particles[gene1] = particle;
                } catch {
                    continue;
                }
            }
        }

        particles_real = new Dictionary<string, ParticleSystem.Particle>();
        foreach(var p in particles.Keys) {
            if(particle_relations.ContainsKey(p)) {
                particles_real[p] = particles[p];
            }
        }

        IEnumerable<ParticleSystem.Particle> vals = particles_real.Values;
        var main = ps.main;
        main.maxParticles = vals.Count();

        ps.SetParticles(vals.ToArray());
        alive_particles = new ParticleSystem.Particle[ps.main.maxParticles];
        num_particles = ps.GetParticles(alive_particles);
    }
        
}
