using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class Grid : MonoBehaviour
{
    public Vector3[,] grid;
    [SerializeField]
    float gridXPos = 0;
    [SerializeField]
    float gridYPos = 0;
    [SerializeField]
    int gridWidth = 10;
    [SerializeField]
    int gridHeight = 10;
    [SerializeField]
    float gridSize = 5;
    public float GridSize {
        get {
            return gridSize;
        }
        private set {
            gridSize = value;
        }
    }

    public Vector3 GridClamp(Vector3 position) {
        // clamps a given Vector3 to conform with the grid step
        float new_x = Mathf.Floor(position.x / GridSize + (gridWidth % 2) * 0.5f) * GridSize + (1 - gridWidth % 2) * 2.5f;
        float new_z = Mathf.Floor(position.z / GridSize + (gridHeight % 2) * 0.5f) * GridSize + (1 - gridHeight % 2) * 2.5f;

        return new Vector3(new_x, position.y, new_z);
    }


    private void Start() {
        GenerateGrid();
    }

    private void OnValidate() {
        GenerateGrid();
    }
    private void GenerateGrid() {
        Vector3 bottomLeft = new Vector3(gridXPos - (.5f * gridWidth * GridSize) + .5f * GridSize, .5f, gridYPos - (.5f * gridHeight * GridSize) + .5f * GridSize);

        grid = new Vector3[gridWidth, gridHeight];
        for (int i = 0; i < gridWidth; i++) {
            for (int j = 0; j < gridHeight; j++) {
                grid[i, j] = new Vector3(bottomLeft.x + (i * GridSize), bottomLeft.y, bottomLeft.z + (j * GridSize));
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.black;
        for (int i = 0; i < gridWidth; i++) {
            for (int j = 0; j < gridHeight; j++) {
                Gizmos.DrawWireCube(grid[i, j], new Vector3(1, 0, 1) * GridSize * .9f);
                Gizmos.DrawWireSphere(grid[i, j], 0.1f);
            }
        }
    }
}
