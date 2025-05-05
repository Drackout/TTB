using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;

    [SerializeField] private Transform _camera;


    void Start() {
        GenerateGrid();
    }
 
    void GenerateGrid() {
        for (int x = 0; x < _width; x++) {
            for (int z = 0; z < _height; z++) {
                Tile spawnedTile = Instantiate(_tilePrefab, new Vector3(x, 0, z), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {z}";

                var isOffset = (x%2==0 && z%2!=0) || (x%2!=0 && z%2==0);
                spawnedTile.Init(isOffset);
            }
        }

        _camera.transform.position = new Vector3((float)_width/2, 8, 0.5f);
    }
}
