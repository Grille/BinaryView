using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GGL.IO;
public interface ILengthPrefix : IViewObject
{
    public long Length { get; set; }
}
