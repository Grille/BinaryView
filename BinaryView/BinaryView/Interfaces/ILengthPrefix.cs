using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grille.IO.Interfaces;
public interface ILengthPrefix : IBinaryViewObject
{
    public long Length { get; set; }
}
