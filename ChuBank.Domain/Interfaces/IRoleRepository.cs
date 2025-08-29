using ChuBank.Domain.Entities;

namespace ChuBank.Domain.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id);
    Task<Role?> GetByNameAsync(string name);
    Task<Role> CreateAsync(Role role);
    Task<IEnumerable<Role>> GetAllAsync();
    Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
}
