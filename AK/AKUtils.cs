
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;


public static class AKUtils
{
    #region Debugging

    public static void Trace(this object self, params object[] others)
    {
#if DEBUG
        LogMessage("#TRACE# ", 2, new[] { self }.Concat(others).JoinStrings(","), null);
#endif
    }

    public static void Trace(params object[] others)
    {
#if DEBUG
        LogMessage("#TRACE# ", 2, others.JoinStrings(","), null);
#endif
    }

    public static Task<T> LogException<T>(this Task<T> self)
    {
#if DEBUG
        self.ContinueWith(task =>
        {
            task.Exception.LogException();
        }, TaskContinuationOptions.OnlyOnFaulted);
#endif
        return self;
    }

    public static Task LogException(this Task self)
    {
#if DEBUG
        self.ContinueWith(task =>
        {
            task.Exception.LogException();
        }, TaskContinuationOptions.OnlyOnFaulted);
#endif
        return self;
    }

#if DEBUG

    private static MethodInfo _Mono_Android_Android_Util_Log_Debug_string_string = FindMethod("Mono.Android", "Android.Util", "Log", "Debug", typeof(string), typeof(string));

    private static ConstructorInfo _mscorlib_System_Diagnostics_StackFrame_int_bool = FindConstructor("mscorlib", "System.Diagnostics", "StackFrame", typeof(int), typeof(bool));
    private static MethodInfo _mscorlib_System_Diagnostics_StackFrame_GetFileName = FindMethod("mscorlib", "System.Diagnostics", "StackFrame", "GetFileName");
    private static MethodInfo _mscorlib_System_Diagnostics_StackFrame_GetFileLineNumber = FindMethod("mscorlib", "System.Diagnostics", "StackFrame", "GetFileLineNumber");
    private static MethodInfo _mscorlib_System_Diagnostics_StackFrame_GetMethod = FindMethod("mscorlib", "System.Diagnostics", "StackFrame", "GetMethod");

    private static PropertyInfo _mscorlib_System_Threading_Thread_CurrentThread = FindProperty("mscorlib", "System.Threading", "Thread", "CurrentThread");
    private static PropertyInfo _mscorlib_System_Threading_Thread_ManagedThreadId = FindProperty("mscorlib", "System.Threading", "Thread", "ManagedThreadId");
    private static PropertyInfo _mscorlib_System_Threading_Thread_Name = FindProperty("mscorlib", "System.Threading", "Thread", "Name");

#endif

    private static void LogMessage(string tag, int stackIndex, string arguments, string message)
    {
#if DEBUG
        try
        {
            int baseStackIndex = Device.OnPlatform(1, 6, 3);
            var frame = _mscorlib_System_Diagnostics_StackFrame_int_bool.Invoke(new object[] { baseStackIndex + stackIndex, true });

            var thread = _mscorlib_System_Threading_Thread_CurrentThread.GetValue(null);

            var managedThreadId = (int)_mscorlib_System_Threading_Thread_ManagedThreadId.GetValue(thread);

            var threadName = "";
            try { threadName = _mscorlib_System_Threading_Thread_Name.GetValue(thread) as string; } catch { }
            var threadInfo = "<" + (managedThreadId).ToString("000") + (string.IsNullOrEmpty(threadName) ? "" : ("=" + threadName)) + ">";
            var dateInfo = "[" + DateTime.Now.ToString("HH:mm:ss,fff") + "]";
            var frameGetFileName = _mscorlib_System_Diagnostics_StackFrame_GetFileName.Invoke(frame, new object[] { });
            var frameGetFileLineNumber = _mscorlib_System_Diagnostics_StackFrame_GetFileLineNumber.Invoke(frame, null);
            var backrefInfo = new string(' ', 32) + " in " + frameGetFileName + ":" + frameGetFileLineNumber;
            var frameMethodObj = _mscorlib_System_Diagnostics_StackFrame_GetMethod.Invoke(frame, null);
            var frameMethod = frameMethodObj as MethodInfo;
            var methodInfo = (frameMethod != null ? frameMethod.DeclaringType.Name + "." + frameMethod.Name : "?.?") + "(" + arguments + ")";
            var msg = threadInfo + " " + dateInfo + " " + methodInfo + " " + message + " " + backrefInfo;

            Device.OnPlatform(
                () => System.Diagnostics.Debug.WriteLine(tag + msg),
                () => _Mono_Android_Android_Util_Log_Debug_string_string.Invoke(null, new object[] { tag, msg }),
                () => System.Diagnostics.Debug.WriteLine(tag + msg)
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            System.Diagnostics.Debug.WriteLine(arguments);
            System.Diagnostics.Debug.WriteLine(message);
        }
#endif
    }


    #endregion

    #region System.Exception

    public static void LogException(this Exception self)
    {
#if DEBUG
        LogMessage("#EXCEPTION#", 2, null, self.ToString());
        var webException = self as WebException;
        if (webException != null)
        {
            using (var reader = new StreamReader(webException.Response.GetResponseStream()))
            {
                LogMessage("#EXCEPTION#", 2, null, reader.ReadToEnd());
            }
        }
#endif
    }

    public static string ExtractMessage(this Exception self)
    {
        string ret = null;
        while (self != null)
        {
            ret = self.Message;
            self = self.InnerException;
        }
        return ret;
    }

    #endregion

    #region IEnumerable<T>

    public static string JoinStrings<T>(this IEnumerable<T> self, string separator = "")
    {
        return string.Join(separator, self.Select(it => it + ""));
    }

    public static void ForEach<T>(this IEnumerable<T> self, Action<int, T> action)
    {
        int i = 0;
        foreach (var item in self)
        {
            action(i, item);
            i++;
        }
    }

    public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
    {
        foreach (var item in self)
        {
            action(item);
        }
    }

    #endregion

    #region System.DateTime

    public static DateTime Truncate(this DateTime dateTime, TimeSpan timeSpan)
    {
        if (timeSpan == TimeSpan.Zero) return dateTime; // Or could throw an ArgumentException
        return dateTime.AddTicks(-(dateTime.Ticks % timeSpan.Ticks));
    }

    #endregion

    #region WPF helpers

    public static void Setter<T>(ref T field, T value, PropertyChangedEventHandler action, object sender, string name)
    {
        if (!object.Equals(field, value))
        {
            field = value;
            if (action != null)
                action(sender, new PropertyChangedEventArgs(name));
        }
    }

    #endregion

    #region System.String

    public static bool IsNullOrEmpty(this string self)
    {
        return string.IsNullOrEmpty(self);
    }

    public static double ToDouble(this string self)
    {
        double d = 0;
        var s = self.Replace(" ", "").Trim();
        if (double.TryParse(s, System.Globalization.NumberStyles.Any, null, out d))
            return d;
        if (double.TryParse(s.Replace(",", "."), out d))
            return d;
        if (double.TryParse(s.Replace(".", ","), out d))
            return d;
        return d;
    }

    #endregion

    #region System.Threading.Tasks

    public static T RunWithDialog<T>(this T self, TaskScheduler uiScheduler, Action showDialog, Action hideDialog, Action<string> showError) where T : Task
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        var token = cts.Token;
        Task.Delay(500, token).ContinueWith(task => {
            showDialog();
            self.ContinueWith(t => {
                hideDialog();
            }, uiScheduler);
        }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, uiScheduler);

        self.ContinueWith(task => {
            cts.Cancel();
            if (task.IsFaulted) {
                task.Exception.LogException();
                showError(task.Exception.ExtractMessage());
            }
        }, uiScheduler);
        return self;
    }

    #endregion

    #region Xamarin.Forms.View

    public static T AddTapAction<T>(this T self, Action execute, bool animateBackground = true) where T : Xamarin.Forms.View
    {
        self.AddTapAction(obj => execute(), animateBackground);
        return self;
    }

    public static T AddTapAction<T>(this T self, Action<object> execute, bool animateBackground = true, TaskScheduler scheduler = null) where T : Xamarin.Forms.View
    {
        var c = self.BackgroundColor;
        if (scheduler == null)
            scheduler = TaskScheduler.FromCurrentSynchronizationContext();

        self.GestureRecognizers.Add(new Xamarin.Forms.TapGestureRecognizer
            {
                Command = new Xamarin.Forms.Command(obj =>
                {
                    if (animateBackground)
                    {
                        self.Animate("clickAnimation", new Animation(dt =>
                        {
                            var ndt = 1 - dt;
                            self.BackgroundColor = new Color(0.5 * ndt + c.R * dt, 0.5 * ndt + c.G * dt, 0.5 * ndt + c.B * dt, 0.5 * ndt + c.A * dt);
                        }));

                        Task.Delay(TimeSpan.FromSeconds(0.2)).ContinueWith(task =>
                        {
                            execute(obj);
                        }, scheduler);
                    }
                    else
                    {
                        self.Animate("clickAnimation", new Animation(dt =>
                        {
                            var ndt = 1 - dt;
                            self.Scale = 0.9 * ndt + 1 * dt;
                        }));

                        Task.Delay(TimeSpan.FromSeconds(0.2)).ContinueWith(task =>
                        {
                            execute(obj);
                        }, scheduler);
                    }
                })
            });
        return self;
    }

    public static void Add(this RelativeLayout.IRelativeList<View> self, View item, Func<RelativeLayout, double> x, Func<RelativeLayout, double> y, Func<RelativeLayout, double> w = null, Func<RelativeLayout, double> h = null)
    {
        self.Add(item, Constraint.RelativeToParent(x), Constraint.RelativeToParent(y), w == null ? null : Constraint.RelativeToParent(w), h == null ? null : Constraint.RelativeToParent(h));
    }

    public static Image AddImage(this RelativeLayout.IRelativeList<View> self, string name, Func<RelativeLayout, double> x, Func<RelativeLayout, double> y, Func<RelativeLayout, double> w = null, Func<RelativeLayout, double> h = null)
    {
        Image ret;
        self.Add(ret = new Image { Source = ImageSource.FromResource("railwayUA.Images." + name) }, x, y, w, h);
        return ret;
    }

    public class ViewPlace
    {
        internal View view;
        internal Func<RelativeLayout, double> x;
        internal Func<RelativeLayout, double> y;
        internal Func<RelativeLayout, double> width;
        internal Func<RelativeLayout, double> height;

        public ViewPlace SetBinding(BindableProperty property, string path)
        {
            view.SetBinding(property, path);
            return this;
        }
    }

    public static ViewPlace Place(this Image self, string source, Func<RelativeLayout, double> x, Func<RelativeLayout, double> y, Func<RelativeLayout, double> w = null, Func<RelativeLayout, double> h = null)
    {
        self.Source = ImageSource.FromResource("railwayUA.Images." + source);
        return new ViewPlace { view = self, x = x, y = y, width = w, height = h };
    }

    public static ViewPlace Place(this View self, Func<RelativeLayout, double> x, Func<RelativeLayout, double> y, Func<RelativeLayout, double> w = null, Func<RelativeLayout, double> h = null)
    {
        return new ViewPlace { view = self, x = x, y = y, width = w, height = h };
    }

    public static RelativeLayout AddChildren(this RelativeLayout self, params ViewPlace[] children)
    {
        foreach (var child in children)
            self.Children.Add(child.view, child.x, child.y, child.width, child.height);
        return self;
    }

    #endregion

    #region Reflection

    public static readonly Type[] EmptyTypes = new Type[0];
    private static Assembly _mscorlib = null;
    private static Assembly [] _assemblies = null;

    //private static Assembly _mscorlib_System = null;

    public static Assembly FindAssembly(string assemblyName)
    {
        if (_mscorlib == null)
        {
            var n = new AssemblyName("mscorlib");
            _mscorlib = Assembly.Load(n);

            var _GetAssemblies = FindMethod(_mscorlib, "System", "AppDomain", "GetAssemblies");
            var _CurrentDomain = FindProperty(_mscorlib, "System", "AppDomain", "CurrentDomain");
            var domain = _CurrentDomain.GetValue(null);
            _assemblies = _GetAssemblies.Invoke(domain, EmptyTypes) as Assembly[];
        }

        return _assemblies.FirstOrDefault(it => it.GetName().Name == assemblyName);
    }

    public static MethodInfo FindMethod(string assemblyName, string namespaceName, string className, string methodName, params Type[] types)
    {
        return FindMethod(FindAssembly(assemblyName), namespaceName, className, methodName, types);
    }

    public static MethodInfo FindMethod(Assembly assembly, string namespaceName, string className, string methodName, params Type[] types)
    {
        try
        {
            var t = assembly.ExportedTypes.FirstOrDefault(it => it.Name == className && it.Namespace == namespaceName);
            var m = t.GetRuntimeMethod(methodName, types);
            //m.Invoke(null, args);
            return m;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
        return null;
    }

    public static ConstructorInfo FindConstructor(string assemblyName, string namespaceName, string className, params Type[] types)
    {
        try
        {
            var n = new AssemblyName(assemblyName); // "mscorlib", "Mono.Android"
            var a = Assembly.Load(n);
            var t = a.ExportedTypes.FirstOrDefault(it => it.Name == className && it.Namespace == namespaceName);
            var ti = t.GetTypeInfo();
            var cc = ti.DeclaredConstructors;
            foreach (var c in cc)
            {
                var pp = c.GetParameters();
                if (pp.Length == types.Length)
                {
                    if (Enumerable.Range(0, pp.Length).All(it => pp[it].ParameterType == types[it]))
                    {
                        return c;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
        return null;
    }

    public static PropertyInfo FindProperty(string assemblyName, string namespaceName, string className, string propertyName)
    {
        return FindProperty(FindAssembly(assemblyName), namespaceName, className, propertyName);
    }

    public static PropertyInfo FindProperty(Assembly assembly, string namespaceName, string className, string propertyName)
    {
        try
        {
            var t = assembly.ExportedTypes.FirstOrDefault(it => it.Name == className && it.Namespace == namespaceName);
            var p = t.GetRuntimeProperty(propertyName);
            return p;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
        return null;
    }

    public static MethodInfo FindInstanceMethod(object instance, string methodName, params Type[] types)
    {
        try
        {
            var t = instance.GetType();
            var m = t.GetRuntimeMethod(methodName, types);
            return m;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
        return null;
    }

    public static PropertyInfo FindInstanceProperty(object instance, string propertyName)
    {
        try
        {
            var t = instance.GetType();
            var p = t.GetRuntimeProperty(propertyName);
            return p;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
        return null;
    }

    //public static PropertyInfo FindProperty(object instance, string propertyName)
    //{
    //    var t = instance.GetType();
    //    var p = t.GetRuntimeProperty(propertyName);
    //    return p;
    //}

    //public static void NativeCallStatic(string className, string methodName, params object[] args)
    //{
    //    var n = new AssemblyName("Mono.Android");
    //    var a = Assembly.Load(n);
    //    var t = a.ExportedTypes.FirstOrDefault(it => it.FullName == className);
    //    var m = t.GetRuntimeMethod(methodName, args.Select(it => it.GetType()).ToArray());
    //    m.Invoke(null, args);
    //}


    //public static object NativeCallConstructor(string assemblyName, string namespaceName, string className, params object[] args)
    //{
    //    var n = new AssemblyName(assemblyName); // "mscorlib", "Mono.Android"
    //    var a = Assembly.Load(n);
    //    var t = a.ExportedTypes.FirstOrDefault(it => it.Name == className && it.Namespace == namespaceName);
    //    var ti = t.GetTypeInfo();
    //    var cc = ti.DeclaredConstructors;
    //    foreach (var c in cc)
    //    {
    //        var pp = c.GetParameters();
    //        if (pp.Length == args.Length)
    //        {
    //            if (Enumerable.Range(0, pp.Length).All(it => pp[it].ParameterType == args[it].GetType()))
    //            {
    //                return c.Invoke(args);
    //            }
    //        }
    //    }
    //    return null;
    //}

    //public static object NativeCallGetterStatic(string assemblyName, string namespaceName, string className, string propertyName)
    //{
    //    var n = new AssemblyName(assemblyName); // "mscorlib", "Mono.Android"
    //    var a = Assembly.Load(n);
    //    var t = a.ExportedTypes.FirstOrDefault(it => it.Name == className && it.Namespace == namespaceName);
    //    var p = t.GetRuntimeProperty(propertyName);
    //    return p.GetValue(null);
    //}

    //public static object NativeCallGetter(object instance, string propertyName)
    //{
    //    var t = instance.GetType();
    //    var p = t.GetRuntimeProperty(propertyName);
    //    return p.GetValue(instance);
    //}

    //public static object NativeCallMethod(object instance, string methodName, params object[] args)
    //{
    //    var t = instance.GetType();
    //    var m = t.GetRuntimeMethod(methodName, args.Select(it => it.GetType()).ToArray());
    //    return m.Invoke(instance, args);
    //}

    #endregion

    #region System.Action, System.Func

    public static void SafeCall(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            ex.LogException();
        }
    }

    public static T SafeCall<T>(T defolt, Func<T> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            ex.LogException();
        }
        return defolt;
    }

    public static async Task<T> SafeCallAsync<T>(Func<Task<T>> action, T defolt)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            ex.LogException();
            return defolt;
        }
    }

    #endregion

    #region Resources

    public static ImageSource CompleteImageSource(string resourceName)
    {
        return ImageSource.FromResource("railwayUA.Images." + resourceName);
    }

    public static string LoadRawString(string resourceName, string assemblyName = null)
    {
        var assembly = typeof(AKUtils).GetTypeInfo().Assembly;
        Stream stream = assembly.GetManifestResourceStream(assembly.GetName() + "." + resourceName);

        using (var reader = new System.IO.StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }

    #endregion
}

