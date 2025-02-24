using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Trinity.PaymentProvider.API.Shared.ActionResults;

public class CustomBadRequest : ValidationProblemDetails
    {
        
        public Response<object> Response { get; }
        public CustomBadRequest(ActionContext context)
        {
            if (!context.ModelState.IsValid)
            {
                Response<object> response = new Response<object>();
                foreach (ModelStateEntry modelStateValue in context.ModelState.Values)
                {
                    foreach (ModelError error in modelStateValue.Errors)
                    {
                        response.Errors.Add(new Error(error.ErrorMessage, error.ErrorMessage));
                    }
                }

                Response = response;
                
                // var result = JsonConvert.SerializeObject(response);
                // context.HttpContext.Response.ContentType = "application/json";
                // context.HttpContext.Response.StatusCode = 400;
                //context.HttpContext.Response.WriteAsync(result);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }


        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                Response<object> response = new Response<object>();
                foreach (ModelStateEntry modelStateValue in context.ModelState.Values)
                {
                    foreach (ModelError error in modelStateValue.Errors)
                    {
                        response.Errors.Add(new Error(error.ErrorMessage, error.ErrorMessage));
                    }
                }

                context.Result = new JsonResult(response) { StatusCode = 400 };
            }
        }

        public void Validate(ActionContext actionContext, ValidationStateDictionary validationState, string prefix, object model)
        {
            if (!actionContext.ModelState.IsValid)
            {
                Response<object> response = new Response<object>();
                foreach (ModelStateEntry modelStateValue in actionContext.ModelState.Values)
                {
                    foreach (ModelError error in modelStateValue.Errors)
                    {
                        response.Errors.Add(new Error(error.ErrorMessage, error.ErrorMessage));
                    }
                }


                //actionContext.HttpContext.Response = new JsonResult(response) { StatusCode = 400 };
            }
        }
    }