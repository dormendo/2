using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Security.Permissions;

namespace ResXFileCodeGeneratorEx.Setup.CustomTool.CustomAction
{
    [RunInstaller(true)]
    public partial class CustomToolInstaller : Installer
    {
        public CustomToolInstaller()
        {
            InitializeComponent();
        }

        [SecurityPermission(SecurityAction.Demand)]
        public override void Install(System.Collections.IDictionary stateSaver)
        {
            base.Install(stateSaver);

            DMKSoftware.CodeGenerators.BaseResXFileCodeGeneratorEx.ComRegisterFunction(null);
        }

        [SecurityPermission(SecurityAction.Demand)]
        public override void Uninstall(System.Collections.IDictionary savedState)
        {
            base.Uninstall(savedState);

            DMKSoftware.CodeGenerators.BaseResXFileCodeGeneratorEx.ComUnregisterFunction(null);
        }
    }
}