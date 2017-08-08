using Marimo.LinqToDejizo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Marimo.LinqToAmazonProductAdvertisingApi
{
    public class PaaSource
    {
        public IQueryable<Book> Books { get; }

        public PaaSource(string awsAccessKeyID, string awsSecretKey, string associateTag)
        {
            Books = new Query<Book>(new PaaProvider(awsAccessKeyID, awsSecretKey, associateTag));
        }
    }
    public class Book
    {
        public bool Include(string keyword)
        {
            return true;
        }

        public string Title { get; }
    }
}