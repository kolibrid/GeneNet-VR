using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TextTest : MonoBehaviour
{
    public Text myText;

    int test = 0;

    void Update()
    {
        myText.text = test.ToString();

        test++;
    }
}
