using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using Trinity.PaymentPlatform.Application.Models;

namespace Trinity.PaymentProvider.API.ModelBinders;

public class PayInModelBinder:IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        using var sr = new StreamReader(bindingContext.HttpContext.Request.Body);
        var json = await sr.ReadToEndAsync();
        JObject requestJObject = JObject.Parse(json);

        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.General);
        options.RespectRequiredConstructorParameters = true;
        var mpesaModel = JsonSerializer.Deserialize<MpesaPayInModel>(json, options);
        if (mpesaModel != null)
        {
            bindingContext.Result = ModelBindingResult.Success(mpesaModel);
        }
        else
        {
            throw new NotSupportedException("Model not supported");
        }
    }
}

public class PayInModelBinderProvider: IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType != typeof(PayInModel))
            return null;

        return new PayInModelBinder();
    }
}