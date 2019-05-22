using KPIWebApi.Utils;
using KPIWebAPI.ViewModels;

namespace XKPI.Utils
{
    public abstract class AbsAssembleFormula : ICanAssembleFormula
    {
        public int KPI_ID;
        public AbsAssembleFormula(int kpiid) { KPI_ID = kpiid; }
        public abstract KPIFormula AssembleFormula();
    }
}
