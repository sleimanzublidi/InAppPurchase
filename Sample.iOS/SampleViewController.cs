using System;
using System.Drawing;
using System.Linq;
using InAppPurchase;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Sample
{
    public class ProductIds
    {
        public static readonly string ProductOne = "com.company.productone";
        public static readonly string ProductTwo = "com.company.producttwo";
    }

    public partial class SampleViewController : UIViewController
    {
        private UITableView tableView;

        public SampleViewController() 
            : base()
        {}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            tableView = new UITableView(View.Frame, UITableViewStyle.Plain);
            tableView.Source = new TableSource();
            View.AddSubview(tableView);

            InAppPurchaseManager.Shared.AddProductIdentifier(ProductIds.ProductOne);
            InAppPurchaseManager.Shared.AddProductIdentifier(ProductIds.ProductTwo);

            InAppPurchaseManager.Shared.PurchaseSucceed += (product, quantity) => 
            {
                UIAlertView alert = new UIAlertView("Purchase Succeed", product.Description, null, "Ok");
                alert.Show();
            };

            InAppPurchaseManager.Shared.PurchaseFailed += (error) => 
            {
                UIAlertView alert = new UIAlertView("Purchase Failed", error.Message, null, "Ok");
                alert.Show();
            };
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            InAppPurchaseManager.Shared.Initialized += delegate 
            {
                tableView.ReloadData();
            };

            InAppPurchaseManager.Shared.Initialize();
        }

        private class TableSource : UITableViewSource
        {
            public TableSource()
            {}

            public override int NumberOfSections(UITableView tableView)
            {
                return 1;
            }

            public override int RowsInSection(UITableView tableview, int section)
            {
                return InAppPurchaseManager.Shared.Products.Length;
            }

            private static readonly NSString CellKey = new NSString("Key");

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell cell = tableView.DequeueReusableCell(CellKey);
                if (cell == null)
                {
                    cell = new UITableViewCell(UITableViewCellStyle.Value1, CellKey);
                    cell.Accessory = UITableViewCellAccessory.DetailDisclosureButton;
                }

                var product = InAppPurchaseManager.Shared.Products[indexPath.Row];

                cell.TextLabel.Text = product.Title;
                cell.DetailTextLabel.Text = product.LocalizedPrice;

                return cell;
            }

            public override void AccessoryButtonTapped(UITableView tableView, NSIndexPath indexPath)
            {
                var product = InAppPurchaseManager.Shared.Products[indexPath.Row];
                InAppPurchaseManager.Shared.PurchaseProduct(product.ProductId);
            }
        }
    }
}

