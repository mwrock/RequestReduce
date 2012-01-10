using System.Collections.Generic;
using SassAndCoffee.Core;
using SassAndCoffee.JavaScript.CoffeeScript;

namespace RequestReduce.SassLessCoffee
{
    public class CoffeeHandler : SassAndCoffeeHandler
    {
        public CoffeeHandler()
            : base(new ContentPipeline(new List<IContentTransform>() { new FileSourceContentTransform("text/coffeescript", ".coffee"), new CoffeeScriptCompilerContentTransform() }))
        {
        }
    }
}
