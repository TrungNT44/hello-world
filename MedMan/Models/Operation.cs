using System;

namespace sThuoc.Models
{
    [Flags]
    public enum Operations
    {
        None = 0,
        Create = 1,
        Read = 2,
        Modify = 4,        
        Execute = 8,
        History = 16,
        Export = 32,
        Import = 64,
        Delete = 128
    }

}
