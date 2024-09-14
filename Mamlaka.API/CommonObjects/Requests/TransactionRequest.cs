using System.ComponentModel.DataAnnotations;

namespace Mamlaka.API.CommonObjects.Requests;

#nullable enable
public class TransactionRequest
{  
    [Required(ErrorMessage = "userId must be provided")]
    public required string UserId { get; set; }

    [Required(ErrorMessage = "transactionRef must be provided")]
    public required string TransactionRefId { get; set; }

    [Required(ErrorMessage = "transactionStatus must be provided")]
    public required string TransactionStatus { get; set; }

    [Required(ErrorMessage = "amount must be provided")]
    public decimal Amount { get; set; }

    public string? ModifiedBy { get; set; }
}
