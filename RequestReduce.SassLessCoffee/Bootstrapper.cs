using RequestReduce.Api;
using RequestReduce.Utilities;

namespace RequestReduce.SassLessCoffee
{
    public static class Bootstrapper
    {
        public static CoffeeHandler CoffeeHandler = new CoffeeHandler();
        public static SassHandler SassHandler = new SassHandler();

        public static void Start()
        {
            Registry.HandlerMaps.Add(x =>
                                         {
                                             var path = x.AbsolutePath.ToLower();
                                             if (path.EndsWith(".sass") || path.EndsWith("scss"))
                                                 return SassHandler;
                                             if (path.EndsWith(".coffee"))
                                                 return CoffeeHandler;
                                             if (path.EndsWith(".less"))
                                                 return new LessHandler(new FileWrapper());
                                             return null;
                                         });
        }
    }
}
