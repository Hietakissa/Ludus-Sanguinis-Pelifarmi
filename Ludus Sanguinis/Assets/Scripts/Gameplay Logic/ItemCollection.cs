using System.Collections.Generic;
using HietakissaUtils;
using UnityEngine;

[System.Serializable]
public class ItemCollection
{
    public ItemSlot[] Slots;


    public void Init()
    {
        foreach (ItemSlot slot in Slots)
        {
            slot.Item.gameObject.SetActive(slot.Count > 0);
            slot.Item.SetTargetTransform(slot.TablePos);
            slot.Item.InstaMoveToTarget();
        }
    }

    public void AddItem(ItemType itemType)
    {
        foreach (ItemSlot slot in Slots)
        {
            if (slot.Item.Type == itemType)
            {
                slot.Count++;
                if (slot.Count > 0) slot.Item.gameObject.SetActive(true);
            }
        }
    }
    public void RemoveItem(ItemType itemType)
    {
        foreach (ItemSlot slot in Slots)
        {
            if (slot.Item.Type == itemType)
            {
                slot.Count--;
                if (slot.Count == 0) slot.Item.gameObject.SetActive(false);
            }
        }
    }
    public void RemoveItem(Item item) => RemoveItem(item.Type);
    public void RemoveItems()
    {
        foreach (ItemSlot slot in Slots) slot.Count = 0;
    }

    public Item GetItem(ItemType itemType)
    {
        foreach (ItemSlot slot in Slots) if (slot.Item.Type == itemType) return slot.Item;
        return null;
    }
    public int GetItemCountForItem(Item item) => GetItemCountForItem(item.Type);
    public int GetItemCountForItem(ItemType itemType)
    {
        foreach (ItemSlot slot in Slots) if (slot.Item.Type == itemType) return slot.Count;
        return 0;
    }

    public List<Item> GetAvailableItems()
    {
        List<Item> itemList = new List<Item>();
        foreach (ItemSlot slot in Slots) if (slot.Count > 0) itemList.Add(slot.Item);
        return itemList;
    }
    
    public List<Item> GetAvailableItems(ItemType[] types)
    {
        List<Item> itemList = new List<Item>();
        foreach (ItemSlot slot in Slots) if (slot.Count > 0 && types.Contains(slot.Item.Type)) itemList.Add(slot.Item);
        return itemList;
    }
}

[System.Serializable]
public class ItemSlot
{
    public Transform TablePos => tablePos;
    [SerializeField] Transform tablePos;

    public Item Item;
    //public ItemType Type;
    public int Count;
}

public enum ItemType
{
    Scale,
    Mirror,
    UnoCard,
    Coupon,
    Hook,
    Heart
}

// scale > use immediately to set scale inaccuracy to 0
// handmirror > use immediately to update card memory
// uno > in gamemanager after both, if used swap cards (not if both players use it)
// coin > use immediately to force-play player cards
// coupon > use immediately to reroll a card
// hook > play immediately to steal item
// heart > play immediately for %chance to dmg
