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
                                                 return new SassHandler();
                                             if (path.EndsWith(".coffee"))
                                                 return new CoffeeHandler();
                                             if (path.EndsWith(".less"))
                                                 return new LessHandler(new FileWrapper());
                                             return null;
                                         });
        }
    }
}
