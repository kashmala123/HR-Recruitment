namespace HRRecruitment.Services
{
    public interface IActivityLogger
    {
        void Log(string employeeId, string actionType, string description);
    }
}