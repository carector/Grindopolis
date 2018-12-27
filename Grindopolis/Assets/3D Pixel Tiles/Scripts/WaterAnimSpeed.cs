using UnityEngine;
using System.Collections;

//This script finds a single animation in a GameObject and changes its speed. Does not work with multiple animations.
//It is included for the purpose of being able to control the animation speed of the animated water in the 3D Pixel Tiles pack.
public class WaterAnimSpeed : MonoBehaviour {
	public float speedMultiplier = 1.0f; //This multiplies the animation speed. 1 is default speed.
	private string animState;
	
	void Start () {
		animState = GetComponent<Animation>().clip.name; //Get the name of the animation clip.
	}
	
	void Update () {
		GetComponent<Animation>()[animState].speed = speedMultiplier; //Change speed to the one set in speedMultiplier.
	}
}
