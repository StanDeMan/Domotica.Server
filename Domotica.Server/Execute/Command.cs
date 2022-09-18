using System.Dynamic;
using System.Text;
using Domotica.Server.Reflection;
using Hardware;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;

namespace Domotica.Server.Execute
{
    public static class Command
    {
        private static ImportAssembly _imported;
        private static readonly StreamWriter Writer;

        /// <summary>
        /// Constructor:
        /// -> On development machine local path is set to: ./dev/pigpio file
        /// -> On linux the path is set to pigpio deamon: /dev/pigpio
        /// </summary>
        static Command()
        {
            var gpio = Platform.DevicePath;
            var fileStream = new FileInfo(gpio!).OpenWrite();
            Writer = new StreamWriter(fileStream, Encoding.ASCII);
        }

        /// <summary>
        /// Execute command
        /// </summary>
        /// <param name="json">What should be executed is located in the json file</param>
        public static void Execute(string json)
        {
            var parameter = ReadCmd(json);
            var present = HasProperty(parameter, "External");

            var ret = present 
                ? RunExternal(parameter) 
                : (bool) RunInternal(parameter);

            Log.Debug($"Command.Execute: {ret}, {json}");
        }

        /// <summary>
        /// External execution
        /// </summary>
        /// <param name="cmdParams">Command parameter part</param>
        /// <returns>True: if went ok</returns>
        private static object RunExternal(dynamic cmdParams)
        {
            object ret;

            try
            {
                _imported = ImportAssembly(cmdParams);

                ret = (_imported.IsLoaded && !_imported.Method!.IsInitialized
                    ? _imported.Method?.Execute(
                        _imported.Method.Name ?? string.Empty,
                        new[] { typeof(object) },
                        PrepareMethodParams(cmdParams))
                    : _imported.Method?.Execute(
                        PrepareMethodParams(cmdParams)))!;
            }
            catch (Exception e)
            {
                Log.Error($"Command.Execute (RunExternal): {e}");

                return null;
            }

            return ret;
        }

        /// <summary>
        /// Internal execution
        /// </summary>
        /// <param name="parameter">Command property</param>
        /// <returns>True: if went ok</returns>
        private static bool RunInternal(dynamic parameter)
        {
            try
            {
                var cmd = Convert.ToString(parameter?.Command);

                Writer.Write(@$"{cmd}{Environment.NewLine}");
                Writer.Flush();
            }
            catch (Exception e)
            {
                Log.Error($"Command.Execute (RunInternal): {e}");

                return false;
            }

            return true;
        }

        /// <summary>
        /// Prepare params
        /// </summary>
        /// <param name="cmdParams">Dynamic deserialized from json</param>
        /// <returns>Parameter for method</returns>
        private static object[] PrepareMethodParams(dynamic cmdParams)
        {
            // object created from json: method execution parameters
            var param = new object[1];
            param[0] = cmdParams!;

            return param;
        }

        /// <summary>
        /// Import Assembly Dll
        /// </summary>
        /// <param name="cmdParams">Dynamic deserialized from json</param>
        /// <returns>Imported Assembly object</returns>
        private static ImportAssembly ImportAssembly(dynamic cmdParams) => new(
            Convert.ToString(cmdParams.External.Assembly),
            Convert.ToString(cmdParams.External.Class))
        {
            Method =
            {
                Name = Convert.ToString(cmdParams.External.Method)
            }
        };

        /// <summary>
        /// Check if property present
        /// </summary>
        /// <param name="cmd">Search here inside</param>
        /// <param name="name">Search for this property</param>
        /// <returns>True: if found</returns>
        private static bool HasProperty(dynamic cmd, string name)
        {
            return cmd is ExpandoObject
                ? ((IDictionary<string, object>)cmd).ContainsKey(name)
                : (bool)(cmd?.GetType().GetProperty(name) != null);
        }

        /// <summary>
        /// Read command section
        /// </summary>
        /// <param name="json">Read from here</param>
        /// <param name="onlyParams">Take only parameter part for the search</param>
        /// <returns>Json tree part: Params ore complete</returns>
        private static dynamic ReadCmd(string json, bool onlyParams = true)
        {
            dynamic cmd = JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());

            return onlyParams 
                ? cmd?.Params 
                : cmd;
        }
    }
}