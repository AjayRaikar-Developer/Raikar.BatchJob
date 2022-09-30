namespace Raikar.BatchJob.Models
{
    public enum BatchMode
    {
        Fresh,
        Manual,
        AutoRetry
    }

    public enum BatchProcessMode
    {
        Single,
        Parallel
    }
}
