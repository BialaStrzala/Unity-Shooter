using UnityEngine;
using PurrNet;

public class PlayerWeapon : NetworkBehaviour
{
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private WeaponData[] weapons;
    [SerializeField] private Transform cameraTransform;
    public SyncVar<int> activeWeaponIndex = new(0, ownerAuth:true);

    protected override void OnSpawned()
    {
        base.OnSpawned();
        activeWeaponIndex.onChanged += OnWeaponChanged;
        OnWeaponChanged(activeWeaponIndex.value);
        //EquipWeapon(0);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        activeWeaponIndex.onChanged -= OnWeaponChanged;
    }
    private void Update()
    {
        //po dodaniu ui bronie nie naleza do playera ???
        //Debug.Log(isOwner);
        if(!isOwner){return;}
        //temp
        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            Debug.Log("hello");
            EquipWeapon(0);}
        if(Input.GetKeyDown(KeyCode.Alpha2)) {EquipWeapon(1);}
    }

    private void EquipWeapon(int index)
    {
        activeWeaponIndex.value = index;
        /* activeWeaponIndex.value = index;
        for(int i=0; i<weaponHolder.childCount; i++)
        {
            var weaponObj = weaponHolder.GetChild(i).gameObject;
            weaponObj.SetActive(i==index);
            //Debug.Log($"Weapon {i} ? = {i==index}");
        }
        Debug.Log($"=== Weapon {activeWeaponIndex.value} equipped ==="); */
    }

    private void OnWeaponChanged(int newValue)
    {
        for (int i = 0; i < weaponHolder.childCount; i++)
        {
            var weaponObj = weaponHolder.GetChild(i).gameObject;
            weaponObj.SetActive(i == newValue);
        }

        Debug.Log($"[SYNC] Weapon changed to {newValue}");
    }
}