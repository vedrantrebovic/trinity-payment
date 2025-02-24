using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Trinity.PaymentProvider.API.Shared.ModelBinders;

public class CsvIntModelBinder:IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var key = bindingContext.ModelName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(key);
        if (valueProviderResult == null)
        {
            return;
        }
        
        var attemptedValue = valueProviderResult.Values;
        if (attemptedValue.Count>0)
        {
            var list = attemptedValue.ToString().Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).
                Select(v => int.Parse(v.Trim())).ToList();

            bindingContext.Result = ModelBindingResult.Success(list);
        }
        else
        {
            bindingContext.Result = ModelBindingResult.Failed();
        }
    }
}