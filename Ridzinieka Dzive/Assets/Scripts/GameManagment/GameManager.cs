using UnityEngine;

public class GameManager : MonoBehaviour
{
    // stores the game data, add any extra data you wan't to store'
    
    public int money;
    public int hunger = 100;
    public int energy = 100;
    public int stress = 0;
    public int health = 100;
    
    public float morning;
    public float day;
    public float evening;
    public float night;

    public GameObject[] calendarDay;
    
    public bool oldBike = false;
    public bool newBike = false;
    public bool oldCar = false;
    public bool newCar = false;
    
}

