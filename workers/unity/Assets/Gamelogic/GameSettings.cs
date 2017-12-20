using Improbable.Collections;
using UnityEngine;

public class GameSettings : MonoBehaviour {

	public static bool debugMode = false;
	public static int initialCharacters = 4;
	public static int settlementBeds = 4;
	public static int settlementRadius = 2;
	public static int standardRadius = 2;

	public static Improbable.Collections.Map<int,int> startingInventory = new Improbable.Collections.Map<int,int> () {
		{0, 40},
		{1, 40},
		{2, 40}
	};

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

