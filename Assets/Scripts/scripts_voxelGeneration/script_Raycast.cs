using UnityEngine;
using System.Collections;

public class script_Raycast : MonoBehaviour 
{
    private     script_PolygonGenerator tScript;
    public      GameObject              target;
    private     LayerMask               layerMask = ( 1 << 0 );

	void Update () 
    {   
        tScript = GameObject.FindGameObjectWithTag( "PolygonGenerator" ).gameObject.GetComponent( "script_PolygonGenerator" ) as script_PolygonGenerator;

        RayCast();
	}

    void RayCast()
    {
        RaycastHit  hit;
        float       distance = Vector3.Distance( transform.position, target.transform.position );

        if ( Physics.Raycast( transform.position, ( target.transform.position - transform.position ).normalized, out hit, distance, layerMask ) )
        {
            Debug.DrawLine( transform.position, hit.point, Color.red );                         // line drawn in red to the hit point if it collides

            Vector2 point   =   new Vector2( hit.point.x, hit.point.y );
                    point  += ( new Vector2( hit.normal.x, hit.normal.y ) ) * -0.5f;

            int roundXtoInt = Mathf.RoundToInt( point.x - .5f );
            int roundYtoInt = Mathf.RoundToInt( point.y + .5f );

            tScript.blocks[ roundXtoInt, roundYtoInt ] = script_PolygonGenerator.air; // set the block at this point to air

            tScript.update = true;

            //Debug.Log( "blocks[ " + tScript.blocks[ roundXtoInt, roundYtoInt ] + " ] hit!" );

            switch (tScript.blocks[ roundXtoInt, roundYtoInt ])
            {
                case script_PolygonGenerator.stone:
                    Debug.Log( "Player hit: Stone"  );
                    break;
                case script_PolygonGenerator.dirt:
                    Debug.Log( "Player hit: Dirt"   );
                    break;
                case script_PolygonGenerator.grass:
                    Debug.Log( "Player hit: Grass"  );
                    break;
                case script_PolygonGenerator.sand:
                    Debug.Log( "Player hit: Sand"   );
                    break;
                default:
                    Debug.Log( "Player hit: unknown");
                    break;
            }
        }
        else
        {
            Debug.DrawLine( transform.position, target.transform.position, Color.blue );        // blue line to the target if not
        }
    }

}
