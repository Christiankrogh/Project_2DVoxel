// Animation Sprite Sheet 
//
// Description: Plays an animated sprite using a sprite sheet
// Instruction: Assign script to a gameObject with a material/texture (sprite sheet) 
// Function arguments: 
// columnSize      - number of frames across (horizontal)
// rowSize         - number of frames down (vertical)
// colFrameStart   - where frame starts (remember 0 is first number in counting)
// rowFrameStart   - where frame starts (remember 0 is first number in counting)
// totalFrames     - number of frames in the animation (count regular)
// framesPerSecond - how fast do you want it to play through (Standard: 12 - 30 fps)

using UnityEngine;
using System.Collections;

public class script_aniSprite : MonoBehaviour 
{
	public static void aniSprite ( GameObject spriteSheet, int columnSize, int rowSize, int colFrameStart, int rowFrameStart, int totalFrames, int framesPerSecond )	// function for animating sprites
	{
		float	tileSize= 1.0f;
		int 	index 	= (int)(Time.time * framesPerSecond);																// time control fps
				index 	= index % totalFrames;																			// modulate to total number of frames	
		int 	u 		= index % columnSize;																			// u gets current x coordinate from column size
		int 	v 		= index / columnSize;																			// v gets current y coordinate by dividing by column size
		int 	uStartPosition = u + colFrameStart;
		int 	vStartPosition = v + rowFrameStart;

		Vector2 size 	= new Vector2 ( tileSize / columnSize, tileSize / rowSize);										// adjusts the texture to the correct scale
		Vector2 offset 	= new Vector2 (uStartPosition * size.x, (1 - size.y) - (vStartPosition * size.y)); 				// stores the value to offset the object's texture

		Material spriteSheetMaterial 			= spriteSheet.renderer.material;
        
		spriteSheetMaterial.mainTextureOffset 	= offset;																// texture offset for diffuse map
		spriteSheetMaterial.mainTextureScale  	= size;																	// texture scale  for diffuse map
		
		//renderer.material.SetTextureOffset ("_BumpMap", offset);														// texture offset for bump (normal map)
		//renderer.material.SetTextureScale  ("_BumpMap", size);														// texture scale  for bump (normal map) 
	}
}

