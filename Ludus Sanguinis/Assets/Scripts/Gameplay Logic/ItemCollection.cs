using System.Collections.Generic;

using UnityEngine;

[System.Serializable]
public class ItemCollection
{
    public ItemSlot[] Slots;


    public void Init()
    {
        foreach (ItemSlot slot in Slots)
        {
            slot.Item.gameObject.SetActive(slot.count > 0);
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
                slot.count++;
                if (slot.count > 0) slot.Item.gameObject.SetActive(true);
            }
        }
    }
    public void RemoveItem(Item item)
    {
        foreach (ItemSlot slot in Slots)
        {
            if (slot.Item.Type == item.Type)
            {
                slot.count--;
                if (slot.count == 0) item.gameObject.SetActive(false);
            }
        }
    }

    public int GetItemCountForItem(Item item)
    {
        foreach (ItemSlot slot in Slots) if (slot.Item.Type == item.Type) return slot.count;
        return 0;
    }

    public List<Item> GetAvailableItems()
    {
        List<Item> itemList = new List<Item>();
        foreach (ItemSlot slot in Slots) if (slot.count > 0) itemList.Add(slot.Item);
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
    public int count;
}

public enum ItemType
{
    Scale,
    Mirror,
    UnoCard,
    Coin,
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
