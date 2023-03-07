public class DummyServer : IServer 
{
    public void QueueCommandLocal(Command c, WriteKey key)
    {
        return;
    }
}
