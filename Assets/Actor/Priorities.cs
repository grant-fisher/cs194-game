using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Priorities
{
  public enum PriorityTypes
  {
    aggressive,
    passive,
    survivalist
  };

  public Dictionary<PriorityTypes, double> PriorityWeights;

  public Priorities()
  {
    PriorityWeights[PriorityTypes.aggressive] = 0.3;
    PriorityWeights[PriorityTypes.passive] = 0.4;
    PriorityWeights[PriorityTypes.survivalist] = 0.3;
  }

}
