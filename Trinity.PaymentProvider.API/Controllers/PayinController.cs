using MediatR;
using Microsoft.AspNetCore.Mvc;
using Trinity.PaymentPlatform.Airtel.Application.Commands;
using Trinity.PaymentPlatform.Application.Commands;
using Trinity.PaymentPlatform.Application.Models;
using Trinity.PaymentPlatform.Infrastructure.ACL.Airtel.Model;
using Trinity.PaymentPlatform.Infrastructure.ACL.Mpesa.Models.Mpesa;
using Trinity.PaymentPlatform.Mpesa.Application.Commands;
using Trinity.PaymentProvider.API.Shared.ActionResults;

namespace Trinity.PaymentProvider.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayinController (IMediator mediator): ControllerBase
    {

        [HttpGet("test")]
        public async Task<IActionResult> Test()
        {
            return Ok();
        }

        [HttpPost("mpesa")]
        public async Task<IActionResult> CreatePayinStkPushAsync([FromBody] MpesaPayInModel model)
        {
            //todo: add client id to request
            return this.Result(await mediator.Send(new CreatePayinRequestCommand(1, model)));
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayin([FromBody] MpesaPayInModel model)
        {
            return this.Result(await mediator.Send(new CreatePayinRequestCommand(1, model)));
        }

        [HttpPost("mpesa/callback")]
        public async Task<IActionResult> MpesaCallback([FromBody] MpesaCallbackRequest model)
        {
            return this.Result(await mediator.Send(new ProcessMpesaPayinCallbackCommand(model)));
        }

        [HttpPost("airtel")]
        public async Task<IActionResult> CreatePayin([FromBody] AirtelPayInModel model)
        {
            return this.Result(await mediator.Send(new CreatePayinRequestCommand(2, model)));
        }


        [HttpPost("airtel/callback")]
        public async Task<IActionResult> AirtelCallback([FromBody] AirtelCallbackRequest model)
        {
            return this.Result(await mediator.Send(new ProcessAirtelPayinCallbackCommand(model)));
        }


        [HttpPost("airtel/refund")]
        public async Task<IActionResult> AirtelRefund([FromBody] AirtelRefundRequest model)
        {
            return this.Result(await mediator.Send(new ProcessAirtelRefundCommand(model)));
        }

    }
}
