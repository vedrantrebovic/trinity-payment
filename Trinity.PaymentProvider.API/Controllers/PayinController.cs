using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa;
using Trinity.PaymentPlatform.Mpesa.Application.Commands;
using Trinity.PaymentProvider.API.Models;
using Trinity.PaymentProvider.API.Shared.ActionResults;

namespace Trinity.PaymentProvider.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayinController (IMediator mediator): ControllerBase
    {
        [HttpPost("mpesa")]
        public async Task<IActionResult> CreatePayinStkPushAsync([FromBody] MpesaPayinModel model)
        {
            //todo: add client id to request
            return this.Result(await mediator.Send(new CreatePayinRequestCommand(model.UserId, model.Amount, model.CurrencyCode, model.AccountNumber, model.TransactionReference)));
        }

        [HttpPost("mpesa/callback")]
        public async Task<IActionResult> MpesaCallback([FromBody] MpesaCallbackRequest model)
        {
            return this.Result(await mediator.Send(new ProcessMpesaPayinCallbackCommand(model)));
        }
    }
}
