namespace Mamlaka.API.DAL.Models;
public class PaymentModel
{
    public string Currency { get; set; }
    public string Tax { get; set; }
    public string Shipping { get; set; }
    public string SubTotal { get; set; }
    public decimal Total { get; set; }
    public string UserId { get; set; }
    public string TransactionDescription { get; set; }
}
