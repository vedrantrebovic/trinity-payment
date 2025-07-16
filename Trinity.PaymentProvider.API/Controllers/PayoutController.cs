using MediatR;
using Microsoft.AspNetCore.Mvc;
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
    public class PayoutController (IMediator mediator): ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> CreatePayoutRequest([FromBody] MpesaPayoutModel model)
        {
            var result = await mediator.Send(new CreatePayoutRequestCommand(1, model));

            return this.Result(await mediator.Send(new CreatePayoutRequestCommand(1, model)));
        }

        [HttpPost("airtel")]
        public async Task<IActionResult> CreateAirtelPayoutRequest([FromBody] AirtelPayoutModel model)
        {
            return this.Result(await mediator.Send(new CreatePayoutRequestCommand(2, model)));
        }
        [HttpPost("result")]
        public async Task<IActionResult> B2CResult([FromBody] B2CResultRequestPayout model)
        {
            return this.Result(mediator.Send(new ProcessB2CResultCommand(model)));
        }

        [HttpPost("timeout")]
        public async Task<IActionResult> Timeout([FromBody] B2CRequest model)
        {
            return this.Result(mediator.Send(new ProcessTimeoutRequestCommand(model)));
        }

        [HttpPost("statusCheck")]
        public async Task<IActionResult> StatusCheck([FromBody] B2CResultRequestPayout model)
        {
            return this.Result(await mediator.Send(new ProcessPayoutStatusCheckCommand(model)));
        }

        [HttpPost("statusCheck/timeout")]
        public async Task<IActionResult> StatusCheckTimeout([FromBody] TransactionStatusQueryRequest model)
        {
            return this.Result(mediator.Send(new ProcessStatusCheckTimeoutRequestCommand(model)));
        }

    }
}
