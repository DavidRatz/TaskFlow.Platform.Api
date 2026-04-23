using System.Reflection;

namespace TaskFlow.Platform.Infrastructure;

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;

    public static Type AssemblyType => typeof(AssemblyReference);
}
