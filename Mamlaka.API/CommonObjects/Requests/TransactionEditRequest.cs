using System.ComponentModel.DataAnnotations;

namespace Mamlaka.API.CommonObjects.Requests;

#nullable enable
public class TransactionEditRequest
{
    [Required(ErrorMessage = "transactionId must be provided")]
    public required long TransactionId { get; set; }

    [Required(ErrorMessage = "amount must be provided")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "transactionStatus must be provided")]
    public required string TransactionStatus { get; set; }

    public string? ModifiedBy { get; set; }
}