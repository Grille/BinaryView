using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryView_Tests;
internal static class ReflectionUtils
{
    public static TFunc CreateDelegate<T, TFunc>(T target, string name) where TFunc : Delegate
    {
        var info = typeof(T).GetMethod(name);
        return (TFunc)info.CreateDelegate(typeof(TFunc), target);

    }
}
