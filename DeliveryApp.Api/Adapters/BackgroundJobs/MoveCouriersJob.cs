using DeliveryApp.Core.Application.Commands.MoveCouriers;
using MediatR;
using Quartz;

namespace DeliveryApp.Api.Adapters.BackgroundJobs;

[DisallowConcurrentExecution]
public class MoveCouriersJob(IMediator mediator) : IJob
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public async Task Execute(IJobExecutionContext context)
    {
        var moveCourierToOrderCommand = new MoveCouriersCommand();
        await _mediator.Send(moveCourierToOrderCommand);
    }
}