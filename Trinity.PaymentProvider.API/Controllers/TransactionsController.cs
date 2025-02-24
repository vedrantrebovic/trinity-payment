using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trinity.PaymentPlatform.Mpesa.Application.Queries;
using Trinity.PaymentProvider.API.Shared.ActionResults;

namespace Trinity.PaymentProvider.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController(IMediator mediator) : ControllerBase
    {
        [HttpGet("status/{transactionReference}")]
        public async Task<IActionResult> GetTransactionStatusAsync([FromRoute]string transactionReference)
        {
            return this.Result(await mediator.Send(new GetTransactionStatusQuery(transactionReference)));
        }
    }
}
