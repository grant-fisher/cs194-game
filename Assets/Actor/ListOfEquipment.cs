using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * Class: ListOfEquipment
 * ----------------------
 * Maintain the internals of what equipment this actor has at the movements
 */
public class ListOfEquipment
{
  public List<string> Equipment;

  public void Push(string newPiece)
  {
    Equipment.Add(newPiece);
  }
}
