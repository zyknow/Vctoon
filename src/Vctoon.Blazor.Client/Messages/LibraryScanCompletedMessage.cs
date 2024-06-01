using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Vctoon.Blazor.Client.Messages;

public class LibraryScanCompletedMessage : ValueChangedMessage<Guid>
{
    public LibraryScanCompletedMessage(Guid value) : base(value)
    {
    }
}