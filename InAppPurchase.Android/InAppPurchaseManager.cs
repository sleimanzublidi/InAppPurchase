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
            get { return true; }
        }

        #endregion
    
        #region ProductInformation

        public override void RequestProductInformation(string[] productIds, ProductInformationDelegate onSucceed, RequestFailedDelegate onFailed)
        {   
            if (productIds == null || productIds.Length == 0)
                throw new ArgumentException("InAppPurchaseManager: At least one product id is required.");

            throw new NotImplementedException();
        }

        #endregion

        #region Initialization

        protected override void Initialize(string[] productIds, InitializatedDelegate onInitialized, InitializationFailedDelegate onFailed)
        {
            if (productIds == null || productIds.Length == 0)
                throw new ArgumentException("InAppPurchaseManager: At least one product id is required.");

            throw new NotImplementedException();
        }

        #endregion

        #region Purchasing

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
            }

            var pi = productDictionary[productId] as ProductInformation;
            if (pi == null)
            {
                OnPurchaseFailed(new InAppPurchaseException("Product is not valid." , 0));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void RestorePurshases()
        {
            throw new NotImplementedException();
        }

        #endregion    
    }
}

