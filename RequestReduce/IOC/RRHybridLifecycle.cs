using StructureMap.Pipeline;

namespace RequestReduce.IOC
{
    public class RRHybridLifecycle : HttpLifecycleBase<RRHttpContextLifecycle, ThreadLocalStorageLifecycle>
    {
        public override string Scope
        {
            get
            {
                return "RRHybridLifecycle";
            }
        }
    }


}
