using System;
using System.Collections.Generic;
using System.Text;

namespace GGL.IO;

/// <summary>
/// Allows object to be serialized by IView Functions
/// </summary>
public interface IViewObject : IViewReadable, IViewWritable { }
