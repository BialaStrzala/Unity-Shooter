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
        EquipWeapon(0);
        gameObject.name = isOwner ? "PLAYER_LOCAL" : "PLAYER_REMOTE";
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        activeWeaponIndex.onChanged -= OnWeaponChanged;
    }
    private void Update()
    {
        if(!isOwner){return;}
        //temp
        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            EquipWeapon(0);}
        if(Input.GetKeyDown(KeyCode.Alpha2)) {EquipWeapon(1);}
    }

    private void EquipWeapon(int index)
    {
        activeWeaponIndex.value = index;
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