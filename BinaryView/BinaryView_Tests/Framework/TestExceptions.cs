using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryView_Tests.Framework;
internal class TestFailException : Exception
{
    public TestFailException(string msg) : base(msg) { 

    }
}

internal class TestSucException : Exception
{
    public TestSucException(string msg) : base(msg)
    {

    }
}
