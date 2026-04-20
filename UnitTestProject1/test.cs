using Microsoft.VisualStudio.TestTools.UnitTesting;
using EnterpriseChatBot.Desktop.Services;

namespace EnterpriseChatBot.Desktop.Tests;

[TestClass]
public class PasswordHasherTests
{
    [TestMethod]
    public void HashPassword_ShouldReturnHash_WhenPasswordIsValid()
    {
        var password = "123456";

        var hash = PasswordHasher.HashPassword(password);

        Assert.IsFalse(string.IsNullOrWhiteSpace(hash));
        Assert.AreNotEqual(password, hash);
    }

    [TestMethod]
    public void VerifyPassword_ShouldReturnTrue_WhenPasswordMatchesHash()
    {
        var password = "qwerty123";
        var hash = PasswordHasher.HashPassword(password);

        var result = PasswordHasher.VerifyPassword(password, hash);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void VerifyPassword_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        var password = "qwerty123";
        var wrongPassword = "wrongpass";
        var hash = PasswordHasher.HashPassword(password);

        var result = PasswordHasher.VerifyPassword(wrongPassword, hash);

        Assert.IsFalse(result);
    }
}