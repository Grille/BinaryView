using System;
using System.Collections.Generic;
using System.Text;

namespace Grille.IO.Interfaces;

/// <summary>
/// Allows object to be writen by WriteIView Function
/// </summary>
public interface IBinaryViewWritable
{
    public void WriteToView(BinaryViewWriter bw);
}
