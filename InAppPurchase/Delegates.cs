using System;

namespace InAppPurchase
{
    public delegate void ProductInformationDelegate(IProductInformation[] products, string[] invalidProducIds);
    public delegate void RequestFailedDelegate(InAppPurchaseException error);

    public delegate void InitializatedDelegate();
    public delegate void InitializationFailedDelegate(Exception error);

    public delegate void PurchaseSucceedDelegate(IProductInformation product, int quantity);
    public delegate void PurchaseFailedDelegate(InAppPurchaseException error);

    public delegate void RestoreSucceedDelegate();
    public delegate void RestoreFailedDelegate(InAppPurchaseException error);
}

