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

namespace InAppPurchase
{
    public interface IInAppPurchaseManager
    {
        bool IsSupported { get; }
        bool CanMakePayments { get; }
        bool IsInitalized { get; }
        IProductInformation[] Products { get; }

        void AddProductIdentifier(string productId);
        void RequestProductInformation(string productId, ProductInformationDelegate onSucceed, RequestFailedDelegate onFailed);
        void RequestProductInformation(string[] productIds, ProductInformationDelegate onSucceed, RequestFailedDelegate onFailed);

        void Initialize();
        void Initialize(string[] productIds);
        event InitializatedDelegate Initialized;
        event InitializationFailedDelegate InitializationFailed;

        void PurchaseProduct(string productId);
        void PurchaseProduct(string productId, int quantity);
        void RestorePurshases();

        event PurchaseSucceedDelegate PurchaseSucceed;
        event PurchaseFailedDelegate PurchaseFailed;
    }
}

