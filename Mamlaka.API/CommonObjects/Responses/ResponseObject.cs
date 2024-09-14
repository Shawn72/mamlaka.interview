using System.Runtime.Serialization;

namespace Mamlaka.API.CommonObjects.Responses;

/// <summary>
/// Generic response status object
/// </summary>
public class ResponseStatus
{
    /// <summary>
    /// Status code of the request
    /// </summary> 
    [DataMember(IsRequired = true)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// Friendly message to be displayed to end-user after evaluating status code
    /// </summary>
    [DataMember(IsRequired = true)]
    public string Message { get; set; } = null!;
}

/// <summary>
/// It's a complex generic base object encapsulating a response object of a specified type alongside request status
/// </summary>    
[DataContract]
public class ResponseObject<T>
{
    /// <summary>
    /// Object containing status of a function call
    /// </summary>
    [DataMember(IsRequired = true)]
    public ResponseStatus Status { get; set; }

    /// <summary>
    /// Generic object containing a method's response
    /// </summary>
    [DataMember(IsRequired = true)]
    public IEnumerable<T> Data { get; set; } 

    public ResponseObject()
    {
        Status = new ResponseStatus { Code = "200", Message = "Request processed successfully" };
        Data = Enumerable.Empty<T>();
    }
}

