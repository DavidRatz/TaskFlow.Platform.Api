using System.Reflection;

namespace TaskFlow.Platform.Domain;

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
