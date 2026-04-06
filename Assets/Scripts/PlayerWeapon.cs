using UnityEngine;
using PurrNet;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private WeaponData[] weapons;
    [SerializeField] private Transform cameraTransform;
    private int currentWeaponIndex;

    private void Update()
    {
        if(!isOwner){return;}
        //temp
        if(Input.GetKeyDown(KeyCode.Alpha1)) {EquipWeapon(0);}
        if(Input.GetKeyDown(KeyCode.Alpha2)) {EquipWeapon(1);}
    }

    private void EquipWeapon(int index)
    {
        currentWeaponIndex = index;
        for(int i = 0; i < weaponHolder.childCount; i++)
        {
            weaponHolder.GetChild(i).gameObject.SetActive(i == index);
        }
        Debug.Log($"Weapon {currentWeaponIndex} equipped");
    }
}