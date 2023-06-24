
using System;
using System.Collections.Generic;
using Godot;

public class PromptManager
{
    public Queue<Prompt> Prompts { get; private set; }
    private HashSet<Type> _currPrompts;
    private float _timer;
    private float _period = 1f;
    private Node _hook;

    public PromptManager(Node hook, Data data)
    {
        _hook = hook;
        _currPrompts = new HashSet<Type>();
        Prompts = new Queue<Prompt>();
        // data.Notices.NeedDecision.Subscribe(d => PushPrompt(new DecisionPrompt(d, Game.I.Client.Key)));
    }

    public void Process(float delta, ClientWriteKey key)
    {
        if (Prompts.Count > 0)
        {
            PushPrompt(Prompts.Dequeue());
        }
    }

    public void PushPrompt(Prompt prompt)
    {
        var w = SceneManager.Instance<PromptWindow>();
        w.Satisfied += () => _currPrompts.Remove(prompt.GetType());
        w.Dismissed += () => _currPrompts.Remove(prompt.GetType());
        w.Setup(prompt);
        _hook.AddChild(w);
        w.Popup_();
    }
}
