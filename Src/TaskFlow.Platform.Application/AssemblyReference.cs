using System.Reflection;

namespace TaskFlow.Platform.Application;

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
