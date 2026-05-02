using FCG.Application.DTOs;
using FCG.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GamesController : ControllerBase
{
    private readonly GameService _gameService;

    public GamesController(GameService gameService) => _gameService = gameService;

    /// <summary>Lista todos os jogos disponíveis.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<GameResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var games = await _gameService.GetAllAsync(ct);
        return Ok(games);
    }

    /// <summary>Obtém um jogo pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var game = await _gameService.GetByIdAsync(id, ct);
        return Ok(game);
    }

    /// <summary>Filtra jogos por gênero.</summary>
    [HttpGet("genre/{genre}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<GameResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByGenre(string genre, CancellationToken ct)
    {
        var games = await _gameService.GetByGenreAsync(genre, ct);
        return Ok(games);
    }

    /// <summary>Cadastra um novo jogo. (Admin)</summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateGameRequest request, CancellationToken ct)
    {
        var result = await _gameService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Atualiza um jogo. (Admin)</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGameRequest request, CancellationToken ct)
    {
        var result = await _gameService.UpdateAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Aplica promoção a um jogo. (Admin)</summary>
    [HttpPatch("{id:guid}/promotion")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(GameResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyPromotion(Guid id, [FromBody] ApplyPromotionRequest request, CancellationToken ct)
    {
        var result = await _gameService.ApplyPromotionAsync(id, request, ct);
        return Ok(result);
    }

    /// <summary>Remove promoção de um jogo. (Admin)</summary>
    [HttpDelete("{id:guid}/promotion")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemovePromotion(Guid id, CancellationToken ct)
    {
        await _gameService.RemovePromotionAsync(id, ct);
        return NoContent();
    }

    /// <summary>Desativa um jogo. (Admin)</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _gameService.DeactivateAsync(id, ct);
        return NoContent();
    }
}
