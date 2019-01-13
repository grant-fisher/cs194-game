using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// *******************************
// Class to handle all user input
// *******************************

public class Input_ {

		public class KeyStatus 
        {
			public KeyCode? keycode;
			public bool Down = false;
			public bool Pressed = false;
			public int Value = 0; // Only used by MoveX and MoveY to specify direction
			public KeyStatus(KeyCode? _keycode) 
			{ 
                if (_keycode != null)
				    keycode = _keycode; 
			}
		} 

		// Used externally
		public static KeyStatus Jump = new KeyStatus(KeyCode.Space);
		public static KeyStatus Dash = new KeyStatus(KeyCode.J);
		public static KeyStatus Attack = new KeyStatus(KeyCode.K);
		public static KeyStatus Grab = new KeyStatus(KeyCode.H);
		public static KeyStatus Swing = new KeyStatus(KeyCode.L);

		// Used externally once set
		public static KeyStatus MoveX = new KeyStatus(null);
		public static KeyStatus MoveY = new KeyStatus(null);
		public static KeyStatus DeltaRope = new KeyStatus(null);

		// Used internally to determine movement 
		private static KeyStatus _Left = new KeyStatus(KeyCode.A);
		private static KeyStatus _Right = new KeyStatus(KeyCode.D);
		private static KeyStatus _Up = new KeyStatus(KeyCode.W);
		private static KeyStatus _Down = new KeyStatus(KeyCode.S);

		// List of all user-handled keys 
		private List<KeyStatus> li = new List<KeyStatus>() { Jump, Dash, Attack, Grab, _Left, _Right, _Up, _Down, Swing };


		public void PrintInputState()
		{
			Debug.Log("Jump: " 		+ (Jump.Pressed ? 1 : 0) + " " 		+ (Jump.Down ? 1 : 0));
			Debug.Log("Dash: " 		+ (Dash.Pressed ? 1 : 0) + " " 		+ (Dash.Down ? 1 : 0));
			Debug.Log("Attack: " 	+ (Attack.Pressed ? 1 : 0) + " " 	+ (Attack.Down ? 1 : 0));
			Debug.Log("Grab: " 		+ (Grab.Pressed ? 1 : 0) + " " 		+ (Grab.Down ? 1 : 0));
			Debug.Log("_Left: " 	+ (_Left.Pressed ? 1 : 0) + " " 	+ (_Left.Down ? 1 : 0));
			Debug.Log("_Right: " 	+ (_Right.Pressed ? 1 : 0) + " " 	+ (_Right.Down ? 1 : 0));
			Debug.Log("_Up: " 		+ (_Up.Pressed ? 1 : 0) + " " 		+ (_Up.Down ? 1 : 0));
			Debug.Log("_Down: " 	+ (_Down.Pressed ? 1 : 0) + " " 	+ (_Down.Down ? 1 : 0));
			Debug.Log("Swing: " 	+ (Swing.Pressed ? 1 : 0) + " " 	+ (Swing.Down ? 1 : 0));

		}		

        public Dictionary<string, KeyStatus> RecordInputState()
        {
            return new Dictionary<string, KeyStatus>() 
            {
                {"Jump", Jump},
                {"Dash", Dash},
                {"Attack", Attack},
                {"Grab", Grab},
                {"DeltaRope", DeltaRope},
                {"_Left", _Left},
                {"_Right", _Right},
                {"_Up", _Up},
                {"_Down", _Down},
				{"Swing", Swing}
            };
        }


		// Update the Down and Pressed members of each key in our list, then set MoveX and MoveY accordingly
		public void Capture() 
        {

			foreach (KeyStatus key in li) 
            {
                // This will throw a runtime exception if key.keycode is null, but I think that's okay, since
                // we shouldn't be investigating anything outside of li, all of which have keycodes defined.
                KeyCode k = key.keycode ?? KeyCode.Escape;

                if (k == KeyCode.Escape)
                {
                    throw new NullKeyCodeException("Tried to capture invalid key");
                }

                key.Down = Input.GetKeyDown(k);
				key.Pressed = Input.GetKey(k);
			}

			// Some of our KeyStatus objects are used for the value field, which is determined
			// by other key status objects
			MoveX.Value = (_Left.Pressed ? -1 : 0) + (_Right.Pressed ? 1 : 0);
			MoveY.Value = (_Down.Pressed ? -1 : 0) + (_Right.Pressed ? 1 : 0);
			DeltaRope.Value = (_Down.Pressed ? -1 : 0) + (_Right.Pressed ? 1 : 0);

		}	

}

// Exceptions
public class NullKeyCodeException : Exception 
{
    public NullKeyCodeException(string message) : base(message) 
    {

    }
}