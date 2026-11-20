using System.Reflection;
using Core.Domain;

namespace Core.Tests;

public sealed class ArchitectureGuardTests
{
    [Fact]
    public void Core_Should_Not_Reference_Infrastructure_Or_AspNetCore_Assemblies()
    {
        Assembly coreAssembly = typeof(Product).Assembly;

        string[] forbiddenPrefixes =
        [
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore",
            "Scalar.AspNetCore",
            "Infrastructure"
        ];

        var referencedNames = coreAssembly
            .GetReferencedAssemblies()
            .Select(a => a.Name ?? string.Empty)
            .ToList();

        var violations = referencedNames
            .Where(name => forbiddenPrefixes.Any(prefix =>
                name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.Empty(violations);
    }
}
