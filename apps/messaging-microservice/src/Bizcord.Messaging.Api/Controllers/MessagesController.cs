using Bizcord.Messaging.Api.Application;
using Bizcord.Shared.Contracts.Models;
using Bizcord.Shared.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Bizcord.Messaging.Api.Controllers;

[ApiController]
[Route("api/messages")]
public sealed class MessagesController(MessageService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> Create(CreateMessageRequest request, CancellationToken cancellationToken)
    {
        var message = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = message.MessageId }, message);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MessageDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var message = await service.GetAsync(id, cancellationToken);
        return message is null ? NotFound() : Ok(message);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MessageDto>>> List([FromQuery] Guid? channelId, CancellationToken cancellationToken)
        => Ok(await service.ListAsync(channelId, cancellationToken));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<MessageDto>> Update(Guid id, UpdateMessageRequest request, CancellationToken cancellationToken)
    {
        var message = await service.UpdateAsync(id, request, cancellationToken);
        return message is null ? NotFound() : Ok(message);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
