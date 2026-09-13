using AtelierTest.Controllers.Dto;
using AtelierTest.Manager.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AtelierTest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerManager _playerManager;

    public PlayersController(IPlayerManager playerManager)
    {
        _playerManager = playerManager;
    }

    /// <summary>Retourne tous les joueurs, triés du meilleur (rank 1) au moins bon.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlayerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlayerDto>>> GetPlayers()
    {
        var players = await _playerManager.GetPlayersAsync();
        return Ok(players);
    }

    /// <summary>Retourne les statistiques agrégées sur l'ensemble des joueurs.</summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(StatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StatisticsDto>> GetStatistics()
    {
        var statistics = await _playerManager.GetStatisticsAsync();
        return Ok(statistics);
    }

    /// <summary>Retourne un joueur par son identifiant.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerDto>> GetPlayerById(int id)
    {
        var player = await _playerManager.GetPlayerByIdAsync(id);
        return player is null ? NotFound() : Ok(player);
    }

    /// <summary>Crée un nouveau joueur.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlayerDto>> CreatePlayer(CreatePlayerDto dto)
    {
        var created = await _playerManager.CreatePlayerAsync(dto);
        return CreatedAtAction(nameof(GetPlayerById), new { id = created.Id }, created);
    }
}
