using System;
using System.Collections.Generic;
using System.Text;

namespace GGL.IO;

/// <summary>
/// Allows object to be writen by WriteIView Function
/// </summary>
public interface IViewWritable
{
    public void WriteToView(BinaryViewWriter bw);
}
