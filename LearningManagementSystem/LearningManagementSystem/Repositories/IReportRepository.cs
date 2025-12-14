using LearningManagementSystem.Models;

namespace LearningManagementSystem.Repositories
{
    public interface IReportRepository
    {
        IQueryable<Report> GetAll();
        Report? GetById(string id);
        IQueryable<Report> GetByStatus(string status);
        void Add(Report report);
        void Update(Report report);
        void Delete(string id);
        void Save();
    }
}

