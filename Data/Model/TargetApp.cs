using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigParser.Data.Model
{
    /// <summary>
    /// WEB = CS + CND
    /// API = CS-API + CND-API
    /// User only specifies CS / CND or CS-API / CND-API.
    /// </summary>
    internal class TargetApp
    {
        private readonly List<string> _appsName;

        public TargetApp(string appName)
        {
            _appsName = new List<string> { appName };
            if(appName.Eql("CS") || appName.Eql("CND"))
            {
                _appsName.Add("WEB");
            }
            if (appName.Eql("CS-API") || appName.Eql("CND-API"))
            {
                _appsName.Add("API");
            }
        }
        public bool CSEqual(string targetApp)
        {
            return _appsName.Any(a => a.Eql(targetApp));
        }

        public bool CSStrictEqual(string targetApp)
        {
            return _appsName[0].Eql(targetApp);
        }


        public bool IsCsSpecific()
        {
            return IsWeb() || IsApi();
        }

        public bool IsWeb()
        {
            return _appsName.Last().Eql("WEB");

        }

        public bool IsApi()
        {
            return _appsName.Last().Eql("API");
        }

        public override string ToString()
        {
            return _appsName[0];
        }
    }
}
