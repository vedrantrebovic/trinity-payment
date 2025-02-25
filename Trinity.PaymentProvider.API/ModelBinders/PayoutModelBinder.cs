using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using Trinity.PaymentPlatform.Application.Models;

namespace Trinity.PaymentProvider.API.ModelBinders;

public class PayoutModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        using var sr = new StreamReader(bindingContext.HttpContext.Request.Body);
        var json = await sr.ReadToEndAsync();
        JObject requestJObject = JObject.Parse(json);
        string? type = requestJObject["entityType"]?.ToObject<string>();

        PayoutModel? model = type
            switch
            {
                nameof(MpesaPayInModel) => JsonSerializer.Deserialize<MpesaPayoutModel>(json),
                _ =>
                    throw new Exception()
            };

        bindingContext.Result = ModelBindingResult.Success(model);
    }
}

public class PayoutModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType != typeof(PayoutModel))
            return null;

        return new PayoutModelBinder();
    }
}