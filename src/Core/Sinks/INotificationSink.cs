namespace Core.Sinks;

public interface INotificationSink
{
    void Notify(string message);
}