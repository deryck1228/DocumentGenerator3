﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.TemplateData
{

    public interface ITemplateLocation
    {
        public string service { get; set; }
        public string template_type { get; set; }
    }
}
