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
        public event EventHandler<DejizoRequestEventArgs> Requested;
        public event EventHandler<DejizoResponseEventArgs> Responsed;

        public IQueryable<DejizoItem> EJdict { get; private set; }

        public DejizoSource()
        {
            var provider = new DejizoProvider();
            provider.Requested += (sender, e) =>
            {
                Requested?.Invoke(this, e);
            };
            provider.Responsed += (sender, e) =>
            {
                Responsed?.Invoke(this, e);
            };
            EJdict = new Query<DejizoItem>(provider);
        }
    }
}
