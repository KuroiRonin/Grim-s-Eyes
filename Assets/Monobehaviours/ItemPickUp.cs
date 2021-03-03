using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    public ItemPickUp itemDefinition;
    public Sprite itemIcon;


    CharacterInventory charInventory;

    GameObject foundStats;
    internal bool isStackable;
    internal bool isIndestructable;

    #region Constructors
    public ItemPickUp()
    {
        charInventory = CharacterInventory.instance;
    }
    #endregion

    void Start()
    {
        foundStats = GameObject.FindGameObjectWithTag("Player");
    
    }

    void StoreItem()
    {
        charInventory.StoreItem(this);
    }

    public void UseItem()
    {
        
    }

    public bool isStorable()
    {

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (itemDefinition.isStorable() )
            {
                StoreItem();
            }
            else
            {
                UseItem();
            }
        }
    }
}
