namespace ICEBOM.Core.Domain.Odoo.JsonRpc
{
    public class OdooJsonRpcException : Exception
    {
        public int Code { get; }

        public OdooJsonRpcException(int code, string message)
            : base(message)
        {
            Code = code;
        }
    }
}