namespace DeCorrespondent
{
    public interface ILogger
    {
        void Info(string message, params object[] args);
        void Debug(string message, params object[] args);
    }
}
