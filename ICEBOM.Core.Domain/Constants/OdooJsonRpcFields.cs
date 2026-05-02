namespace ICEBOM.Core.Domain.Constants
{
    public static class OdooJsonRpcFields
    {
        public const string JsonRpcVersion = "2.0";

        public static class Request
        {
            public const string Method = "method";
            public const string Params = "params";
            public const string Id = "id";
        }

        public static class Response
        {
            public const string Result = "result";
            public const string Error = "error";
            public const string Id = "id";
        }

        public static class Error
        {
            public const string Code = "code";
            public const string Message = "message";
            public const string Data = "data";
        }
    }
}