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
using Mamlaka.API.DAL.Models;
using Mamlaka.API.CommonObjects.Requests;
using Mamlaka.API.DAL.Entities.Transactions;

namespace Mamlaka.API.Controllers;

[
    ApiController,
    Route("api/payment"),
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
    /// add new payment transaction to the system
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
    /// get the list of all payment transactions
    /// </summary>
    /// <returns></returns>
    [HttpGet("transactions")]
    public async Task<IActionResult> GetAllTransactions()
    {
        return Ok(await _transactionRepository.GetTransactionsList());
    }

    /// <summary>
    /// get specific payment by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTransactionById(long id)
    {
        if (string.IsNullOrWhiteSpace(id.ToString())) return BadRequest("id must be provided.");
        return Ok(await _transactionRepository.GetTransaction(id));
    }

    /// <summary>
    /// update payment transaction
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
    /// delete payment entry from the database
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("delete/{id}"), Authorize(Policy = nameof(AuthPolicy.SuperRights))]
    [Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> DeleteTransaction(long id)
    {
        Transaction? transaction = await _transactionRepository.GetTransaction(id);
        if (transaction is null)
            return NotFound("transaction entry not found.");
        return Ok(await _transactionRepository.DeleteTransaction(transaction));
    }

    #region PAYPAL

    /// <summary>
    /// initiate third party paypal payment
    /// </summary>
    /// <param name="paymentModel"></param>
    /// <returns></returns>
    /// <remarks>
    /// request format
    ///  {
    ///    "currency": "USD",
    ///    "tax": "1.00",
    ///    "shipping": "1.00",
    ///    "subTotal": "2.00",
    ///    "total": 4, //total = (tax + shipping + subTotal) in USD
    ///    "userId": "aadebb5b-39fd-4dee-ae1a-d01f18df833d",
    ///    "transactionDescription": "paypal-test-mbuvi"
    ///  }
    /// </remarks>
    [HttpPost, Route("paypal-payment"), AllowAnonymous]
    [Produces(MediaTypeNames.Application.Json), Consumes(MediaTypeNames.Application.Json)]
    public async Task<IActionResult> AddNewPaypalTransaction([FromBody, Required] PaymentModel paymentModel)
    {
        if (paymentModel is null)
            return BadRequest();

        string baseUrl = $"{Request.Scheme}://{Request.Host}";

        return Ok(await _transactionRepository.CreatePaypalPayment(paymentModel, baseUrl));
    }

    /// <summary>
    /// paypal callback endpoint
    /// </summary>
    /// <param name="paymentId"></param>
    /// <param name="token"></param>
    /// <param name="PayerID"></param>
    /// <returns></returns>
    [HttpGet("paypal/success")]
    public IActionResult PaypalPaymentResponse(string paymentId, string token, string PayerID)
    {
        return Ok(_transactionRepository.ExcecutePaypalPayment(paymentId, token, PayerID));
    }

    /// <summary>
    /// cancel paypal transaction
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    [HttpGet("paypal/cancel")]
    public IActionResult CancelPaypalPayment(string token)
    {
        return Ok(_transactionRepository.CancelPaypalPayment(token));
    }

    #endregion

    #region PAGINATON

    /// <summary>
    /// return list of paged payments
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
