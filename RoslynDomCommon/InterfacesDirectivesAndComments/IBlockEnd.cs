﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoslynDom.Common
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDetailBlockEnd : IDetail<IDetailBlockEnd>
   {
      IDetailBlockStart BlockStart { get;  }
      string BlockStyleName { get; }
   }
}
