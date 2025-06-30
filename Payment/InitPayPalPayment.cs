using System;
using System.Linq;
using AndroidX.Fragment.App;
using Com.Braintreepayments.Api;
using WoWonder.Activities.Tabbes;
using WoWonder.Helpers.Utils;
using WoWonder.Payment.Utils;
using Exception = Java.Lang.Exception;
using Object = Java.Lang.Object;

namespace WoWonder.Payment
{
    public class InitPayPalPayment : Object, IPayPalListener 
    {
        private readonly FragmentActivity ActivityContext;
        private readonly TabbedMainActivity GlobalContext;
        public string Price;
        private BraintreeClient BraintreeClient;
        private PayPalClient PayPalClient;
          
        /// <summary>
        /// https://github.com/braintree/braintree_android/blob/master/v4_MIGRATION_GUIDE.md
        /// </summary>
        /// <param name="activity"></param>
        public InitPayPalPayment(FragmentActivity activity)
        {
            try
            {
                ActivityContext = activity;
                GlobalContext = TabbedMainActivity.GetInstance();
                ConfigureDropInClient();
            }
            catch (System.Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Paypal
        public void BtnPaypalOnClick(string price)
        {
            try
            {
                Price = price;
                 
                var dropInRequest = InitPayPal();
                if (dropInRequest == null)
                    return;

                PayPalClient.TokenizePayPalAccount(ActivityContext, dropInRequest);
            }
            catch (System.Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private void ConfigureDropInClient()
        {
            try
            {  
                // DropInClient can also be instantiated with a tokenization key
                switch (ListUtils.SettingsSiteList?.PaypalMode)
                {
                    case "sandbox":
                        BraintreeClient = new BraintreeClient(ActivityContext, AppSettings.SandboxTokenizationKey);
                        break;
                    case "live":
                        BraintreeClient = new BraintreeClient(ActivityContext, AppSettings.ProductionTokenizationKey);
                        break;
                    default:
                        BraintreeClient = new BraintreeClient(ActivityContext, AppSettings.SandboxTokenizationKey);
                        break;
                }
                //Make sure to register listener in onCreate
                PayPalClient = new PayPalClient(BraintreeClient);
                PayPalClient.SetListener(this); 
            }
            catch (System.Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        private PayPalCheckoutRequest InitPayPal()
        {
            try
            {
                var option = ListUtils.MyProfileList.FirstOrDefault();

                var currency = ListUtils.SettingsSiteList?.Currency ?? "USD";

                //Address
                PostalAddress billingAddress = new  PostalAddress();
                billingAddress.RecipientName = "Jill Doe"; 
                billingAddress.PhoneNumber = "5551234567";
                billingAddress.StreetAddress = "555 Smith St";
                billingAddress.ExtendedAddress = "#2";
                billingAddress.Locality = "Chicago";
                billingAddress.Region = "IL";
                billingAddress.PostalCode = "12345";
                billingAddress.CountryCodeAlpha2 = "US";
                 
                //PayPal Request
                PayPalCheckoutRequest paypalRequest = new PayPalCheckoutRequest(Price);
                paypalRequest.CurrencyCode = currency;
                paypalRequest.MerchantAccountId = AppSettings.MerchantAccountId;
                paypalRequest.DisplayName = AppSettings.ApplicationName;
                paypalRequest.BillingAgreementDescription = "Add to balance";
                //paypalRequest.LandingPageType = ("billing");
                paypalRequest.Intent = IPayPalPaymentIntent.Authorize;
                paypalRequest.ShippingAddressOverride = billingAddress;
                 
                return paypalRequest;
            }
            catch (System.Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }
           
        public static void DisplayResult(PayPalAccountNonce paypalAccountNonce)
        {
            try
            { 
                string details = "";
                
                details = "First name: " + paypalAccountNonce.FirstName + "\n";
                details += "Last name: " + paypalAccountNonce.LastName + "\n";
                details += "Email: " + paypalAccountNonce.Email + "\n";
                details += "Phone: " + paypalAccountNonce.Phone + "\n";
                details += "Payer id: " + paypalAccountNonce.PayerId + "\n";
                details += "Client metadata id: " + paypalAccountNonce.ClientMetadataId + "\n";
                details += "Billing address: " + paypalAccountNonce.BillingAddress + "\n";
                details += "Shipping address: " + paypalAccountNonce.ShippingAddress;

                Console.WriteLine(details);
            }
            catch (System.Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
         
        /// <summary>
        /// handle error
        /// </summary>
        /// <param name="error"></param>
        public void OnPayPalFailure(Exception error)
        {
           
        }
         
        /// <summary>
        /// use the result to update your UI and send the payment method nonce to your server
        /// </summary>
        /// <param name="result"></param>
        public void OnPayPalSuccess(PayPalAccountNonce result)
        {
            try
            {
                DisplayResult(result);
                // google pay doesn't have a payment method nonce to display; fallback to OG ui
                if (ActivityContext is PaymentBaseActivity activity)
                {
                    activity.TopWallet();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        } 
    }
}