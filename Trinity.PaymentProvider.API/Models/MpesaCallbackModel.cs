using System.Text.Json;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa;

namespace Trinity.PaymentProvider.API.Models;

public class MpesaCallbackModel
{
    public MpesaCallbackBodyModel Body { get; set; }
}

public class MpesaCallbackBodyModel
{
    public MpesaStkCallbackModelBody StkCallback { get; set; }
}

public class MpesaStkCallbackModelBody
{
    public string MerchantRequestID { get; set; }
    public string CheckoutRequestID { get; set; }
    public int ResultCode { get; set; }
    public string ResultDesc { get; set; }
    public MpesaCallbackMetadataModel? CallbackMetadata { get; set; }
}

public class MpesaCallbackMetadataModel
{
    public List<MpesaCallbackItemModel> Item { get; set; }
}

public class MpesaCallbackItemModel: IParameter
{
    public string Name { get; set; }
    public JsonElement Value { get; set; }

    public string Key=> Name;
    public object? RawValue => Value;
}