using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DirectXTexNet;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Tiger;
using Tiger.Schema;
using Tiger.Schema.Shaders;
using Tiger.Schema.Static;
using Blob = Tiger.Blob;
using Device = SharpDX.Direct3D11.Device;
using Point = System.Windows.Point;
using RegisterComponentType = Tiger.Schema.RegisterComponentType;
using Vector4 = System.Numerics.Vector4;

namespace AtlasSharp;



/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        InitTiger();

        AtlasView.Loaded += (sender, args) => {
            // uint staticHash = 0x80bce840; // 40E8BC80
            // uint staticHash = 0x80bce912; // 12e9bc80
            string staticHash = "3BC0DE80";
            AtlasView.LoadStatic(new FileHash(staticHash));};
    }

    private void InitTiger()
    {
        HashSet<Type> lazyStrategistSingletons = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Select(t => t.GetNonGenericParent(typeof(Strategy.LazyStrategistSingleton<>)))
            .Where(t => t is { ContainsGenericParameters: false })
            .Select(t => t.GetNonGenericParent(typeof(Strategy.StrategistSingleton<>)))
            .ToHashSet();

        // Get all classes that inherit from StrategistSingleton<>
        // Then call RegisterEvents() on each of them
        HashSet<Type> allStrategistSingletons = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Select(t => t.GetNonGenericParent(typeof(Strategy.StrategistSingleton<>)))
            .Where(t => t is { ContainsGenericParameters: false })
            .ToHashSet();

        allStrategistSingletons.ExceptWith(lazyStrategistSingletons);

        // order dependencies from InitializesAfterAttribute
        List<Type> strategistSingletons = SortByInitializationOrder(allStrategistSingletons.ToList()).ToList();

        foreach (Type strategistSingleton in strategistSingletons)
        {
            strategistSingleton.GetMethod("RegisterEvents", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        }

        string[] args = Environment.GetCommandLineArgs();
        CharmInstance.Args = new CharmArgs(args);
        CharmInstance.InitialiseSubsystems();
    }

    private static IEnumerable<Type> SortByInitializationOrder(IEnumerable<Type> types)
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

