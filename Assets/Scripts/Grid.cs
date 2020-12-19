using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Vector3[,] grid;
    [SerializeField]
    float gridXPos;
    [SerializeField]
    float gridYPos;
    [SerializeField]
    int gridWidth;
    [SerializeField]
    int gridHeight;
    public float gridSize;


    private void Start() {
        
        GenerateGrid();
    }

    private void OnValidate() {
        GenerateGrid();
    }
    void GenerateGrid() {
        Vector3 bottomLeft = new Vector3(gridXPos - (.5f * gridWidth * gridSize) + .5f * gridSize, .5f, gridYPos - (.5f * gridHeight * gridSize) + .5f * gridSize);

        grid = new Vector3[gridWidth, gridHeight];
        for (int i = 0; i < gridWidth; i++) {
            for (int j = 0; j < gridHeight; j++) {
                grid[i, j] = new Vector3(bottomLeft.x + (i * gridSize), bottomLeft.y, bottomLeft.z + (j * gridSize));
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.black;
        for (int i = 0; i < gridWidth; i++) {
            for (int j = 0; j < gridHeight; j++) {
                Gizmos.DrawWireCube(grid[i, j], new Vector3(1, 0, 1) * gridSize * .9f);
            }
        }
    }
}
