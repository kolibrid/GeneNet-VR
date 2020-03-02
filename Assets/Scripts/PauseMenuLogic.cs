using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Prefabs.Locomotion.Teleporters;
using Zinnia.Data.Type;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class PauseMenuLogic : MonoBehaviour
{
    public Transform playArea;
    public Transform headOrientation;
    public Transform gameLocation;
    public GameObject menu;
    public GameObject cameraToAppearInFrontOf;

    protected bool inPauseMenu = false;

    public List<GameObject> pauseItems;
    public List<GameObject> gameItems;

    public GameObject teleportationRelease;
    public GameObject teleportationPress;
    public void SwitchRooms() {
        TransformData teleportDestination = new TransformData(gameLocation);
        if (!inPauseMenu)
        {
            gameLocation.position = new Vector3(headOrientation.position.x, playArea.position.y, headOrientation.position.z);

            Vector3 right = Vector3.Cross(playArea.up, headOrientation.forward);
            Vector3 forward = Vector3.Cross(right, playArea.up);

            gameLocation.rotation = Quaternion.LookRotation(forward, playArea.up);

            menu.transform.position = gameLocation.position + (gameLocation.forward * 1.8f);
            Vector3 incr = new Vector3(0, 1f, 0);
            menu.transform.position += incr;
            menu.transform.rotation = gameLocation.rotation;
        }

        inPauseMenu = !inPauseMenu;

        foreach (GameObject item in pauseItems) {
            item.SetActive(inPauseMenu);
        }

        foreach (GameObject item in gameItems) {
            item.SetActive(!inPauseMenu);
        }

        menu.SetActive(inPauseMenu);
    }
    public void ResetGame() {
        SceneManager.LoadScene("Final", LoadSceneMode.Single);
    }

    public void SwitchTeleportationToPress(bool value) {
        teleportationRelease.SetActive(!value);
        teleportationPress.SetActive(value);
    }
    public void SwitchTeleportationToRelease(bool value) {
        SwitchTeleportationToPress(!value);
    }
}
