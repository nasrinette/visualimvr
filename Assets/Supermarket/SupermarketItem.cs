using UnityEngine;

public class SupermarketItem : MonoBehaviour
{
    public enum ItemType
    {
        Tomato,
        BellPepper,
        Eggplant,
        Zucchini
    }
    
    [Header("Item Info")]
    public ItemType itemType;
    
    [Header("Visual")]
    public Color actualColor; // The "real" color (for debugging/reference)
    
    void Start()
    {
        // Ensure the item has a Rigidbody for physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Make sure collider exists
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"{gameObject.name} needs a collider for collision detection!");
        }
    }
}
