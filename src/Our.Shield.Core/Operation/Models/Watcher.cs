using Our.Shield.Core.Models;
using System;
using System.Text.RegularExpressions;
using System.Web;

namespace Our.Shield.Core.Operation.Models
{
    internal class Watcher
    {
        public int Priority;

        public string AppId;

        public Regex Regex;

        public Func<int, HttpApplication, WatchResponse> Request;
    }
}
