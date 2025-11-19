using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class MessagesController(IUnitOfWork unitOfWork) : BaseAPIController
{
  [HttpPost]
  public async Task<ActionResult<MessageDTO>> CreateMessage(CreateMessageDTO createMessageDTO)
  {
    var sender = await unitOfWork.MemberRepository.GetMembeByIdAsync(User.GetMemberId());
    var recipient = await unitOfWork.MemberRepository.GetMembeByIdAsync(createMessageDTO.RecipientId);

    if (recipient == null || sender == null || sender.Id == createMessageDTO.RecipientId)
    {
      return BadRequest("Cannot send this message");
    }
    var message = new Message
    {
      SenderId = sender.Id,
      RecipientId = recipient.Id,
      Content = createMessageDTO.Content
    };
    unitOfWork.MessageRepository.AddMessage(message);
    if (await unitOfWork.Complete())
    {
      return message.ToDTO();
    }
    return BadRequest("Failed to send message");
  }

  [HttpGet]
  public async Task<ActionResult<PaginatedResult<MessageDTO>>> GetMessagesByContainer([FromQuery] MessageParams messageParams)
  {
    messageParams.MemberId = User.GetMemberId();
    return await unitOfWork.MessageRepository.GetMessagesForMember(messageParams);
  }

  [HttpGet("thread/{recipientId}")]
  public async Task<ActionResult<IReadOnlyList<MessageDTO>>> GetMessageThread(string recipientId)
  {
    var currentMemberId = User.GetMemberId();
    return Ok(await unitOfWork.MessageRepository.GetMessageThread(currentMemberId, recipientId));
  }

  [HttpDelete("{id}")]
  public async Task<ActionResult> DeleteMessage(string id)
  {
    var memberId = User.GetMemberId();
    var message = await unitOfWork.MessageRepository.GetMessage(id);
    if (message == null)
    {
      return BadRequest("Cannot delete this message");
    }
    // A user cannot delete a message that he did not get or send
    if (message.SenderId != memberId && message.RecipientId != memberId)
    {
      return BadRequest("You cannot delete this message");
    }
    // If the current user is the sender - mark the message as deleted on the sender side
    if (message.SenderId == memberId)
    {
      message.SenderDeleted = true;
    }
    // If the current user is the recipient - mark the message as deleted on the recipient side
    if (message.RecipientId == memberId)
    {
      message.RecipientDeleted = true;
    }
    // A new syntax for conditional checking. In the given object, if the parameters in the brackets are true the condition is satisfied
    if (message is { SenderDeleted: true, RecipientDeleted: true })
    {
      unitOfWork.MessageRepository.DeleteMessage(message);
    }
    if (await unitOfWork.Complete())
    {
      return Ok();
    }
    return BadRequest("Problem deleting the message");
  }
}
