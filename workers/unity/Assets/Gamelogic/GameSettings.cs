using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour {

	public static bool debugMode = true;

	public static float speed = 1f;

	// productivity = items/hours

	// gotta figure out how to control this
	public static float foresterProductivity = 100f * speed;
	public static float quarryProductivity = 100f * speed;
	public static float farmProductivity = 100f * speed;

	// this balances it to require every guy to farm to stay alive
	// farmProductivity = hungerPerHour / wheatHunger

	// how to use
	// if we up wheatHunger to 6, we get an extra 100 wheat per hour,
	// which you can imagine is available to convert to logs or stone

	public static float wheatHunger = 15f;
	public static float hungerPerHour = 500f * speed;


}

