using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Menu : MonoBehaviour
{
    public abstract string menuName {get; protected set;}
    public string callingMenuName {get; set;}
}
