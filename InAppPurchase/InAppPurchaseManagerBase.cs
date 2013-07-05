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
using System.Text;

namespace InAppPurchase
{        
    public abstract class InAppPurchaseManagerBase : IInAppPurchaseManager
    { 
        #region Properties

        /// <summary>
        /// Returns true if In-App Purchases are supported by the operating system
        /// </summary>
        public virtual bool IsSupported
        {
            get { return false; }
        }

        /// <summary>
        /// Returns true if In-App Purchase payments are possible and are enabled on the device
        /// </summary>
        public virtual bool CanMakePayments
        {
            get { return false; }
        }

        public bool IsInitalized { get; protected set; }

        protected readonly Dictionary<string, IProductInformation> productDictionary = new Dictionary<string, IProductInformation>();
        public IProductInformation[] Products
        {
            get 
            { 
                lock (productDictionary)
                {
                    return productDictionary.Values.Where(v => v != null).ToArray(); 
                }
            }
        }

        #endregion
    
        #region ProductInformation

        protected class ProductInformation : IProductInformation
        {
            public ProductInformation(string productId)
            {
                ProductId = productId;
                Title = String.Empty;
                LocalizedPrice = "0.00";
                DownloadContentVersion = String.Empty;
            }

            public string ProductId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public float Price { get; set; }
            public string LocalizedPrice { get; set; }
            public bool IsDownloadable { get; set; }
            public string DownloadContentVersion { get; set; }
        }

        protected class InvalidProductInformation : ProductInformation
        {
            public InvalidProductInformation(string productId)
                : base(productId)
            {
                Title = "Invalid";
                Description = "Invalid Product";
                LocalizedPrice = "0.00";
                DownloadContentVersion = String.Empty;
            }
        }

        public virtual void RequestProductInformation(string productId, ProductInformationDelegate onSucceed, RequestFailedDelegate onFailed)
        {   
            if (String.IsNullOrWhiteSpace(productId) == true)
                throw new ArgumentException("InAppPurchaseManager: Invalid product id.");

            RequestProductInformation(new string[]{ productId }, onSucceed, onFailed);
        }

        public abstract void RequestProductInformation(string[] productIds, ProductInformationDelegate onSucceed, RequestFailedDelegate onFailed);

        public virtual void AddProductIdentifier(string productId)
        {
            IsInitalized = false;
            if (productDictionary.ContainsKey(productId)) return;
            productDictionary[productId] = new ProductInformation(productId);
        }

        #endregion

        #region Initialization

        public virtual void Initialize()
        {
            if (productDictionary.Count == 0)
                throw new Exception("InAppPurchaseManager: At least one product id is required.");

            string[] productIds = productDictionary.Keys.ToArray();
            Initialize(productIds);
        }

        public virtual void Initialize(string[] productIds)
        {
            Initialize(productIds, OnInitialized, OnInitializationFailed);
        }

        protected abstract void Initialize(string[] productIds, InitializatedDelegate onInitialized, InitializationFailedDelegate onFailed);

        public event InitializatedDelegate Initialized;
        protected void OnInitialized()
        {
            if (Initialized != null)
            {
                Initialized();
            }
        }

        public event InitializationFailedDelegate InitializationFailed;
        protected void OnInitializationFailed(Exception ex)
        {
            if (InitializationFailed != null)
            {
                InitializationFailed(ex);
            }
        }

        #endregion

        #region Purchasing

        public virtual void PurchaseProduct(string productId)
        {
            PurchaseProduct(productId, 1);
        }

        public abstract void PurchaseProduct(string productId, int quantity);

        public event PurchaseSucceedDelegate PurchaseSucceed;
        protected void OnPurchaseSucceed(IProductInformation product, int quantity)
        {
            if (PurchaseSucceed != null)
            {
                PurchaseSucceed(product, quantity);
            }
        }

        public event PurchaseFailedDelegate PurchaseFailed;
        protected void OnPurchaseFailed(InAppPurchaseException ex)
        {
            if (PurchaseFailed != null)
            {
                PurchaseFailed(ex);
            }
        }

        public abstract void RestorePurshases();

        public event RestoreSucceedDelegate RestoreSucceed;
        protected void OnRestoreSucceed()
        {
            if (RestoreSucceed != null)
            {
                RestoreSucceed();
            }
        }

        public event RestoreFailedDelegate RestoreFailed;
        protected void OnRestoreFailed(InAppPurchaseException ex)
        {
            if (RestoreFailed != null)
            {
                RestoreFailed(ex);
            }
        }

        #endregion    
    }
}

