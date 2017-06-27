﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Core.Models
{
    public abstract class Journal : IJournal
    {
        string AppId { get; internal set; }
        int EnvironmentId { get; }
        DateTime Datestamp { get; }
        

    }
}
