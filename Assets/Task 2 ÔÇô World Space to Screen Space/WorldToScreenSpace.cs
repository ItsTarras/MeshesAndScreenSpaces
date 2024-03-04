﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class generates a mesh using a height map texture. The texture defines
 * the size of the mesh, and also the height of each vertex in the mesh
 *
 * You will have to write the code to position the sphere on top of the geometry
 * You can copy your code from Task 4 for this. 
 * 
 * You will also have to write the code to correctly draw the reticle texture
 * over top of the sphere. This will require you to convert the sphere's position
 * into screen space, and use this when you draw the reticle.
 * 
 * PROD321 - Interactive Computer Graphics and Animation 
 * Copyright 2023, University of Canterbury
 * Written by Adrian Clark
 */

public class WorldToScreenSpace : MonoBehaviour
{
    // Defines the height map texture used to create the mesh and set the heights
    public Texture2D heightMapTexture;

    // Defines the height scale that we multiply the height of each vertex by
    public float heightScale = 30;

    // Store our mesh filter at class level so we can access it in the Start
    // and the Update function
    MeshFilter meshFilter;

    // The current position of the sphere (starts at 0,0,0)
    public Vector3Int SpherePosition = new Vector3Int(0, 0, 0);

    // The sphere gameobject (so we can move it around)
    GameObject Sphere;

    // The width and height of the mesh, so we can stop the sphere from going
    // off the edge of the mesh in the update function
    int width, height;

    // The texture used for the reticle
    public Texture2D reticleTexture;

    // Reference to the camera which our scene will be rendered from
    public Camera renderingCamera;

    // Create a list to store our vertices
    List<Vector3> vertices = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        // If no camera was defined, use the camera tagged with MainCamera
        if (renderingCamera == null)
            renderingCamera = Camera.main;

        // Create our sphere primitive
        Sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // Set it's scale to 5,5,5
        Sphere.transform.localScale = new Vector3(5, 5, 5);
        // Set it's colour to red
        Sphere.GetComponent<Renderer>().material.color = Color.red;

        // To make things a bit more challenging, lets rotate the mesh
        transform.localRotation = Quaternion.Euler(0, -15, 0);

        // Create a list to store our triangles
        List<int> triangles = new List<int>();

        // Calculate the Height and Width of our mesh from the heightmap's
        // height and width 
        height = heightMapTexture.height;
        width = heightMapTexture.width;

        // Generate our Vertices
        // Loop through the meshes length and width
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                // Create a new vertex using the x and z positions, and get the
                // y position as the pixel from the height map texture. As it's
                // gray scale we can use any colour channel, in this case red.
                // Multiply the pixel value by the height scale to get the final
                // y value
                vertices.Add(new Vector3(x, heightMapTexture.GetPixel(x, z).r * heightScale, z));
            }
        }

        // Generate our triangle Indicies
        // Loop through the meshes length-1 and width-1
        for (int z = 0; z < height - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                // Multiply the Z value by the mesh width to get the number
                // of pixels in the rows, then add the value of x to get the
                // final index. Increase the values of X and Z accordingly
                // to get the neighbouring indicies
                int vTL = z * width + x;
                int vTR = z * width + x + 1;
                int vBR = (z + 1) * width + x + 1;
                int vBL = (z + 1) * width + x;

                // Create the two triangles which make each element in the quad
                // Triangle Top Left->Bottom Left->Bottom Right
                triangles.Add(vTL);
                triangles.Add(vBL);
                triangles.Add(vBR);

                // Triangle Top Left->Bottom Right->Top Right
                triangles.Add(vTL);
                triangles.Add(vBR);
                triangles.Add(vTR);
            }
        }

        // Create our mesh object
        Mesh mesh = new Mesh();

        // Assign the vertices and triangle indicies
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        // Use recalculate normals to calculate the vertex normals for our mesh
        mesh.RecalculateNormals();

        // Create a new mesh filter, and assign the mesh from before to it
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        // Create a new renderer for our mesh, and use the Standard shader
        // For the material, and set the colour to green
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.color = Color.green;

    }

    // Update is called once per frame
    void Update()
    {
        // If the down arrow is pressed and the sphere is not at the bottom of the mesh
        // move it down
        if (Input.GetKey(KeyCode.DownArrow) && SpherePosition.z > 1) SpherePosition.z--;
        // If the up arrow is pressed and the sphere is not at the top of the mesh
        // move it up
        if (Input.GetKey(KeyCode.UpArrow) && SpherePosition.z < height - 1) SpherePosition.z++;

        // If the left arrow is pressed and the sphere is not at the left edge of the mesh
        // move it left
        if (Input.GetKey(KeyCode.LeftArrow) && SpherePosition.x > 1) SpherePosition.x--;
        // If the right arrow is pressed and the sphere is not at the right edge of the mesh
        // move it right
        if (Input.GetKey(KeyCode.RightArrow) && SpherePosition.x < width - 1) SpherePosition.x++;

        /**** 
         * 
         * TODO: Add code to update the transform position of the sphere. You
         * will need to calculate the height of the mesh at the position of the sphere
         * (you can use the vertices list for this, and the same code as we use for calculating
         * our triangle indices), and set the sphere's world position (stored as transform.position)
         * to the world position of the vertex + the sphere's radius
         * 
         * 
         ****/
        Sphere.transform.position = transform.TransformPoint(vertices[(SpherePosition.z * width + SpherePosition.x)]);
        Vector3 sp = Sphere.transform.position;
        sp.y += (Sphere.GetComponent<Renderer>().bounds.size.x) / 2;
        Sphere.transform.position = sp;
    }

    private void OnGUI()
    {
        /**** 
         * 
         * TODO: Add code to calculate the screen position that the reticle should be drawn at.
         * Keep in mind that screen positions go from 0->Screen.Width and 0->Screen.Height. Also
         * GUI.DrawTexture positions are flipped vertically from the Screen Space positions.
         * 
         ****/
        int sphereRadius = (int)(Sphere.GetComponent<Renderer>().bounds.size.x) / 2;
        float yVal;
        float xVal;
        yVal = renderingCamera.WorldToScreenPoint(Sphere.transform.position).y;
        xVal = renderingCamera.WorldToScreenPoint(Sphere.transform.position).x;
        GUI.DrawTexture(new Rect(xVal - 25, (Screen.height - yVal) - 25, 50, 50), reticleTexture);
    }
}
