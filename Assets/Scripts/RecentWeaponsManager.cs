using System.Collections.Generic;
using UnityEngine;

public class RecentWeaponsManager : MonoBehaviour
{
    public static RecentWeaponsManager Instance { get; private set; }

    private const string RECENT_WEAPONS_KEY = "RecentWeapons";
    private const int MAX_RECENT_WEAPONS = 9;

    private List<string> recentWeaponIDs = new List<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            LoadRecentWeapons();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddRecentWeapon(WeaponData weapon)
    {
        if (weapon == null)
            return;

        string weaponID = weapon.name;

        recentWeaponIDs.Remove(weaponID);

        recentWeaponIDs.Insert(0, weaponID);

        if (recentWeaponIDs.Count > MAX_RECENT_WEAPONS)
        {
            recentWeaponIDs.RemoveAt(recentWeaponIDs.Count - 1);
        }

        SaveRecentWeapons();
    }

    public List<WeaponData> GetRecentWeapons()
    {
        List<WeaponData> weapons = new List<WeaponData>();
        
        foreach (string weaponID in recentWeaponIDs)
        {
            WeaponData weapon = Resources.Load<WeaponData>($"Weapons/{weaponID}");
            if (weapon != null)
            {
                weapons.Add(weapon);
            }
        }

        return weapons;
    }

    private void SaveRecentWeapons()
    {
        string serializedData = string.Join(",", recentWeaponIDs);
        PlayerPrefs.SetString(RECENT_WEAPONS_KEY, serializedData);
        PlayerPrefs.Save();
    }

    private void LoadRecentWeapons()
    {
        if (PlayerPrefs.HasKey(RECENT_WEAPONS_KEY))
        {
            string serializedData = PlayerPrefs.GetString(RECENT_WEAPONS_KEY);
            if (!string.IsNullOrEmpty(serializedData))
            {
                recentWeaponIDs = new List<string>(serializedData.Split(','));
            }
        }
    }
}
