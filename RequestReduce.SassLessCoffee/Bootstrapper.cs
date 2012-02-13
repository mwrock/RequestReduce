using System;
using RequestReduce.Api;
using RequestReduce.Utilities;

namespace RequestReduce.SassLessCoffee
{
    public static class Bootstrapper
    {
        public static CoffeeHandler CoffeeHandler;
        public static SassHandler SassHandler;

        static Bootstrapper()
        {
            try
            {
                CoffeeHandler = new CoffeeHandler();
                SassHandler = new SassHandler();
            }
            catch (Exception ex)
            {
                var message = string.Format("There were errors Loading Sass and Coffee Handlers");
                var wrappedException = new ApplicationException(message, ex);
                if (Registry.CaptureErrorAction != null)
                    Registry.CaptureErrorAction(wrappedException);
            }
        }

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
