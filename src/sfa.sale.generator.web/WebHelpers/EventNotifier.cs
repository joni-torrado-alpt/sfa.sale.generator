using MudBlazor;

namespace sfa.sale.generator.web.WebHelpers;

public class EventNotifier
{
    public event EventNotificationMessageHandler OnNotify;

    public void Notify(string messsage, Severity severity = Severity.Info) => OnNotify?.Invoke(this, new(messsage, severity));
}

public delegate void EventNotificationMessageHandler(object? sender, EventNotificationMessageArgs args);

public class EventNotificationMessageArgs : EventArgs
{
    public string Message { get; init; }

    public Severity Severity { get; init; }

    public EventNotificationMessageArgs(string message, Severity severity)
    {
        Message = message;
        Severity = severity;
    }
}