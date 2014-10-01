using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Le_Fluffie
{
    class LFPlugIn
    {
        [CompilerGenerated]
        public string Name = "";
        [CompilerGenerated]
        public ConstructorInfo xConst = null;
        [CompilerGenerated]
        public string Identifier = "";

        public bool valid { get { return xConst != null; } }

        public LFPlugIn(string path)
        {
            Assembly loadedasm = null;
            try
            {
                loadedasm = Assembly.LoadFrom(path);
                if (loadedasm == null)
                    return;
            }
            catch { return; }
            try
            {
                string asmname = loadedasm.GetName().Name;
                Module mod = loadedasm.GetModule(asmname + ".dll");
                Type basetype = mod.GetType(asmname.Replace(' ', '_') + "._Default");
                xConst = basetype.GetConstructor(new Type[] { typeof(X360.STFS.STFSPackage), typeof(System.Windows.Forms.Form)});
                Name = asmname;
            }
            catch { }
            FileStream[] xStreams = loadedasm.GetFiles();
            for (int i = ((xConst == null) ? 0 : 1); i < xStreams.Length; i++)
                xStreams[i].Dispose();
        }
    }
}
