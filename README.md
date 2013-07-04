InAppPurchase
=============

Description
-----------

A abstract layer for In-App purchase to be used for Monotouch.iOS, Monotouch.Android, Windows Phone and Windows Store applications.
This is a work in progress and only the iOS version is implemented.

Usage
-----
    
    // Initialize Product Ids
    InAppPurchaseManager.Shared.AddProductIdentifier("com.company.productone");
    InAppPurchaseManager.Shared.AddProductIdentifier("com.company.producttwo");
    InAppPurchaseManager.Shared.Initialize();

    // When the user clicks the buy button
    InAppPurchaseManager.Shared.PurchaseSucceed += (product, quantity) => 
    {
        // Enable funcionality, eg.
    };

    InAppPurchaseManager.Shared.PurchaseFailed += (error) => 
    {
        // Display error message
    };    
    InAppPurchaseManager.Shared.PurchaseProduct("com.company.producttwo");


Licensing
---------

Copyright 2013 Sleiman Zublidi

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
