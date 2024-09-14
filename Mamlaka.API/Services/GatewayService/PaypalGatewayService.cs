using PayPal.Api;
using Mamlaka.API.Helpers;
using Mamlaka.API.DAL.Models;

namespace Mamlaka.API.Services.GatewayService;
public class PaypalGatewayService
{
    private readonly string _mode; 
    private readonly string _clientId;
    private readonly string _clientSecret;
    private IdRefGenerator _keyGenerator;
    public PaypalGatewayService(IConfiguration configuration)
    {
        IConfigurationSection paypalConfig = configuration.GetSection("PayPal");
        _mode = paypalConfig["Mode"];
        _clientId = paypalConfig["ClientId"];
        _keyGenerator = new IdRefGenerator();
        _clientSecret = paypalConfig["ClientSecret"];
    }

    private APIContext GetApiContext()
    {
        Dictionary<string, string> config = new Dictionary<string, string>
        {
            { "mode", _mode }
        };

        string accessToken = new OAuthTokenCredential(_clientId, _clientSecret, config).GetAccessToken();
        return new APIContext(accessToken) { Config = config };
    }

    public Payment CreatePayment(string baseUrl, PaymentModel paymentModel,  string intent = "sale" )
    {
        var apiContext = GetApiContext();

        var payer = new Payer() { payment_method = "paypal" };

        var redirectUrls = new RedirectUrls()
        {
            cancel_url = $"{baseUrl}/paypal/cancel",
            return_url = $"{baseUrl}/paypal/success"
        };

        var details = new Details()
        {
            tax = paymentModel.Tax,
            shipping = paymentModel.Shipping,
            subtotal = paymentModel.SubTotal
        };

        var amount = new Amount()
        {
            currency = paymentModel.Currency,
            total = paymentModel.Total.ToString(), // Total must be the sum of shipping, tax, and subtotal.
            details = details
        };

        var transactionList = new List<Transaction>();



        transactionList.Add(new Transaction()
        {
            description = paymentModel.TransactionDescription,
            invoice_number = _keyGenerator.KeyGenerator("INV-", 6), // Generate a unique invoice number for each order.
            amount = amount           
        });

        var payment = new Payment()
        {
            intent = intent, // Can be "sale", "authorize", or "order".
            payer = payer,
            transactions = transactionList,
            redirect_urls = redirectUrls
        };

        return payment.Create(apiContext);
    }

    public Payment ExecutePayment(string paymentId, string payerId)
    {
        var apiContext = GetApiContext();
        var paymentExecution = new PaymentExecution() { payer_id = payerId };
        var payment = new Payment() { id = paymentId };

        return payment.Execute(apiContext, paymentExecution);
    }

}
