using System;
using System.Collections.Generic;
using System.Text;

namespace GGL.IO;
public interface IViewReadable
{
    public void ReadFromView(BinaryViewReader br);
}
