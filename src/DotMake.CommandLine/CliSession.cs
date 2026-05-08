using System;
using System.Text;
using DotMake.CommandLine.Util;

namespace DotMake.CommandLine
{
    internal class CliSession : IDisposable
    {
        private readonly CliSettings settings;

        public CliSession(CliSettings settings)
        {
            this.settings = settings;

            if (settings.Output == null) //Console.Out is being used
            {
                //Important, encoding must be set before accessing Console.Out (there seems to be some kind of delay?),
                //otherwise it's not updated until next execution of the app.
                //Related: https://stackoverflow.com/questions/45513075/why-does-checking-the-console-outputencoding-take-so-long
                ConsoleExtensions.SetOutputEncoding(Encoding.UTF8);
            }

            var cliWriter = CliWriter.GetCached(settings.Output ?? Console.Out);
            cliWriter.SetStyle(settings.Theme.DefaultStyle);

            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            //Not called much but just in case
            //https://stackoverflow.com/a/20676074

            Dispose();
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;

            var cliWriter = CliWriter.GetCached(settings.Output ?? Console.Out);
            cliWriter.ResetStyle();
        }
    }
}
