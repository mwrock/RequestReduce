using System.Collections.Generic;
using SassAndCoffee.Core;
using SassAndCoffee.Ruby.Sass;

namespace RequestReduce.SassLessCoffee
{
    public class SassHandler : SassAndCoffeeHandler
    {
        public SassHandler()
            : base(new ContentPipeline(new List<IContentTransform> { new SassCompilerContentTransform() }))
        {
        }
    }
}
