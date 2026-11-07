namespace Bizcord.Services
{
    // Resultat af et kald til gatewayen: enten succes (evt. med en værdi) eller en fejlbesked, der kan vises.
    public class GatewayResult
    {
        public string? Error { get; }
        public bool IsSuccess => Error is null;

        protected GatewayResult(string? error) => Error = error;

        public static GatewayResult Ok() => new(null);
        public static GatewayResult Fail(string error) => new(error);
    }

    public class GatewayResult<T> : GatewayResult
    {
        public T? Value { get; }

        private GatewayResult(T? value, string? error) : base(error) => Value = value;

        public static GatewayResult<T> Ok(T value) => new(value, null);
        public static new GatewayResult<T> Fail(string error) => new(default, error);
    }
}
