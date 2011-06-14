using System;
using System.IO;
using RequestReduce.Utilities;
using Xunit;
using Xunit.Sdk;

namespace RequestReduce.Facts.Integration
{
    public class OutputTraceOnFailFactAttribute : FactAttribute
    {
        protected override System.Collections.Generic.IEnumerable<ITestCommand>  EnumerateTestCommands(IMethodInfo method)
        {
            yield return new OutputTraceOnFailCommand(method, Console.Out);
        }
    }

    public class OutputTraceOnFailCommand : FactCommand
    {
        private readonly TextWriter output;

        public OutputTraceOnFailCommand(IMethodInfo method, TextWriter output)
            : base(method)
        {
            this.output = output;
        }

        public override MethodResult Execute(object testClass)
        {
            MethodResult result = null;
            try
            {
                result = base.Execute(testClass);
            }
            finally
            {
                if (!(result is PassedResult))
                {
                    var trace =
                        new WebClientWrapper().DownloadString(
                            "http://localhost:8877/local.html?OutputError=1&OutputTrace=1");
                    output.WriteLine(trace);
                    if(output != Console.Out)
                        Console.Out.WriteLine(trace);
                }
            }
            return result;
        }
    }

}