# GeneNet VR
<img src="images/bignet_vr.png" width="100%" height="auto" alt="Screenshot from GeneNet VR" />

GeneNet VR is a prototype of a bioinformatics VR application for the Oculus Quest to visualize large biological networks. It uses two datasets from MIxT as a case study. MIxT is a web application for interactive data exploration in system biology developed by UiT and Concordia University. MIxT provides an [interactive 2-dimensional view](https://mixt-tumor-stroma.bci.mcgill.ca/network) for networks. In this project we use Virtual Reality to solve problems related to the visualization of large biological networks, such as: information overload, high interconnectivity, and high dimensionality.

A video showing the application can be viewed here on YouTube: https://youtu.be/N4QDZiZqVNY

## Installation

Clone the repository locally and open it with Unity using the 2018.4.10f1 version. You can run the project on your Oculus Quest by using the Oculus Link or by creating an .apk file in order to run the application on the Oculus Quest hardware.

## Usage
<img src="images/oculus_quest_inputs.png" width="100%" height="auto" alt="Mapping of the different actions for the Oculus Quest controllers" />
GeneNet VR uses the Oculus Quest controllers to interact with the biological networks. These are the different actions that we can use, check the image above for a reference on how to use them with the controllers.

1. Snap rotation: rotates the camera 45 degrees to the right or to the left.
2. Filter and morph menu: it shows a 2-dimensional menu in front of the user. The menu contains checkboxes with oncogroup categories, which the user can uncheck or check to hide or show the genes from the dataset that correspond to those groups. In addition, a slider is on the menu and it allows the user to morph the networks in order to compare them.
3. Scale network: the user can scale up or down the network using these hand triggers.
4. Translate network: the user can translate the network around the scene using this hand trigger.
5. Select node: the user can select nodes using a laser pointer.
6. Select item in menu: this button is used to interact with the 2-dimensional menu.
7. Oculus menu: it opens the menu from Oculus.
8. Teleport: the user can move around by teleporting to other places in the scene.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## License
![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)
