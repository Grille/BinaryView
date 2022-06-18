using System;
using System.Collections.Generic;
using System.Text;

namespace GGL.IO;
public interface IViewWritable
{
    public void WriteToView(BinaryViewWriter bw);
}
