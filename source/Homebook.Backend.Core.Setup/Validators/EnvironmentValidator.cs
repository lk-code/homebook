using FluentValidation;
using Homebook.Backend.Core.Setup.Models;
using System.Text.RegularExpressions;

namespace Homebook.Backend.Core.Setup.Validators;

public class EnvironmentValidator : AbstractValidator<EnvironmentConfiguration>
{
    public EnvironmentValidator()
    {
        // Database Host validation
        RuleFor(x => x.DatabaseHost)
            .Must(BeValidHostname)
            .WithMessage("DatabaseHost must be a valid hostname or IP address")
            .When(x => !string.IsNullOrEmpty(x.DatabaseHost));

        // Database Port validation
        RuleFor(x => x.DatabasePort)
            .Must(BeValidPort)
            .WithMessage("DatabasePort must be a valid port number between 1 and 65535")
            .When(x => !string.IsNullOrEmpty(x.DatabasePort));

        // Database Name validation+
        RuleFor(x => x.DatabaseName)
            .Must(BeValidDatabaseName)
            .WithMessage("DatabaseName can only contain alphanumeric characters, underscores and hyphens")
            .Length(1, 63)
            .WithMessage("DatabaseName must be between 1 and 63 characters long")
            .When(x => !string.IsNullOrEmpty(x.DatabaseName));

        // Database Username validation
        RuleFor(x => x.DatabaseUserName)
            .Must(BeValidUsername)
            .WithMessage("DatabaseUserName can only contain alphanumeric characters, underscores, dots and hyphens")
            .Length(1, 63)
            .WithMessage("DatabaseUserName must be between 1 and 63 characters long")
            .When(x => !string.IsNullOrEmpty(x.DatabaseUserName));

        // Database Password validation
        RuleFor(x => x.DatabaseUserPassword)
            .Must(BeValidPassword)
            .WithMessage("DatabaseUserPassword contains invalid characters")
            .MinimumLength(8)
            .WithMessage("DatabaseUserPassword must be at least 8 characters long")
            .When(x => !string.IsNullOrEmpty(x.DatabaseUserPassword));
    }

    private static bool BeValidHostname(string? hostname)
    {
        if (string.IsNullOrEmpty(hostname))
            return true;

        // 1. Check for valid IPv4 (3 dots, 4 numbers between 1-255)
        if (IsValidIPv4(hostname))
            return true;

        // 2. Check for valid IPv6 (valid format, only 0-9 and a-f)
        if (IsValidIPv6(hostname))
            return true;

        // 3. Check for valid hostname (only a-z and 0-9, with dots and hyphens for structure)
        if (IsValidHostnameOnly(hostname))
            return true;

        // 4. Otherwise return false
        return false;
    }

    private static bool IsValidHostnameOnly(string hostname)
    {
        if (string.IsNullOrEmpty(hostname) || hostname.Length > 253)
            return false;

        // Split by dots to check each label
        var labels = hostname.Split('.');

        return labels.All(label =>
        {
            // Each label must be 1-63 characters
            if (string.IsNullOrEmpty(label) || label.Length > 63)
                return false;

            // Only alphanumeric characters and hyphens allowed
            if (!label.All(c => char.IsLetterOrDigit(c) || c == '-'))
                return false;

            // Cannot start or end with hyphen
            if (label.StartsWith('-') || label.EndsWith('-'))
                return false;

            return true;
        });
    }

    private static bool IsValidIPv4(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return false;

        // Split and check each octet individually for better validation
        var parts = ip.Split('.');
        if (parts.Length != 4) return false;

        return parts.All(part =>
        {
            // Check for empty parts
            if (string.IsNullOrEmpty(part)) return false;

            // Check if it's a valid number
            if (!int.TryParse(part, out int num)) return false;

            // Check range (1-255)
            if (num < 1 || num > 255) return false;

            // Prevent leading zeros (except for single digit numbers)
            if (part.Length > 1 && part[0] == '0') return false;

            // Ensure the parsed number matches the original string
            return part == num.ToString();
        });
    }

    private static bool IsValidIPv6(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) return false;

        // Comprehensive IPv6 regex pattern
        var ipv6Regex = new Regex(
            @"^(?:" +
            // Standard IPv6 (8 groups of 4 hex digits)
            @"(?:[0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}|" +
            // IPv6 with :: compression (various positions)
            @"(?:[0-9a-fA-F]{1,4}:){1,7}:|" +
            @"(?:[0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|" +
            @"(?:[0-9a-fA-F]{1,4}:){1,5}(?::[0-9a-fA-F]{1,4}){1,2}|" +
            @"(?:[0-9a-fA-F]{1,4}:){1,4}(?::[0-9a-fA-F]{1,4}){1,3}|" +
            @"(?:[0-9a-fA-F]{1,4}:){1,3}(?::[0-9a-fA-F]{1,4}){1,4}|" +
            @"(?:[0-9a-fA-F]{1,4}:){1,2}(?::[0-9a-fA-F]{1,4}){1,5}|" +
            @"[0-9a-fA-F]{1,4}:(?:(?::[0-9a-fA-F]{1,4}){1,6})|" +
            @":(?:(?::[0-9a-fA-F]{1,4}){1,7}|:)|" +
            // IPv6 with IPv4 suffix (like ::ffff:192.168.1.1)
            @"(?:[0-9a-fA-F]{1,4}:){1,4}:(?:(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])\.){3}(?:25[0-5]|2[0-4][0-9]|1[0-9][0-9]|[1-9]?[0-9])|" +
            // Special cases
            @"::1|::" +
            @")$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        return ipv6Regex.IsMatch(ip);
    }

    private static bool BeValidPort(string? port)
    {
        if (string.IsNullOrEmpty(port))
            return true;

        return int.TryParse(port, out int portNum) && portNum >= 1 && portNum <= 65535;
    }

    private static bool BeValidDatabaseName(string? databaseName)
    {
        if (string.IsNullOrEmpty(databaseName))
            return true;

        // Only allow alphanumeric characters, underscores and hyphens
        var regex = new Regex(@"^[a-zA-Z0-9_-]+$", RegexOptions.Compiled);
        return regex.IsMatch(databaseName) &&
               !databaseName.StartsWith("-") &&
               !databaseName.EndsWith("-");
    }

    private static bool BeValidUsername(string? username)
    {
        if (string.IsNullOrEmpty(username))
            return true;

        // Alphanumeric characters, underscores, dots and hyphens
        var regex = new Regex(@"^[a-zA-Z0-9_.-]+$", RegexOptions.Compiled);
        return regex.IsMatch(username) &&
               !username.StartsWith(".") &&
               !username.EndsWith(".") &&
               !username.StartsWith("-") &&
               !username.EndsWith("-");
    }

    private static bool BeValidPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return true;

        // Prevents dangerous characters that could be used for SQL/Command injection
        var dangerousChars = new[] { '\'', '"', ';', '<', '>', '&', '|', '`', '$' };

        // Check if password contains dangerous characters
        return !dangerousChars.Any(password.Contains) &&
               // Additionally: No control characters except normal whitespace
               !password.Any(c => char.IsControl(c) && c != '\t' && c != '\n' && c != '\r');
    }
}
