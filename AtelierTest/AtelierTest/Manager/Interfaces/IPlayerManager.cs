using AtelierTest.Controllers.Dto;

namespace AtelierTest.Manager.Interfaces;

public interface IPlayerManager
{
    Task<IReadOnlyList<PlayerDto>> GetPlayersAsync();
    Task<PlayerDto?> GetPlayerByIdAsync(int id);
    Task<StatisticsDto> GetStatisticsAsync();
    Task<PlayerDto> CreatePlayerAsync(CreatePlayerDto dto);
}
