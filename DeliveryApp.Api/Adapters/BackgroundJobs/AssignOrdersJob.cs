using DeliveryApp.Core.Application.Commands.AssignOrder;
using MediatR;
using Quartz;

namespace DeliveryApp.Api.Adapters.BackgroundJobs;

[DisallowConcurrentExecution]
public class AssignOrdersJob(IMediator mediator) : IJob
{
    private readonly IMediator _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));

    public async Task Execute(IJobExecutionContext context)
    {
        var assignOrdersCommand = new AssignOrderCommand();
        await _mediator.Send(assignOrdersCommand);
    }
}