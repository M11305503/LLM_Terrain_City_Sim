using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CityEvolutionParams
{
    [Range(0, 0.5f)]
    public float baseHighriseProb = 0.10f;
    [Range(0, 0.5f)]
    public float baseIndustrialProb = 0.05f;
    [Range(0, 0.2f)]
    public float highriseClusterBoost = 0.10f;
    [Range(0, 0.2f)]
    public float industrialClusterBoost = 0.08f;
    [Range(0, 0.5f)]
    public float parkSpreadProb = 0.08f;
    


}
