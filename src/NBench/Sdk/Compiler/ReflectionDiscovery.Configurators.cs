using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace NBench.Sdk.Compiler
{
    public partial class ReflectionDiscovery
    {
        public static readonly Type MeasurementConfiguratorType = typeof(IMeasurementConfigurator<>);

        /// <summary>
        /// Cache of <see cref="MeasurementAttribute"/> types and their "best fitting" <see cref="IMeasurementConfigurator"/> type
        /// </summary>
        private readonly ConcurrentDictionary<Type, Type> _measurementConfiguratorTypes = new ConcurrentDictionary<Type, Type>();
        private static readonly Lazy<IReadOnlyList<Type>> AllConfigurators = new Lazy<IReadOnlyList<Type>>(() => LoadAllTypeConfigurators().ToList(), true);

        // force the lazy loading of all configurators; should only happen the first time a ReflectionDiscovery class is instantiated
        private readonly IReadOnlyList<Type> _configurators = AllConfigurators.Value;

        /// <summary>
        /// Finds a matching <see cref="IMeasurementConfigurator{T}"/> type for a given type of <see cref="MeasurementAttribute"/>
        /// </summary>
        /// <param name="measurementType">A type of <see cref="MeasurementAttribute"/></param>
        /// <param name="specificAssembly">
        ///     Optional parameter. If an <see cref="Assembly"/> is provided, we limit our search 
        ///     for <see cref="IMeasurementConfigurator{T}"/> definitions to just that target assembly.
        /// </param>
        /// <returns>A corresponding <see cref="IMeasurementConfigurator{T}"/> type</returns>
        public Type GetConfiguratorTypeForMeasurement(Type measurementType, Assembly specificAssembly = null)
        {
            ValidateTypeIsMeasurementAttribute(measurementType);

            // served up the cached version if we already have it
            if (_measurementConfiguratorTypes.ContainsKey(measurementType))
                return _measurementConfiguratorTypes[measurementType];

            // search for a match
            var match = FindBestMatchingConfiguratorForMeasurement(measurementType, 
                specificAssembly == null ? _configurators : _configurators.Where(x=> x.Assembly.Equals(specificAssembly)));

            // cache the result
            _measurementConfiguratorTypes[measurementType] = match;

            return match;
        }

        /// <summary>
        /// Creates a <see cref="IMeasurementConfigurator"/> instance for the provided <see cref="MeasurementAttribute"/> type.
        /// </summary>
        /// <param name="measurementType">A type of <see cref="MeasurementAttribute"/></param>
        /// <param name="specificAssembly">
        ///     Optional parameter. If an <see cref="Assembly"/> is provided, we limit our search 
        ///     for <see cref="IMeasurementConfigurator{T}"/> definitions to just that target assembly.
        /// </param>
        /// <returns>
        ///     If a <see cref="IMeasurementConfigurator"/> type match was found, this method will return a NEW instance of that.
        ///     If no match was found, we return a special case instance of <see cref="MeasurementConfigurator.EmptyConfigurator"/>.
        /// </returns>
        public IMeasurementConfigurator GetConfiguratorForMeasurement(Type measurementType, Assembly specificAssembly = null)
        {
            ValidateTypeIsMeasurementAttribute(measurementType);

            var configuratorType = GetConfiguratorTypeForMeasurement(measurementType, specificAssembly);

            // special case: EmptyConfigurator (no match found)
            if (configuratorType == MeasurementConfigurator.EmptyConfiguratorType)
                return MeasurementConfigurator.EmptyConfigurator.Instance;

            // construct the instance
            return(IMeasurementConfigurator)Activator.CreateInstance(configuratorType);
        }

        public static IEnumerable<Type> LoadAllTypeConfigurators()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<Type> seedTypes = new List<Type>();
            return assemblies.Aggregate(seedTypes,
                (types, assembly) =>
                    types.Concat(
                        assembly.DefinedTypes.Where(
                            y => y.IsClass
                            && !y.IsGenericTypeDefinition
                            && IsValidConfiguratorType(y))));
        }

        public static Type FindBestMatchingConfiguratorForMeasurement(Type measurementType,
            IEnumerable<Type> knownConfigurators)
        {
            IEnumerable<Type> seed = new List<Type>();
            var configuratorsThatSupportType = knownConfigurators.Aggregate(seed,
                (list, type) =>
                {
                    var supportsType = GetConfiguratorInterfaces(type).Any<Type>(y => SupportsType(measurementType, y, false));
                    return supportsType ? list.Concat(new[] { type }) : list;
                }).ToList();

            // short-circuit if we couldn't find any matching types
            if (!configuratorsThatSupportType.Any())
                return MeasurementConfigurator.EmptyConfiguratorType;

            var currentType = measurementType;

            // check base classes first
            while (currentType != typeof(object) && currentType != null)
            {
                foreach (var configurator in configuratorsThatSupportType)
                {
                    if (ConfiguratorSupportsMeasurement(currentType, configurator, true))
                        return configurator;
                }
                currentType = currentType.BaseType; //descend down the inheritance chain
            }

            // check interfaces next
            var interfaces = measurementType.GetTypeInfo().ImplementedInterfaces;
            foreach (var i in interfaces)
            {
                foreach (var configurator in configuratorsThatSupportType)
                {
                    if (ConfiguratorSupportsMeasurement(measurementType, configurator, true))
                        return configurator;
                }
            }

            throw new InvalidOperationException(
                "Code never should have reached this line. Should have found matching type");
        }

        /// <summary>
        /// Determine if a given <see cref="IMeasurementConfigurator"/> type is a match for a
        /// <see cref="MeasurementAttribute"/> type.
        /// </summary>
        /// <param name="measurementType">A <see cref="MeasurementAttribute"/> type.</param>
        /// <param name="expectedConfiguratorType">A <see cref="IMeasurementConfigurator"/> type.</param>
        /// <param name="exact">
        ///     If <c>true</c>, then this method will look for an exact 1:1 type match. 
        ///     If <c>false</c>, which is the default then this method will return <c>true</c>
        ///     when any applicable types are assignable from <see cref="measurementType"/>.
        /// </param>
        /// <returns><c>true</c> if a match was found, <c>false</c> otherwise.</returns>
        public static bool ConfiguratorSupportsMeasurement(Type measurementType,
            Type expectedConfiguratorType, bool exact = false)
        {
            ValidateTypeIsMeasurementAttribute(measurementType);
            Contract.Assert(IsValidConfiguratorType(expectedConfiguratorType), $"{expectedConfiguratorType} must derive from {MeasurementConfiguratorType}");

            var currentType = expectedConfiguratorType;
            while (currentType != typeof(object) && currentType != null)
            {
                var configurators = GetConfiguratorInterfaces(currentType).ToList<Type>();
                if (configurators.Count == 0)
                {
                    currentType = currentType.BaseType;
                    continue; //move down the inheritance chain
                }

                // otherwise, we need to scan each of the IMeasurementConfigurator<> matches and make sure that the
                // type arguments are assignable
                return configurators.Any(configuratorType => SupportsType(measurementType, configuratorType, exact));
            }

            return false;

        }

        private static bool SupportsType(Type measurementType, Type configuratorType, bool exact)
        {
            return configuratorType.GenericTypeArguments.Any(y => exact ? y == measurementType : y.IsAssignableFrom(measurementType));
        }

        /// <summary>
        /// Check if a given <see cref="configuratorType"/> is a valid implementation of <see cref="IMeasurementConfigurator{T}"/>.
        /// </summary>
        /// <param name="configuratorType">The <see cref="Type"/> we're going to test.</param>
        /// <returns>true if <see cref="configuratorType"/> implements <see cref="IMeasurementConfigurator{T}"/>, false otherwise.</returns>
        public static bool IsValidConfiguratorType(Type configuratorType)
        {
            return GetConfiguratorInterfaces(configuratorType).Any();
        }

        private static IEnumerable<Type> GetConfiguratorInterfaces(Type type)
        {
            var genericInterfaceDefinitions = type.GetTypeInfo().ImplementedInterfaces.Where(x => x.IsGenericType);
            return genericInterfaceDefinitions.Where(x => x.GetGenericTypeDefinition() == MeasurementConfiguratorType);
        }

        private static void ValidateTypeIsMeasurementAttribute(Type measurementType)
        {
            Contract.Requires(measurementType != null);
            Contract.Assert(MeasurementAttributeType.IsAssignableFrom(measurementType),
                $"{measurementType} must derive from {MeasurementAttributeType}");
        }
    }
}
