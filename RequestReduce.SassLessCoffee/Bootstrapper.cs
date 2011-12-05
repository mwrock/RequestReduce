using RequestReduce.Api;
using RequestReduce.Utilities;
using SassAndCoffee.Core.Compilers;

namespace RequestReduce.SassLessCoffee
{
    public static class Bootstrapper
    {
        public static void Start()
        {
            Registry.HandlerMaps.Add(x =>
                                         {
                                             var path = x.AbsolutePath.ToLower();
                                             if (path.EndsWith(".sass") || path.EndsWith("scss"))
                                                 return new SassAndCoffeeHandler(new SassFileCompiler());
                                             if (path.EndsWith(".coffee"))
                                                 return new SassAndCoffeeHandler(new CoffeeScriptFileCompiler());
                                             if (path.EndsWith(".less"))
                                                 return new LessHandler(new FileWrapper());
                                             return null;
                                         });
        }
    }
}
