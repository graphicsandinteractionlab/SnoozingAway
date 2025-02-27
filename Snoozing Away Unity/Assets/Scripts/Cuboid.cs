﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


/*
 Das ist hier erstmal so wie man es nicht machen soll: alles in einem Skript 

eine Voxelstruktur in der die Charaktere sich bewegen. 

 */

public class Cuboid : MonoBehaviour
{
    [System.Serializable]
    public class Cell
    {
        // state 
        public bool enabled = false;
        // model
        public int code = 0;

        bool[] walkable = {false, false, false, false, false, false};
    }

    protected class Cursor {
        public int pos = 0;
        public int code = 0;
        public bool enabled = false;
    }


    protected Cell[] cells;

    public GameObject cursorShape;
    public GameObject[] cellObjects;
    // approximately the original size
    public Vector3Int dimensions = new Vector3Int(10, 10, 10);
    public float cellSize = 2.0f;

    private Cursor EditorCursor = new Cursor();

    // store cell data - temporary storage also for editor
    private string cellDataFile = "/cells.dat";

    private Vector3 [] cameraPoints;
    private int currentCameraPos = 1;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Application.persistentDataPath);

        // create cells - welcome to Unity vectors having no trace
        cells = new Cell[dimensions.x * dimensions.y * dimensions.z];

        for(int i = 0; i < cells.Length;i++) {
            cells[i] = new Cell();
        }

        var zoom  = 2.0f;

        var camDist = new Vector3(dimensions.x * cellSize * zoom,
            dimensions.x * cellSize * zoom,
            dimensions.x * cellSize * zoom);

        cameraPoints = new Vector3[6];
        cameraPoints[0] = Vector3.Scale(Vector3.forward,camDist);
        cameraPoints[1] = Vector3.Scale(Vector3.back,camDist);
        cameraPoints[2] = Vector3.Scale(Vector3.left,camDist);
        cameraPoints[3] = Vector3.Scale(Vector3.right,camDist);
        cameraPoints[4] = Vector3.Scale(Vector3.up,camDist);
        cameraPoints[5] = Vector3.Scale(Vector3.down,camDist);

        Read();

        UpdateVisuals();

    }

    // Update is called once per frame
    void Update()
    {
        // get the target position
        var target = cameraPoints[currentCameraPos];

        // transform update only if we haven't reached the point
        var delta = Camera.main.transform.position - target;

        // never compare on 0 with FP ;) - remember PROG1 
        // this should be Mathf.Epsilon: thanks Unity for breaking this convention
        if (delta.sqrMagnitude > 0.1F) { 
 
            // can make the 'whoopiness' adjustable with last parameter of slerp
            Camera.main.transform.position = Vector3.Slerp(Camera.main.transform.position,target,0.4f);
            Camera.main.transform.LookAt(Vector3.zero,Vector3.up);
    
        }

        // this should go into some update cursor method ...
        var updatedCursorPos = EditorCursor.pos;

        // general stuff
        if (Input.GetKeyUp(KeyCode.Q)) {
            Application.Quit();
        } else 
        // generate random world
        if (Input.GetKeyUp(KeyCode.G))
        {
            Randomize();
            UpdateVisuals();
        } else if (Input.GetKeyUp(KeyCode.C)) {
            // toggle cursor
            EditorCursor.enabled = !EditorCursor.enabled;
        }
        // save stuff
        else if (Input.GetKeyUp(KeyCode.S))
        {
            Save();
        }
        // read and visualize
        else if (Input.GetKeyUp(KeyCode.R))
        {
            Read();
            UpdateVisuals();
        }
        /* view points */
        else if (Input.GetKeyDown(KeyCode.Alpha0)) {
            currentCameraPos = 0;
        } else if (Input.GetKeyDown(KeyCode.Alpha1)) {
            currentCameraPos = 1;
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            currentCameraPos = 2;
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            currentCameraPos = 3;
        } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            currentCameraPos = 4;
        } else if (Input.GetKeyDown(KeyCode.Alpha5)) {
            currentCameraPos = 5;
        }
        /* minimal editor */
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            updatedCursorPos += dimensions.x;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            updatedCursorPos -= dimensions.x;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            updatedCursorPos += 1;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            updatedCursorPos -= 1;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            updatedCursorPos += dimensions.x * dimensions.y;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            updatedCursorPos -= dimensions.x * dimensions.y;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            cells[EditorCursor.pos].code = EditorCursor.code;
            cells[EditorCursor.pos].enabled = !cells[EditorCursor.pos].enabled;
            UpdateVisuals();
        }

        // just in case
        cursorShape.GetComponent<Renderer>().enabled = EditorCursor.enabled;

        // clamp
        EditorCursor.pos = Mathf.Clamp(updatedCursorPos,0,cells.Length-1);

        // visualize cursor if applicable
        var pos_i = GetPosition(EditorCursor.pos, dimensions);
        Vector3 p = (Vector3)pos_i * cellSize - CenterPoint;

        cursorShape.transform.localPosition = p;
    }

    void Randomize()
    {
        // just for debugging
        foreach(Cell c in cells)
        {
            c.code = Random.Range(0, cellObjects.Length);
            c.enabled = true;
        }
    }

    void UpdateVisuals()
    {
        // delete all children
        gameObject.DeleteAllChildren();

        // build visual representation
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].enabled) 
            {

                var pos_i = GetPosition(i, dimensions);

                Vector3 p = (Vector3)pos_i * cellSize - CenterPoint;

                var item = cellObjects[cells[i].code];

                if (item)
                {
                    var cellObject = Instantiate(item, p, Quaternion.identity);

                    cellObject.transform.parent = transform;
                    // cellObject.transform.localPosition = p;

                    cellObject.name = "cell_" + pos_i.x + "_" + pos_i.y + "_" + pos_i.z;
                }
            }
        }
    }

    void Save()
    {
        string dataPath = Application.persistentDataPath + cellDataFile;

        FileStream file;

        file = File.Exists(dataPath) ? File.OpenWrite(dataPath) : File.Create(dataPath);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, cells);

        file.Close();
    }

    bool Read()
    {

        string dataPath = Application.persistentDataPath + cellDataFile;

        FileStream file;

        if (File.Exists(dataPath))
        {
            file = File.OpenRead(dataPath);
        }
        else
        {
            Debug.LogError("File not found");
            return false;
        }

        BinaryFormatter bf = new BinaryFormatter();

        cells = (Cell[])bf.Deserialize(file);

        file.Close();

        return true;
    }

    public int CellCount 
    {
        get { return dimensions.x * dimensions.y * dimensions.z; }
    }

    public int GetEnabledCount {
        get { 
            int count = 0;
            foreach(Cell c in cells) if (c.enabled) count++;
            return count; 
        }
    }

    public Cell[] Cells
    {
        get { return cells; } 
    }

    public Vector3Int Dimensions
    {
        get { return dimensions; }
    }

    public Vector3 CenterPoint 
    {

        get { return new Vector3(dimensions.x * cellSize * 0.5f,
                dimensions.y * cellSize * 0.5f,
                dimensions.z * cellSize * 0.5f); }
    }

    static int GetOffset(Vector3Int pos, Vector3Int dim)
    {
        return pos.x + pos.y * dim.x + pos.z * dim.x * dim.y;
    }

    public static Vector3Int GetPosition(int idx, Vector3Int dim)
    {
        return new Vector3Int(idx % dim.x, (idx / dim.x) % dim.y, idx / (dim.x * dim.y));
    }
}
