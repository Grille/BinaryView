using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryView_Tests.Framework;
public class TestFailException : Exception
{
    public TestFailException(string msg) : base(msg) { 

    }
}

public class TestSuccessException : Exception
{
    public TestSuccessException(string msg) : base(msg)
    {

    }
}
