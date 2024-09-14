using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;

using Mamlaka.API.Paging;
using Mamlaka.API.DAL.DTOs;
using Mamlaka.API.DAL.Enums;
using Mamlaka.API.Attributes;
using Mamlaka.API.Interfaces;
using Mamlaka.API.Exceptions;
using Mamlaka.API.CommonObjects.Requests;
using Mamlaka.API.Services.GatewayService;
using Mamlaka.API.DAL.Entities.Transactions;
using Mamlaka.API.DAL.Models;

namespace Mamlaka.API.Controllers;

[
    ApiController,
    Route("api/transactions"),
    SwaggerOrder("B"),
    EnableCors("CorsPolicy")
]
#nullable enable
public class TransactionController : ControllerBase
{
    private readonly ITransactionPagination _paginationRepository;
    private readonly ITransactionRepository _transactionRepository;
    public TransactionController(
        ITransactionPagination paginationRepository,
        ITransactionRepository transactionRepository
        )
    {
        _paginationRepository = paginationRepository;
        _transactionRepository = transactionRepository; 
    }

    /// <summary>
    /// add new transaction to the system
    /// </summary>
    /// <param name="request"></param>
    /// <remarks>
    /// request format:
    ///    POST /api/transactions/new
    ///    {
    ///         "userId": "wywtyr-wrwuwur-wurgwurgu-wyewuy",
    ///         "transactionRefId": "TRAREF83893",
    ///         "amount": "4500",
    ///         "modifiedBy": "Shawn Mbuvi"
    ///    }
    /// </remarks>
    /// <returns></returns>
    [HttpPost, Route("new"), AllowAnonymous]
    [Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> AddNewTransaction([FromBody, Required] TransactionRequest request)
    {
        if (request is null)
            return BadRequest();
        //validate status
        if (!Enum.IsDefined(typeof(TransactionStatus), request.TransactionStatus))
        {
            throw new CustomException($"transaction status : {request.TransactionStatus} is not pre-defined!", "ERROR412", HttpStatusCode.PreconditionFailed);
        }

        return Ok(await _transactionRepository.CreateTransaction(request));
    }

    /// <summary>
    /// third party paypal payment endpoint
    /// </summary>
    /// <param name="paymentModel"></param>
    /// <returns></returns>
    [HttpPost, Route("paypal-payment"), AllowAnonymous]
    [Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
    public IActionResult AddNewPaypalTransaction([FromBody, Required] PaymentModel paymentModel)
    {
        if (paymentModel is null)
            return BadRequest();

        string baseUrl = $"{Request.Scheme}://{Request.Host}";

        return Ok(_transactionRepository.CreatePaypalPayment(paymentModel, baseUrl));
    }

    /// <summary>
    /// execute payment and return response, determine if it went through
    /// </summary>
    /// <param name="paymentId"></param>
    /// <param name="payerID"></param>
    /// <returns></returns>
    [HttpPost, Route("execute-paypal-payment"), AllowAnonymous]
    [Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
    public IActionResult ExcecutePaypalTransaction([FromQuery, Required] string paymentId, string payerID)
    {
        return Ok(_transactionRepository.ExcecutePaypalPayment(paymentId, payerID));
    }

    /// <summary>
    /// get the list of all transactions
    /// </summary>
    /// <returns></returns>
    [HttpGet("list")]
    public async Task<IActionResult> GetAllTransactions()
    {
        return Ok(await _transactionRepository.GetTransactionsList());
    }

    /// <summary>
    /// get specific transction by Id
    /// </summary>
    /// <param name="transactionId"></param>
    /// <returns></returns>
    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetTransactionById(long transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId.ToString())) return BadRequest("transactionId must be provided.");
        return Ok(await _transactionRepository.GetTransaction(transactionId));
    }

    /// <summary>
    /// return book: update book loan details with status returned true
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPut("update")]
    [Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> UpdateTransaction([FromBody] TransactionEditRequest request)
    {
        Transaction? transaction = await _transactionRepository.GetTransaction(request.TransactionId);
        if (transaction is null)
            return BadRequest("transaction not found.");
        return Ok(await _transactionRepository.UpdateTransaction(request, transaction));
    }

    /// <summary>
    /// delete transaction entry from the database
    /// </summary>
    /// <param name="transactionId"></param>
    /// <returns></returns>
    [HttpDelete("delete/{transactionId}")]
    [Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> DeleteTransaction(long transactionId)
    {
        Transaction? transaction = await _transactionRepository.GetTransaction(transactionId);
        if (transaction is null)
            return NotFound("transaction entry not found.");
        return Ok(await _transactionRepository.DeleteTransaction(transaction));
    }

    #region PAGINATON
   
    /// <summary>
    /// return list of paged loan repayments
    /// </summary>
    /// <param name="pagingParameters"></param>
    /// <returns></returns>
    [HttpGet("paged-list")]
    public IActionResult GetPagedTransactionData([FromQuery] PagingParameters pagingParameters)
    {
        PagedList<object> itemList = _paginationRepository.GetPageTransactionsList(pagingParameters);
        PaginationDto pagedTransactions = new PaginationDto
        {
            TotalCount = itemList.TotalCount,
            PageSize = itemList.PageSize,
            CurrentPage = itemList.CurrentPage,
            TotalPages = itemList.TotalPages,
            HasNext = itemList.HasNext,
            HasPrevious = itemList.HasPrevious,
            Data = itemList
        };
        return Ok(pagedTransactions);
    }
    #endregion PAGINATON
}
