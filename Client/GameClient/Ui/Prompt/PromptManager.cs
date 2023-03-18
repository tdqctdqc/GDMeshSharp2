
using System.Collections.Generic;
using Godot;

public class PromptManager
{
    public Queue<IPrompt> Prompts { get; private set; }
    private float _timer;
    private float _period = 1f;
    private Node _hook;

    public PromptManager(Node hook, Data data, ClientWriteKey key)
    {
        _hook = hook;
        Prompts = new Queue<IPrompt>();
        data.Notices.NeedDecision.Subscribe(d => GetPrompt(new DecisionPrompt(d, key)));
    }

    public void Process(float delta, ClientWriteKey key)
    {
        _timer += delta;
        if (_timer >= _period)
        {
            _timer = 0;
            Check(key);
        }

        if (Prompts.Count > 0)
        {
            GetPrompt(Prompts.Dequeue());
        }
    }

    private void Check(ClientWriteKey key)
    {
        var player = key.Data.BaseDomain.Players.LocalPlayer;
        if (player != null && player.Regime.Empty())
        {
            Prompts.Enqueue(new ChooseRegimePrompt(key.Data, key));
        }
    }
    public void GetPrompt(IPrompt prompt)
    {
        var w = SceneManager.Instance<PromptWindow>();
        w.Setup(prompt);
        _hook.AddChild(w);
        w.Popup_();
    }
}
