using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddToggles : MonoBehaviour
{
    private string[] oncoGroups;
    
    // Start is called before the first frame update
    void Start()
    {
        TextAsset oncoGroups_text = Resources.Load<TextAsset>("oncoGroups");
        string[] oncoGroups_array = oncoGroups_text.text.Split('\n');

        oncoGroups = new string[oncoGroups_array.Length];

        for (int group = 0; group < oncoGroups_array.Length - 1; group++)
        {
            string[] content = oncoGroups_array[group].Split(',');
            oncoGroups[group] = content[0];
        }

        // Change label of Toggles
        int i = 0;
        foreach (Transform child in transform)
        {
            Text t = child.GetChild(1).GetComponent<Text>();
            t.text = oncoGroups[i];
            i++;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
