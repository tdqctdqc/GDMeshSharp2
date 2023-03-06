public class DummyServer : IServer 
{
    public void QueueCommand(Command c, WriteKey key)
    {
        throw new System.NotImplementedException();
    }
}
