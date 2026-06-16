using Microsoft.EntityFrameworkCore;
using TODO_App.Domain.Entities;
using TODO_App.Domain.Interfaces;

namespace TODO_App.DataAccess.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task AddAsync(User user) => await _dbContext.Users.AddAsync(user);

    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
}
