﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class HexInfo : MonoBehaviour
{
/*    //basic hexagon mesh making
    public Vector3[] Vertices;
    public Vector2[] uv;
    public int[] Triangles;

    void Start()
    {
        MeshSetup();
    }

    void MeshSetup()
    {
        #region verts
        float floorLevel = 0;
        Vertices = new Vector3[]
        {
            new Vector3(-1f , floorLevel, -.5f),
            new Vector3(-1f, floorLevel, .5f),
            new Vector3(0f, floorLevel, 1f),
            new Vector3(1f, floorLevel, .5f),
            new Vector3(1f, floorLevel, -.5f),
            new Vector3(0f, floorLevel, -1f)
        };

        #endregion

        #region triangles
        Triangles = new int[]
        {
            1,5,0,
            1,4,5,
            1,2,4,
            2,3,4
        };

        #endregion

        #region uv
        uv = new Vector2[]
        {
            new Vector2(0,0.25f),
            new Vector2(0,0.75f),
            new Vector2(0.5f,1),
            new Vector2(1,0.75f),
            new Vector2(1,0.25f),
            new Vector2(0.5f,0),
        };

        #endregion

        #region finalize
        //add a mesh filter to the GO the script is attached to; cache it for later
        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        //add a mesh renderer to the GO the script is attached to
        gameObject.AddComponent<MeshRenderer>();

        //create a mesh object to pass our data into
        Mesh mesh = new Mesh();

        //add our vertices to the mesh
        mesh.vertices = Vertices;
        //add our triangles to the mesh
        mesh.triangles = Triangles;
        //add out UV coordinates to the mesh
        mesh.uv = uv;

        //make it play nicely with lighting
        mesh.RecalculateNormals();

        //set the GO's meshFilter's mesh to be the one we just made
        meshFilter.mesh = mesh;

        //UV TESTING
        Renderer<Material>.mainTexture = texture;

        #endregion

    } */
}