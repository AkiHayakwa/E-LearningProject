using LearningManagementSystem.Models;
using System.Collections.Generic;

namespace LearningManagementSystem.Repositories
{
    public interface IRoleRepository
    {
        IEnumerable<Role> GetAll();
        Role GetById(string id);
        void Add(Role role);
        void Update(Role role);
        void Delete(string id);
        void Save();
    }
}