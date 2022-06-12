using UnityEngine;

// Animation.Play example. Let the S and J keys start
// a spin or jump animation. Let Space play back spin and
// jump at the same time. Let Z play spin and jump with
// spin doubled in speed.
//
// Spin: rotate the cube 360 degrees in half or one second
// Jump: bounce up to 2 units and down in one second
//
// Note: AnimationState.layer is no longer supported, but still exists.

public class GetUp : MonoBehaviour
{
    private Animation anim;

    void Start()
    {
        anim = gameObject.GetComponent<Animation>();
        anim["mixamo_com"].layer = 123;
    }

    // double the spin speed when true
    private bool fastSpin = false;

    void Update()
    {
        // leave spin or jump to complete before changing
        if (anim.isPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Spinning");
            anim.Play("spin");
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            Debug.Log("Jumping");
            anim.Play("jump");
        }

        // combine jump and spin
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Jumping and spinning");
            anim.Play("jump");
            anim.Play("spin");
        }

        // have spin speed reverted to 1.0 second
        if (fastSpin == true)
        {
            anim["spin"].speed = 1.0f;
            fastSpin = false;
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Jumping and spinning in half a second");
            anim.Play("jump");
            anim["spin"].speed = 2.0f;
            anim.Play("spin");

            // we've used spin at a speed of two, now mark
            // the spin speed to revert back to one
            fastSpin = true;
        }
    }
}