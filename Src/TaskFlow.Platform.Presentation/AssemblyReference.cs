using System.Reflection;

namespace TaskFlow.Platform.Presentation;

public static class AssemblyReference
{
    public static Assembly Assembly => typeof(AssemblyReference).Assembly;
}
