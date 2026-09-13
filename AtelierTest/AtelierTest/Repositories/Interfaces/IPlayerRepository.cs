using AtelierTest.Repositories.Model;

namespace AtelierTest.Repositories.Interfaces;

public interface IPlayerRepository
{
    Task<IReadOnlyList<PlayerModel>> GetAllAsync();
    Task<PlayerModel?> GetByIdAsync(int id);
    Task<PlayerModel> AddAsync(PlayerModel player);
}
