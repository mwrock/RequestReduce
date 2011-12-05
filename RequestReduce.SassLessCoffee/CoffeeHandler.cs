using SassAndCoffee.Core.Compilers;

namespace RequestReduce.SassLessCoffee
{
    public class CoffeeHandler : SassAndCoffeeHandler
    {
        public CoffeeHandler()
            : base(new CoffeeScriptFileCompiler())
        {
        }
    }
}
