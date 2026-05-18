using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Comun.BaseDatos
{
    public class Parameter
    {
        public string Name
        {
            get;
            set;
        }

        public DataType Type
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }

        public ParameterDirection Direction
        {
            get;
            set;
        }

        public int Length
        {
            get;
            set;
        }

        public byte[] Blob
        {
            get;
            set;
        }

        public Guid Guid
        {
            get;
            set;
        }

        public enum DataType
        {
            Integer,
            LongInt,
            ShortInt,
            Decimal,
            Float,
            Currency,
            Bit,
            VarChar,
            VarChar2,
            Date,
            DateTime,
            VarBinary,
            UniqueIdentifier,
            Boolean,
            Memo
        }
    }
}
