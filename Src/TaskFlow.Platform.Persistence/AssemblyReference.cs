using System.Reflection;

namespace TaskFlow.Platform.Persistence;

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
