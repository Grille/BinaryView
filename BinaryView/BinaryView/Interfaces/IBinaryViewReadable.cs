using System;
using System.Collections.Generic;
using System.Text;

namespace Grille.IO.Interfaces;

/// <summary>
/// Allows object to be read by ReadIView Function
/// </summary>
public interface IBinaryViewReadable
{
    public void ReadFromView(BinaryViewReader br);
}
