using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region LocalizedCliCommand

    // used to test that the properties in the base class are also localized when the derived class is localized by using `nameof` operator with resource properties in the attributes.
    //it is located in separate file to be sure  that issue#71 is fixed
    internal class LocalizedCliBaseCommand
    {
        [CliOption(Description = nameof(TestResources.BaseOptionDescription))]
        public string BaseOption { get; set; } = "DefaultForBaseOption";
       
    }

    #endregion
}
