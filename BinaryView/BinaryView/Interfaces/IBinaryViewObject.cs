using System;
using System.Collections.Generic;
using System.Text;

namespace Grille.IO.Interfaces;

/// <summary>
/// Allows object to be serialized by IView Functions
/// </summary>
public interface IBinaryViewObject : IBinaryViewReadable, IBinaryViewWritable { }
