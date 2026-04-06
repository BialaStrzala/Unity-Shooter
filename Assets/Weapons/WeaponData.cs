using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public GameObject modelPrefab;
    public float fireRate = 0.2f;
    public float range = 20f;
    public int damage = 10;
    public float reloadTime = 1.5f;
    public int bullets;
}