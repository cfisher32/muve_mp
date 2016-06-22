using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	int speed = 100;
	int turnSpeed = 100;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		ProcessForwardMove();
		ProcessTurnMove();
	}

	void ProcessForwardMove()
	{
		if(Input.GetKey(KeyCode.W))
		{
			transform.Translate(0, 0, speed * Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.S))
		{
			transform.Translate(0, 0, -speed * Time.deltaTime);
		}
	}

	void ProcessTurnMove()
	{
		if(Input.GetKey(KeyCode.A))
		{
			transform.Rotate(0, -turnSpeed * Time.deltaTime, 0);
		}
		if (Input.GetKey(KeyCode.D))
		{
			transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
		}
	}
}
