using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public enum DrawMode{ NoiseMap,ColorMap, DrawMesh};
    public DrawMode drawMode;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public int mapWidth = 0;
    public int mapHeight = 0;
    public float scale = 0.0f;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public bool autoUpdate = true;
    public TerrainType[] terrainType;
    private int count = 0;

   
    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, scale,octaves,persistance,lacunarity, offset);

        //We construct our heightMap
        float[,] heightMap = new float[mapWidth, mapHeight];


        Color[] colorMap = new Color[mapHeight * mapWidth];
        for(int y = 0; y < mapHeight; y++)
        {
            for(int x=0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for(int i=0;i< terrainType.Length; i++)
                {
                    if (currentHeight < terrainType[i].noiseHeight)
                    {
                        colorMap[y * mapWidth + x] = terrainType[i].color;
                        if (i > 0)
                        {
                            heightMap[x, y] = terrainType[i - 1].gameHeight;
                        }
                        else
                        {
                            heightMap[x, y] = terrainType[0].gameHeight;
                        }
                        
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if(drawMode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap,mapWidth,mapHeight));
        }
        else if (drawMode == DrawMode.DrawMesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap, meshHeightMultiplier, meshHeightCurve), TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
        }
    }

    void Start()
    {

    }

    void Update()
    {
        count++;
        if (count == 5)
            GenerateMap();
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;             //Le nom correspondant
    public float noiseHeight;       //Le niveau du bruit à partir du quel on applique
    public float gameHeight;        //Le niveau correspondant dans le jeu
    public Color color;             //La couleur à appliquer
}
