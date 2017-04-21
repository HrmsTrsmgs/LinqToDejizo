﻿using Marimo.LinqToDejizo;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Marimo.LinqToDejizo
{
    public class DejizoSource
    {
        public IQueryable<DejizoItem> EJdict { get; set; } = new Query<DejizoItem>(new DejizoProvider());
    }
}
