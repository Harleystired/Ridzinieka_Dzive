using System;

public interface IScenarioPanel
{
    void Show(string prompt, string choice1, string choice2, string choice3, Action<int> onChoicePicked);
    void Hide();
    bool IsVisible { get; }
}
