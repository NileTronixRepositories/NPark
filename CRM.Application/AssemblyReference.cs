using System.Reflection;

namespace CRM.Application
{
    public class AssemblyReference
    {
        public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
    }
}