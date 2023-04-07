using System;
using System.Collections.Generic;
using System.Text;

namespace GGL.IO;

/// <summary>
/// Allows object to be read by ReadIView Function
/// </summary>
public interface IViewReadable
{
    public void ReadFromView(BinaryViewReader br);
}
