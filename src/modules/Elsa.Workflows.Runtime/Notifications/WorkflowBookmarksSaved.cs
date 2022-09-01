using Elsa.Mediator.Services;
using Elsa.Workflows.Core.Models;
using Elsa.Workflows.Persistence.Entities;

namespace Elsa.Workflows.Runtime.Notifications;

public record WorkflowBookmarksSaved(List<Bookmark> Bookmarks) : INotification;