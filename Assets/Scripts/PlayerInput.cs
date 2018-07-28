using UnityEngine;
using System.Collections;

[RequireComponent(typeof (Player))]
public class PlayerInput : MonoBehaviour {

    Player player;
	// Use this for initialization
	void Start () {
        player = GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () {
	    Vector2 directionalInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        player.SetDirectionalInput(directionalInput);
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire1")) {
            player.OnJumpKeyDown();
        }
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Fire1"))
        {
            player.OnJumpKeyUp();
        }
    }
}
