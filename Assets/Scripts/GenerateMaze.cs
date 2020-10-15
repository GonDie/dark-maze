﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GenerateMaze : MonoBehaviour
{
  public GameObject Wall;
  public int width = 7, height = 7;
  static int N = 1, S = 2, E = 4, W = 8;
  Dictionary<int, int> DX = new Dictionary<int, int>(){
    { E , 1 }, { W, -1 } , { N, 0 } , { S, 0 }
  };

  Dictionary<int, int> DY = new Dictionary<int, int>(){
    { E , 0 }, { W, 0 } , { N, -1 } , { S, 1 }
  };

  Dictionary<int, int> OPPOSITE = new Dictionary<int, int>(){
    { E , W }, { W, E } , { N, S } , { S, N }
  };

  int[,] grid;

  int [] getRandomDirections () {
    int [] directions = new int[] { N, S, E, W };

    for (int t = 0; t < directions.Length; t++ ) {
        int tmp = directions[t];
        int r = Random.Range(t, directions.Length);
        directions[t] = directions[r];
        directions[r] = tmp;
    }

    return directions;
  }

  bool between (int value, int min, int max) {
    return value >= min && value <= max;
  }
  void CarvePassagesFrom (int cx, int cy, int [,] grid) {
    int [] directions = getRandomDirections();

    int direction = 0;

    for (int i = 0; i < directions.Length; i ++) {
      direction = directions[i];
      int nx = cx + DX[direction];
      int ny = cy + DY[direction];

      if (between(ny, 0, height - 1) && between(nx, 0, width -1) && grid[ny, nx] == 0) {
        grid[cy, cx] = grid[cy, cx] | direction;
        grid[ny, nx] = grid[ny, nx] | OPPOSITE[direction];
        CarvePassagesFrom(nx, ny, grid);
      }
    }
  }

  void BuildWall (Vector3 i, Vector3 j) {
    Quaternion rotation = Quaternion.Euler(0, i.x == j.x ? 90 : 0, 0);
    Instantiate(Wall, new Vector3(i.x + ((j.x - i.x) / 2), 0.5f, i.z + ((j.z - i.z) / 2)), rotation, transform);
  }

  void BuildVertical (int pos) {
    BuildWall(vertices[pos], vertices[pos + width + 1]);
  }

  void BuildHorizontal (int pos) {
    BuildWall(vertices[pos], vertices[pos + 1]);
  }
  void BuildMazeWalls () {
    for (int i = 0; i < width; i++) {
      BuildWall(vertices[i], vertices[i + 1]);
    }

    int vi = 0; // vertice index
    int c = width + 1;

    for (int y = 0; y < height ; y ++) {
      vi = y * (width + 1);
      BuildVertical(vi);
     
      for (int x = 0; x < width ; x ++) {
        if ((grid[y, x] & S) != 0) {
          // space
        } else {
          BuildHorizontal(vi + c + x);
        }

        if ((grid[y, x] & E) != 0) {
          if (((grid[y, x] | grid[y, x+1]) & S) != 0) {
            // space
          } else {
            BuildHorizontal(vi + c + x);
          }
        } else {
          BuildVertical(vi + x + 1);
        }
      }
    }
  }
  void printMaze () {
    string text = "";

    text += " ";

    for (int i = 0; i < width * 2 - 1 ; i ++) {
      text += "_";
    }

    text += "\n";

    for (int y = 0; y < height ; y ++) {
      text += "|";

      for (int x = 0; x < width ; x ++) {
        text += (grid[y, x] & S) != 0 ? " " : "_";
        if ((grid[y, x] & E) != 0) {
          text += ((grid[y, x] | grid[y, x+1]) & S) != 0 ? " " : "_";
        } else {
          text += "|";
        }
      }

      text += "\n";
    }

    Debug.Log(text);
  }

  private Vector3[] vertices;
  private Mesh mesh;
	private void Generate () {
    GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";

		vertices = new Vector3[ (height + 1) * (width + 1)];

    for (int i = 0, y = 0; y <= height; y++) {
			for (int x = 0; x <= width; x++, i++) {
				vertices[i] = new Vector3(y, 0, x);
			}
		}

    int total = width * height;
    int[] triangles = new int[6 * total];

    int index = 0;
    int count = 0;

    while (count < total) {
      if ((index + 1) % (width + 1) != 0) {
        FillRect(index, count, ref triangles);
        count++;
      }

      index ++;
    }

    mesh.vertices = vertices;
    mesh.triangles = triangles;
	}

  void FillRect (int index, int count, ref int [] triangles) {
    int p = count * 6;
    
    triangles[p] = index;
		triangles[p + 1] = index + 1;
		triangles[p + 2] = index + width + 1;
    triangles[p + 3] = triangles[p + 1];
		triangles[p + 4] = index + width + 2;
		triangles[p + 5] = triangles[p + 2];
  }

  void OnDrawGizmos() {
    if (vertices == null) {
			return;
		}

		Gizmos.color = Color.black;

		for (int i = 0; i < vertices.Length; i++) {
			Gizmos.DrawSphere(vertices[i], 0.1f);
		}
	}

  void Awake () {
		Generate();
	}

  void Start() {
    // Populate grid
    grid = new int[height, width];

    for (int i = 0; i < height; i++) {
      for (int j = 0; j < width; j++) {
        grid[i, j] = 0;
      }
    }

    CarvePassagesFrom(0, 0, grid);
    printMaze();
    BuildMazeWalls();
  }

  void Update()
  {
      
  }
}
