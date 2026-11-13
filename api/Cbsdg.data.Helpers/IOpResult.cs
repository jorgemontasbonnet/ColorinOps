namespace Cbsdg.Data.Helpers
{
    public interface IOpResult
    {
        bool HasError { get; }
        string ErrorMessage { get; }
        string ErrorCode { get; }
        bool Success { get; }
    }
}
