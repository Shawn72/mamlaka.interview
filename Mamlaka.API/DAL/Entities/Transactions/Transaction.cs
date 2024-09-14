
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mamlaka.API.DAL.Enums;

namespace Mamlaka.API.DAL.Entities.Transactions;

[Index(nameof(Id))]
public class Transaction
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Key]
    [Column(TypeName = "bigint")]
    public long Id { get; set; }

    [ForeignKey("User")]
    [Column(TypeName = "varchar(450)")]
    public virtual string UserId { get; set; }

    [JsonIgnore]
    public User User { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "varchar(450)")]
    public string TransactionRefId { get; set; }

    [Column(TypeName = "varchar(450)")]
    public TransactionStatus TransactionStatus { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime TransactionDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime ModifiedAt { get; set; }

    [Column(TypeName = "mediumtext")]
    public string ModifiedBy { get; set; }
}
