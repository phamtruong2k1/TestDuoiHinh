using System.Linq;
#if UNITY_IAP
using UnityEngine.Purchasing;
#endif
using System;
using UnityEngine;
using System.Collections.Generic;

//Standart Unity In App Purchase class
public class IAPController : MonoBehaviour
#if UNITY_IAP
,IStoreListener
#endif
{
#if UNITY_IAP

    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

    void Start()
    {
        // If we haven't set up the Unity Purchasing reference
        if (m_StoreController == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }
    }
    public void InitializePurchasing()
    {
        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            // ... we are done here.
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        IEnumerable<ProductDefinition> products = GameController.Instance.InAppProducts.Select(p =>
        {
            PayoutDefinition definition = new PayoutDefinition("coins", p.coinsReward);
            ProductDefinition productDefinition = new ProductDefinition(p.productId, p.productId, ProductType.Consumable, true, definition);
            return productDefinition;
        });
        builder.AddProducts(products);

        UnityPurchasing.Initialize(this, builder);
    }
    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    public void BuyProductID(string productId)
    {
        // If Purchasing has been initialized ...
        if (IsInitialized())
        {
            // ... look up the Product reference with the general product identifier and the Purchasing 
            // system's products collection.
            Product product = m_StoreController.products.WithID(productId);
            // If the look up found a product for this device's store and that product is ready to be sold ... 
            if (product != null && product.availableToPurchase)
            {
                // Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
                // asynchronously.
                m_StoreController.InitiatePurchase(product);
            }
            // Otherwise ...
            else
            {
                // ... report the product look-up failure situation  
                // Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        // Otherwise ...
        else
        {
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            // Debug.Log("BuyProductID FAIL. Not initialized.");
        }
    }

    public void SetupLocalPrices()
    {
        try
        {
            GameController.Instance.InAppProducts = GameController.Instance.InAppProducts.Select(p =>
            {
                p.localPrice = m_StoreController.products.WithID(p.productId).metadata.localizedPriceString;
                return p;
            }).ToArray();
        }
        catch (NullReferenceException ex)
        {
            Debug.LogWarning("Can not get prices from console while in Editor" + "\n" + ex.Message);
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        Debug.LogWarning("OnInitialized: PASS");
        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;
        if (m_StoreController != null)
        {
            SetupLocalPrices();
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        Debug.LogWarning("OnInitializeFailed InitializationFailureReason:" + error);
    }
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {

        GameController.Instance.EarnCoins(Convert.ToInt32(args.purchasedProduct.definition.payout.quantity), true);
        if (GameController.Instance.DisableAdsOnPurchase)
        {
            GameController.Instance.DisableAds();
        }
        // Or ... a non-consumable product has been purchased by this user.
        /* else if (String.Equals(args.purchasedProduct.definition.id, kProductIDNonConsumable, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
            // TODO: The non-consumable item has been successfully purchased, grant this item to the player.
        }*/

        // Return a flag indicating whether this product has completely been received, or if the application needs 
        // to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still 
        // saving purchased products to the cloud, and when that save is delayed. 
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
        // this reason with the user to guide their troubleshooting actions.
        // Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
    }
#endif
}

[Serializable]
public struct InAppProduct
{
    public string productId;
    public int coinsReward;
    public string buttonDescription;

    [HideInInspector]
    public string localPrice;
    public Sprite icon;
}
