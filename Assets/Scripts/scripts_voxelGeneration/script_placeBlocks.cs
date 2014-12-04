using UnityEngine;
using System.Collections;

public class script_placeBlocks : MonoBehaviour 
{
    private     script_PolygonGenerator tScript;
    public      GameObject              target;
    private     LayerMask               layerMask = ( 1 << 0 );
    void Update()
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
            point += ( new Vector2( hit.normal.x, hit.normal.y ) ) * 0.5f;

            tScript.blocks[ Mathf.RoundToInt( point.x - .5f ), Mathf.RoundToInt( point.y + .5f ) ] = 1; // set the block at this point to air

            tScript.update = true;
        }
        else
        {
            Debug.DrawLine( transform.position, target.transform.position, Color.blue );        // blue line to the target if not
        }
    }
}
