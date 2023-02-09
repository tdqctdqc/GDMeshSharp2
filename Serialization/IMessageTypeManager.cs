
public interface IMessageTypeManager
{
    void HandleIncoming(byte marker, byte[] data);
}
