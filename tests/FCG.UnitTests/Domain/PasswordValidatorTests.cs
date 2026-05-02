using FCG.Domain.Exceptions;
using FCG.Domain.Services;
using FluentAssertions;
using Xunit;

namespace FCG.UnitTests.Domain;

public class PasswordValidatorTests
{
    [Theory]
    [InlineData("Senha@123")]
    [InlineData("P@ssw0rd")]
    [InlineData("Abc123!@#")]
    [InlineData("MyP4ss_word!")]
    public void Validate_WithStrongPassword_ShouldNotThrow(string password)
    {
        var act = () => PasswordValidator.Validate(password);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("12345678")]        // apenas números
    [InlineData("abcdefgh")]        // apenas letras minúsculas
    [InlineData("ABCDEFGH")]        // apenas letras maiúsculas
    [InlineData("Abc1234")]         // sem especial, menos de 8
    [InlineData("Ab@1")]            // muito curto
    [InlineData("semNumero@Letra")] // sem número
    [InlineData("SemEspecial123")]  // sem especial
    public void Validate_WithWeakPassword_ShouldThrowDomainException(string password)
    {
        var act = () => PasswordValidator.Validate(password);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void Validate_WithNullOrEmpty_ShouldThrowDomainException(string? password)
    {
        var act = () => PasswordValidator.Validate(password!);
        act.Should().Throw<DomainException>().WithMessage("*empty*");
    }

    [Theory]
    [InlineData("Senha@123", true)]
    [InlineData("fraco", false)]
    [InlineData("", false)]
    public void IsValid_ShouldReturnExpectedResult(string password, bool expected)
    {
        PasswordValidator.IsValid(password).Should().Be(expected);
    }
}
