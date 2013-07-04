//
//   Copyright 2013 Sleiman Zublidi
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//

using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.StoreKit;

namespace InAppPurchase
{
    [Preserve]
    public sealed class InAppPurchaseManager : InAppPurchaseManagerBase, IInAppPurchaseManager
    {
        #region Singleton

        private static IInAppPurchaseManager instance = new InAppPurchaseManager();
        public static IInAppPurchaseManager Shared
        {
            get { return instance; }
        }

        #endregion    

        #region Properties

        public override bool IsSupported
        {
            get { return true; }
        }

        public override bool CanMakePayments
        {
            get { return SKPaymentQueue.CanMakePayments; }
        }

        #endregion

        #region ProductInformation

        private class SKProductInformation : ProductInformation
        {
            public SKProductInformation(SKProduct product)
                : base(product.ProductIdentifier)
            {
                Product = product;
            }

            public SKProduct Product { get; set; }
        }

        private SKProductsRequest productsRequest;
        private ProductsRequestDelegate productsRequestDelegate;

        public override void RequestProductInformation(string[] productIds, ProductInformationDelegate onSucceed, RequestFailedDelegate onFailed)
        {   
            if (productIds == null || productIds.Length == 0)
                throw new ArgumentException("InAppPurchaseManager: At least one product id is required.");

            var products = new NSString[productIds.Length];
            for (int i = 0; i < productIds.Length; i++)
            {
                products[i] = new NSString(productIds[i]);
            }
            NSSet productIdentifiers = NSSet.MakeNSObjectSet<NSString>(products);

            if (productsRequestDelegate != null)
            {
                productsRequestDelegate.Dispose();
                productsRequestDelegate = null;
            }
            productsRequestDelegate = new ProductsRequestDelegate(onSucceed, onFailed);

            if (productsRequest != null)
            {
                productsRequest.Dispose();
                productsRequest = null;
            }

            productsRequest = new SKProductsRequest(productIdentifiers);
            productsRequest.Delegate = productsRequestDelegate;
            productsRequest.Start();
        }

        private class ProductsRequestDelegate : SKProductsRequestDelegate
        {
            private ProductInformationDelegate onSucceed;
            private RequestFailedDelegate onFailed;

            public ProductsRequestDelegate(ProductInformationDelegate onSucceed, RequestFailedDelegate onFailed)
            {
                this.onSucceed = onSucceed;
                this.onFailed = onFailed;
            }

            public override void ReceivedResponse(SKProductsRequest request, SKProductsResponse response)
            {
                if (response == null && onFailed != null)
                {
                    onFailed(new InAppPurchaseException("Invalid response", 0));
                    Console.WriteLine("InAppPurchaseManager: ReceivedResponse: SKProductsResponse is null!");
                }
                else
                {
                    List<ProductInformation> products = new List<ProductInformation>();

                    foreach (SKProduct product in response.Products)
                    {
                        SKProductInformation pi = new SKProductInformation(product);
                        pi.Title = product.LocalizedTitle;
                        pi.Description = product.Description;
                        pi.Price = product.Price.FloatValue;
                        pi.LocalizedPrice = product.LocalizedPrice();
                        pi.IsDownloadable = product.Downloadable;
                        pi.DownloadContentVersion = product.DownloadContentVersion;
                        products.Add(pi);
                    }

                    if (onSucceed != null)
                    {
                        onSucceed(products.ToArray(), response.InvalidProducts);
                    }
                }
            }

            public override void RequestFailed(SKRequest request, NSError error)
            {
                if (onFailed != null)
                {
                    if (error != null)
                    {
                        onFailed(new InAppPurchaseException(error.LocalizedDescription, error.Code));
                        Console.WriteLine("InAppPurchaseManager: RequestFailed: {0}", error.LocalizedDescription);
                    }
                    else
                    {
                        onFailed(new InAppPurchaseException("Uknown Error", 0));
                        Console.WriteLine("InAppPurchaseManager: RequestFailed: NSError is null!");
                    }
                }
            }
        }

        #endregion

        #region Initialization

        protected override void Initialize(string[] productIds, InitializatedDelegate onInitialized, InitializationFailedDelegate onFailed)
        {
            if (productIds == null || productIds.Length == 0)
                throw new ArgumentException("InAppPurchaseManager: At least one product id is required.");

            IsInitalized = false;

            RequestProductInformation(productIds, (products, invalidProductIds) =>
            {
                foreach (var product in products) 
                {
                    productDictionary[product.ProductId] = product;
                }

                foreach (var id in invalidProductIds) 
                {
                    Console.WriteLine("InAppPurchaseManager: Product id '{0}' is not valid.", id);
                    productDictionary[id] = new InvalidProductInformation(id);
                }

                if (productDictionary.Count == 0)
                {
                    OnInitializationFailed(new Exception("InAppPurchaseManager: There are not valid products."));
                }
                else
                {
                    IsInitalized = true;
                    if (onInitialized != null) onInitialized();
                }
            }, 
            ex => 
            { 
                if (onFailed != null) onFailed(ex); 
            });
        }

        #endregion

        #region Purchasing

        private PaymentTransactionObserver observer = null;

        public override void PurchaseProduct(string productId, int quantity)
        {
            if (String.IsNullOrWhiteSpace(productId))
                throw new ArgumentNullException("InAppPurchaseManager: Product id is not valid.");

            if (productDictionary.Keys.Contains(productId) == false)
                throw new InvalidOperationException("InAppPurchaseManager: Product id not found in current product list.");

            if (quantity <= 0)
                throw new ArgumentException("InAppPurchaseManager: Quantity must be greater than zero.");

            if (IsSupported == false)
                throw new NotSupportedException("InAppPurchaseManager: In-app purchases not supported by OS.");

            if (CanMakePayments == false)
                throw new InvalidOperationException("InAppPurchaseManager: In-app purchases is disabled.");

            if (IsInitalized == false) 
            {
                OnPurchaseFailed(new InAppPurchaseException("InAppPurchaseManager is not initialized." , 0));
                return;
            }

            SKProductInformation pi = productDictionary[productId] as SKProductInformation;
            if (pi == null || pi.Product == null)
            {
                OnPurchaseFailed(new InAppPurchaseException("Product is not valid." , 0));
            }
            else
            {
                PurchaseProduct(pi.Product, quantity);
            }
        }

        public override void RestorePurshases()
        {
            if (observer == null)
            {
                observer = new PaymentTransactionObserver(this);
                SKPaymentQueue.DefaultQueue.AddTransactionObserver(observer);
            }

            SKPaymentQueue.DefaultQueue.RestoreCompletedTransactions();
        }

        private void PurchaseProduct(SKProduct product, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException("InAppPurchaseManager: Product is not valid.");

            if (observer == null)
            {
                observer = new PaymentTransactionObserver(this);
                SKPaymentQueue.DefaultQueue.AddTransactionObserver(observer);
            }

            if (quantity > 1)
            {
                SKMutablePayment payment = SKMutablePayment.PaymentWithProduct(product);
                payment.Quantity = quantity;
                SKPaymentQueue.DefaultQueue.AddPayment(payment);
            }
            else
            {
                SKPayment payment = SKPayment.PaymentWithProduct(product);
                SKPaymentQueue.DefaultQueue.AddPayment(payment);
            }

            Console.WriteLine("");
        }


        private void CompleteTransaction(SKPaymentTransaction transaction)
        {
            FinishTransaction(transaction, true);
        }

        private void RestoreTransaction(SKPaymentTransaction transaction)
        {
            FinishTransaction(transaction, true);
        }

        private void FailedTransaction(SKPaymentTransaction transaction)
        {
            if (transaction.Error.Code == 2)
            {
                OnPurchaseFailed(new InAppPurchaseException("Payment was CANCELLED.", transaction.Error.Code));
            }
            else
            {
                OnPurchaseFailed(new InAppPurchaseException(transaction.Error.LocalizedDescription, transaction.Error.Code));
            }
            FinishTransaction(transaction, false);
        }

        private void FinishTransaction(SKPaymentTransaction transaction, bool wasSuccessful)
        {
            if (transaction == null)
                throw new ArgumentNullException("InAppPurchaseManager: Transaction is not valid.");

            SKPaymentQueue.DefaultQueue.FinishTransaction(transaction);  // THIS IS IMPORTANT - LET'S APPLE KNOW WE'RE DONE !!!!
            
            if (wasSuccessful)
            {
                string productId = transaction.Payment.ProductIdentifier;
                int quantity = transaction.Payment.Quantity;
            
                if (transaction.OriginalTransaction != null)
                {
                    productId = transaction.OriginalTransaction.Payment.ProductIdentifier;
                    quantity = transaction.OriginalTransaction.Payment.Quantity;
                }
            
                IProductInformation product = productDictionary [productId];
                OnPurchaseSucceed(product, quantity);
            }
        }

        private class PaymentTransactionObserver : SKPaymentTransactionObserver
        {
            private InAppPurchaseManager manager;

            public PaymentTransactionObserver(InAppPurchaseManager manager)
            {
                this.manager = manager;
            }

            public override void UpdatedTransactions(SKPaymentQueue queue, SKPaymentTransaction[] transactions)
            {
                foreach (SKPaymentTransaction transaction in transactions)
                {
                    switch (transaction.TransactionState)
                    {
                        case SKPaymentTransactionState.Purchased:
                            manager.CompleteTransaction(transaction);
                            break;

                            case SKPaymentTransactionState.Restored:
                            manager.RestoreTransaction(transaction);
                            break;

                            case SKPaymentTransactionState.Failed:
                            manager.FailedTransaction(transaction);
                            break;

                            default:
                            break;
                    }
                }
            }

            public override void PaymentQueueRestoreCompletedTransactionsFinished(SKPaymentQueue queue)
            {

            }

            public override void RestoreCompletedTransactionsFailedWithError(SKPaymentQueue queue, NSError error)
            {

            }
        } 

        #endregion
    }

    public static class SKProductExtension 
    {
        public static string LocalizedPrice(this SKProduct product)
        {
            var formatter = new NSNumberFormatter();
            formatter.FormatterBehavior = NSNumberFormatterBehavior.Version_10_4;  
            formatter.NumberStyle = NSNumberFormatterStyle.Currency;
            formatter.Locale = product.PriceLocale;
            return formatter.StringFromNumber(product.Price);
        }
    }
}

