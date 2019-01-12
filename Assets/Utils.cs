using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour {
	public static Vector2 GetActualBC2DSize(GameObject a) {

		/* Since BoxCollider2D size does not match scaled size, if we have a game object with a
		 * box collider component, we can use this to get the actual size */
		Transform t = a.GetComponent<Transform>();
		BoxCollider2D b = a.GetComponent<BoxCollider2D>();
		return new Vector2(b.size[0] * t.lossyScale[0], b.size[1] * t.lossyScale[1]);
	
	}

	public static Vector2 GetSizeFromSpriteRenderer(GameObject a) {

		/* Returns width, height as a Vector2, for convenience */
		SpriteRenderer sr = a.GetComponent<SpriteRenderer>();
		Bounds b = sr.bounds;
		return new Vector2(b.size[0], b.size[1]);

	}



    public static Vector2 Vec3ToVec2(Vector3 v) { return new Vector2(v[0], v[1]); }
    public static Vector3 Vec2ToVec3(Vector2 v) { return new Vector3(v[0], v[1], 0f); }


  

}

// Tuples not supported in whatever .net framework Unity uses by default
public class Tuple<T,U> { 
    public T One { get; private set; }
    public U Two { get; private set; }

    public Tuple(T one, U two)
    {
        One = one;
        Two = two;
    }
}


public delegate void UDelegateUpdate(); // Delegate our begin(), end(), update() functions to this type
public delegate IEnumerator UDelegateCoroutine(); // Delegate our coroutine() functions to this type

