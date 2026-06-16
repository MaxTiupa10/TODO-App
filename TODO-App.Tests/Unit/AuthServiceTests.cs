using Microsoft.Extensions.Configuration;
using Moq;
using TODO_App.Domain.Entities;
using TODO_App.Domain.Interfaces;
using TODO_App.Services.DTOs;
using TODO_App.Services.Services;

namespace TODO_App.Tests.Unit;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly IConfiguration _configuration;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TestSecretKeyForJwtTokenGeneration12345!",
                ["Jwt:Issuer"] = "TODO-App",
                ["Jwt:Audience"] = "TODO-App",
                ["Jwt:ExpireMinutes"] = "60"
            })
            .Build();

        _sut = new AuthService(_userRepository.Object, _configuration);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ReturnsNull()
    {
        _userRepository
            .Setup(r => r.GetByUsernameAsync("existing"))
            .ReturnsAsync(new User { Id = 1, Username = "existing", PasswordHash = "hash" });

        var result = await _sut.RegisterAsync(new RegisterDto { Username = "existing", Password = "Password123!" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsNull()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correct");
        _userRepository
            .Setup(r => r.GetByUsernameAsync("user"))
            .ReturnsAsync(new User { Id = 1, Username = "user", PasswordHash = hash });

        var result = await _sut.LoginAsync(new LoginDto { Username = "user", Password = "wrong" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsToken()
    {
        var password = "Password123!";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        _userRepository
            .Setup(r => r.GetByUsernameAsync("user"))
            .ReturnsAsync(new User { Id = 5, Username = "user", PasswordHash = hash });

        var result = await _sut.LoginAsync(new LoginDto { Username = "user", Password = password });

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
        Assert.Equal("user", result.Username);
        Assert.Equal(5, result.UserId);
    }
}
