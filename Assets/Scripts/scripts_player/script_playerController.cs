using UnityEngine;
using System.Collections;


[RequireComponent( typeof( CharacterController ) )]
public class script_playerController : MonoBehaviour 
{

    private  Vector3 move;
    public float speed = 3.0F;
    public float rotateSpeed = 3.0F;

    void Update()
    {
        CharacterController controller = GetComponent<CharacterController>();

        if ( Input.GetKey( KeyCode.A ) )
        {
            move = transform.TransformDirection( Vector3.left );
        }
        if ( Input.GetKey( KeyCode.D ) )
        {
            move = transform.TransformDirection( Vector3.right );
        }


        float curSpeed = speed * Input.GetAxis( "Horizontal" );

        controller.SimpleMove( move * curSpeed );
    }
 
}
