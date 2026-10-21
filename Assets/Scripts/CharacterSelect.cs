using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterData
{
    public string name;
    public GameObject model;
    public int price;
    public bool isUnlocked;
}


public class CharacterSelect : MonoBehaviour
{
    [Header("Characters")]
    public CharacterData[] data;




}
