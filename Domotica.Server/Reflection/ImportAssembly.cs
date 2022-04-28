#nullable enable
using System.Reflection;
using Serilog;

namespace Domotica.Server.Reflection
{
    public sealed class ImportAssembly
    {
        /// <summary>
        /// Execute a method in the assembly dll
        /// </summary>
        public Method? Method { get; set; }

        public Assembly? Assembly { get; set; }

        public string? ExecPath { get; set; }

        public string? Name { get; set; }

        public string? ClassName { get; set; }

        /// <summary>
        /// True: id assembly is loaded
        /// </summary>
        public bool IsLoaded { get; set; }

        /// <summary>
        /// Import Assembly and take the specified parameters
        /// </summary>
        /// <param name="assemblyPath">Path where assembly resides</param>
        /// <param name="assemblyName">Name of assembly</param>
        /// <param name="className">Name of class</param>
        /// <param name="ctorParams">Constructor parameters</param>
        /// <exception cref="ArgumentNullException">All parameter have to be given</exception>
        /// <exception cref="DllNotFoundException">If no assembly DLL is found</exception>
        public ImportAssembly(
            string? assemblyPath,
            string? assemblyName,
            string? className,
            object?[]? ctorParams = null)
        {
            try
            {
                CheckParameters(assemblyPath, assemblyName, className);
                Assembly = LoadAssembly(assemblyPath, assemblyName, className, ctorParams);
            }
            catch (Exception e)
            {
                var errMsg = $@"ImportAssembly failed: {e}";

                Log.Error(errMsg);
                throw new DllNotFoundException(errMsg);
            }
        }

        /// <summary>
        /// Import Assembly and take the specified parameters
        /// Path is set to execution path read from: Environment.ProcessPath
        /// </summary>
        /// <param name="assemblyName">Name of assembly</param>
        /// <param name="className">Name of class</param>
        /// <param name="ctorParams">Constructor parameters</param>
        /// <exception cref="DllNotFoundException">If no assembly DLL is found</exception>
        public ImportAssembly(
            string? assemblyName,
            string? className,
            object?[]? ctorParams = null)
        {
            try
            {
                var assemblyPath = Path.GetDirectoryName(
                    Assembly.GetExecutingAssembly().Location);
                
                if (string.IsNullOrEmpty(assemblyPath))
                    throw new FileNotFoundException($"ImportAssembly: import failed: {assemblyPath}");

                CheckParameters(assemblyPath, assemblyName, className);
                Assembly = LoadAssembly(assemblyPath, assemblyName, className, ctorParams);
            }
            catch (Exception e)
            {
                var errMsg = $@"ImportAssembly failed: {e}";

                Log.Error(errMsg);
                throw new DllNotFoundException(errMsg);
            }
        }

        /// <summary>
        /// All parameters needed -> so check if present
        /// </summary>
        /// <param name="assemblyPath">Path to assembly Dll</param>
        /// <param name="assemblyName">Used Assembly name</param>
        /// <param name="className">Used Class name</param>
        /// <exception cref="ArgumentNullException">If one parameter is missing - throw exception</exception>
        private void CheckParameters(string? assemblyPath, string? assemblyName, string? className)
        {
            ExecPath = assemblyPath ?? throw new ArgumentNullException(nameof(assemblyPath));
            Name = assemblyName ?? throw new ArgumentNullException(nameof(assemblyName));
            ClassName = className ?? throw new ArgumentNullException(nameof(className));
        }

        /// <summary>
        /// Try to load assembly
        /// </summary>
        /// <param name="assemblyPath">Path were assembly resides</param>
        /// <param name="assemblyName">Assembly name</param>
        /// <param name="className">Assembly class to be imported</param>
        /// <param name="ctorParams">Class constructor parameters</param>
        /// <returns>Loaded Assembly instance</returns>
        private Assembly LoadAssembly(string? assemblyPath, string? assemblyName, string? className, object?[]? ctorParams)
        {
            // load assembly
            var assembly = Assembly.LoadFrom($@"{assemblyPath}\{assemblyName}.dll");
            var type = assembly.GetType($@"{assemblyName}.{className}");

            if (type == null) return assembly;

            // create assembly constructor instance with constructor parameters
            var classInstance = Activator.CreateInstance(type, ctorParams);

            // instantiate method
            Method = new Method(classInstance, type);
            IsLoaded = true;

            return assembly;
        }
    }

    public sealed class Method
    {
        private readonly object? _classInstance;

        public string? Name { get; set; }

        public bool IsInitialized { get; set; }

        private Type? Type { get; set; }

        private Type[]? Types { get; set; }

        private object?[]? MethodParams { get; set; }

        /// <summary>
        /// Fluent part for ImportAssembly: instantiate method
        /// </summary>
        /// <param name="classInstance">Instance of class</param>
        /// <param name="type">Type of class</param>
        public Method(object? classInstance, Type type)
        {
            _classInstance = classInstance;
            Type = type;
        }

        /// <summary>
        /// Execute method from imported assembly and class instance
        /// </summary>
        /// <param name="methodName">Take this method</param>
        /// <param name="types">Define method types</param>
        /// <param name="methodParams">Define method parameters</param>
        /// <returns>return value or object</returns>
        /// <exception cref="Exception"></exception>
        public object? Execute(string methodName, Type[]? types = null, object?[]? methodParams = null)
        {
            Name = methodName;
            Types = types;
            MethodParams = methodParams;

            var methodInfo = types == null ? 
                Type?.GetMethod(methodName) : 
                Type?.GetMethod(methodName, types);
            
            if (methodInfo == null)
                throw new Exception($"Method not found: {methodName}.");

            IsInitialized = true;

            return methodInfo.Invoke(_classInstance, methodParams);
        }

        /// <summary>
        /// Execute method from imported assembly and class instance
        /// </summary>
        /// <param name="methodParams">one to many parameters</param>
        /// <returns>return value or object</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public object? Execute(object?[]? methodParams = null)
        {
            MethodParams = methodParams;

            // if parameters are present - execute
            if (!IsInitialized)
                throw new ArgumentException($"IsInitialized state: {IsInitialized}");

            // cla
            if (Name == null)
                throw new Exception($"Method name not set: {Name}.");

            var methodInfo = Types == null ? 
                Type?.GetMethod(Name) : 
                Type?.GetMethod(Name, Types);
            
            if (methodInfo == null)
                throw new Exception($"Method not found: {Name}.");

            return methodInfo.Invoke(_classInstance, MethodParams);
        }
    }
}
