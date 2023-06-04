using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Arithmic;
using Charm.Objects;
using Tiger;
using Tiger.Schema;

namespace Charm
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Get all classes that inherit from StrategistSingleton<>
            // Then call RegisterEvents() on each of them
            List<Type> allStrategistSingletons = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Select(t => t.GetNonGenericParent(typeof(Strategy.StrategistSingleton<>)))
                .Where(t => t != null)
                .ToHashSet().ToList();

            // order dependencies from InitializesAfterAttribute
            allStrategistSingletons = SortByInitializationOrder(allStrategistSingletons).ToList();

            foreach (Type strategistSingleton in allStrategistSingletons)
            {
                strategistSingleton.GetMethod("RegisterEvents", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            }

            Log.Info("Initialising Charm subsystems");
            string[] args = Environment.GetCommandLineArgs();
            CharmInstance.Args = new CharmArgs(args);
            CharmInstance.InitialiseSubsystems();
            Log.Info("Initialised Charm subsystems");

            Strategy.SetStrategy(TigerStrategy.DESTINY2_LATEST);

            if (Commandlet.RunCommandlet())
            {
                Environment.Exit(0);
            }
        }

        public static IEnumerable<Type> SortByInitializationOrder(IEnumerable<Type> types)
        {
            var dependencyMap = new Dictionary<Type, List<Type>>();
            var dependencyCount = new Dictionary<Type, int>();

            // Build dependency map and count dependencies
            foreach (var type in types)
            {
                var attributes = type.GenericTypeArguments[0].GetCustomAttributes(typeof(InitializeAfterAttribute), true);
                foreach (InitializeAfterAttribute attribute in attributes)
                {
                    var dependentType = attribute.TypeToInitializeAfter.GetNonGenericParent(
                        typeof(Strategy.StrategistSingleton<>));
                    if (!dependencyMap.ContainsKey(dependentType))
                    {
                        dependencyMap[dependentType] = new List<Type>();
                        dependencyCount[dependentType] = 0;
                    }
                    dependencyMap[dependentType].Add(type);
                    dependencyCount[type] = dependencyCount.ContainsKey(type) ? dependencyCount[type] + 1 : 1;
                }
            }

            // Perform topological sorting
            var sortedTypes = types.Where(t => !dependencyCount.ContainsKey(t)).ToList();
            var queue = new Queue<Type>(dependencyMap.Keys.Where(k => dependencyCount[k] == 0));
            while (queue.Count > 0)
            {
                var type = queue.Dequeue();
                sortedTypes.Add(type);

                if (dependencyMap.ContainsKey(type))
                {
                    foreach (var dependentType in dependencyMap[type])
                    {
                        dependencyCount[dependentType]--;
                        if (dependencyCount[dependentType] == 0)
                        {
                            queue.Enqueue(dependentType);
                        }
                    }
                }
            }

            if (sortedTypes.Count < types.Count())
            {
                throw new InvalidOperationException("Circular dependency detected.");
            }

            return sortedTypes;
        }
    }
}
