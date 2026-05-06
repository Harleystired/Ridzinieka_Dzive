public interface IWorkScenarioPanel : IScenarioPanel
{
    void SetJobContext(GameManager.JobType jobType);
    void ShowForJob(string prompt, string choice1, string choice2, string choice3, 
        System.Action<int> onChoiceMade, GameManager.JobType jobType);
}
