using SassAndCoffee.Core.Compilers;

namespace RequestReduce.SassLessCoffee
{
    public class SassHandler : SassAndCoffeeHandler
    {
        public SassHandler() : base(new SassFileCompiler())
        {
        }
    }
}
