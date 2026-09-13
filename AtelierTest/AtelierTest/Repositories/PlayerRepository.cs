using AtelierTest.Repositories.Interfaces;
using AtelierTest.Repositories.Model;
using Microsoft.EntityFrameworkCore;

namespace AtelierTest.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly PlayerContext _context;

    public PlayerRepository(PlayerContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PlayerModel>> GetAllAsync()
    {
        return await _context.Players.AsNoTracking().ToListAsync();
    }

    public async Task<PlayerModel?> GetByIdAsync(int id)
    {
        return await _context.Players.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PlayerModel> AddAsync(PlayerModel player)
    {
        var maxId = await _context.Players.AnyAsync()
            ? await _context.Players.MaxAsync(p => p.Id)
            : 0;
        player.Id = maxId + 1;

        _context.Players.Add(player);
        await _context.SaveChangesAsync();

        return player;
    }
}
