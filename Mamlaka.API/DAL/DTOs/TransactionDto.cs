using Mamlaka.API.DAL.Entities;
using Mamlaka.API.DAL.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mamlaka.API.DAL.DTOs;
public class TransactionDto
{
    public long Id { get; set; }   
    public string UserId { get; set; }
    public string UserFullName { get; set; }
    public decimal Amount { get; set; }
    public string TransactionRefId { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public string TransactionDate { get; set; }
    public string CreatedAt { get; set; }
    public string ModifiedAt { get; set; }
    public string ModifiedBy { get; set; }
}
