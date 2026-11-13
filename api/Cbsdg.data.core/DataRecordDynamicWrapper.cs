using System.Data;
using System.Dynamic;

namespace Cbsdg.Data.Core
{
    public class DataRecordDynamicWrapper : DynamicObject
    {
        private readonly IDataRecord _dataRecord;
        public DataRecordDynamicWrapper(IDataRecord dataRecord) { _dataRecord = dataRecord; }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _dataRecord[binder.Name];
            return result != null;
        }
    }
}
