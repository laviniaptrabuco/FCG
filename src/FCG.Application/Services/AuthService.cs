using FCG.Application.DTOs;
using FCG.Application.Interfaces;
using FCG.Domain.Entities;
using FCG.Domain.Enums;
using FCG.Domain.Exceptions;
using FCG.Domain.Interfaces;
using FCG.Domain.Services;
using Microsoft.Extensions.Logging;

namespace FCG.Application.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, ct)
            ?? throw new DomainException("Invalid email or password.");

        if (!user.IsActive)
            throw new DomainException("Account is deactivated.");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new DomainException("Invalid email or password.");

        var token = _jwtService.GenerateToken(user);
        var expiresAt = _jwtService.GetExpiration();

        _logger.LogInformation("User {Email} logged in successfully.", user.Email);

        return new LoginResponse(token, user.Name, user.Email, user.Role.ToString(), expiresAt);
    }

    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken ct = default)
    {
        PasswordValidator.Validate(request.Password);

        if (await _userRepository.ExistsByEmailAsync(request.Email, ct))
            throw new DomainException($"Email '{request.Email}' is already registered.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(request.Name, request.Email, passwordHash, UserRole.User);

        await _userRepository.AddAsync(user, ct);
        _logger.LogInformation("New user registered: {Email}", user.Email);

        return MapToResponse(user);
    }

    private static UserResponse MapToResponse(User user) =>
        new(user.Id, user.Name, user.Email, user.Role.ToString(), user.CreatedAt, user.IsActive);
}
