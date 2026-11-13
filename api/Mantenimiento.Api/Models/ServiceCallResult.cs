using Cbsdg.Data.Helpers;

namespace Mantenimiento.Api.Models;

public class ServiceCallResult<T>
{
    public OperationResult Result { get; set; } = new OperationResult();
    public T? Data { get; set; }
}
