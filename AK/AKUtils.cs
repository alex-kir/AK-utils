﻿
//#define XAMARIN_FORMS

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
using System.Runtime.CompilerServices;

#if XAMARIN_FORMS
using Xamarin.Forms;
#endif

internal static class AKUtils
{
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

    #region System.Collections.IEnumerable<T>

    public static string JoinStrings<T>(this IEnumerable<T> self, string separator = "")
    {
        return string.Join(separator, self.Select(it => it + ""));
    }

    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> self)
    {
        return self.OrderBy(it => it);
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

    public static int Median(this IEnumerable<int> self)
    {
        var pp = self.OrderBy(it => it).ToArray();
        int i = pp.Length / 2;
        int j = (pp.Length - 1) - i;
        return (pp[i] + pp[j]) / 2;
    }

    #endregion

    #region System.ComponentModel.INotifyPropertyChanged (ViewModel)

    //Example:    
    //class ViewModel : INotifyPropertyChanged {
    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private bool isSelected;
    //    public bool IsSelected { get { return isSelected; } set { AKUtils.Setter(ref isSelected, value, PropertyChanged, this, "IsSelected"); } }
    //}
    public static void Setter<T>(ref T field, T value, PropertyChangedEventHandler action, object sender, params string [] names)
    {
        if (!object.Equals(field, value))
        {
            field = value;
            if (action != null)
                foreach(var name in names)
                    action(sender, new PropertyChangedEventArgs(name));
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

    #region System.String

    public static bool IsNullOrEmpty(this string self)
    {
        return string.IsNullOrEmpty(self);
    }

    public static string Format(this string self, params object [] args)
    {
        return string.Format(self, args);
    }

    public static string F(this string self, params object [] args)
    {
        return string.Format(self, args);
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

    public static Task WithCancellableAwaiter(this Task task, CancellationToken token)
    {
        return task.ContinueWith(t => t.GetAwaiter().GetResult(), token);
    }

    public static Task<T> WithCancellableAwaiter<T>(this Task<T> task, CancellationToken token)
    {
        return task.ContinueWith(t => t.GetAwaiter().GetResult(), token);
    }

    public static void DoNotAwait(this Task task)
    {
    }

    public class TaskSchedulerAwaiter : INotifyCompletion
    {
        readonly TaskScheduler scheduler;
        public TaskSchedulerAwaiter(TaskScheduler scheduler)
        {
            this.scheduler = scheduler;
        }

        public bool IsCompleted
        { 
            get { return false; }
        }

        public void OnCompleted(Action continuation)
        {
            var task = new Task(continuation);
            task.Start(scheduler);
        }

        public void GetResult() { }
    }

    public static TaskSchedulerAwaiter GetAwaiter(this TaskScheduler self)
    {
        return new TaskSchedulerAwaiter(self);
    }

    #endregion

#if XAMARIN_FORMS
    
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
        self.Add(ret = new Image().SetResource(name), x, y, w, h);
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

    public static void OnItemTapped<T>(this ListView self, Action<T> action)
    {
        self.ItemTapped += (sender, e) => {
            try {
                if (e.Item != null)
                    action((T)e.Item);
                ((ListView)sender).SelectedItem = null;
            }
            catch (Exception ex) {
                ex.LogException();
            }
        };
    }

    public static ImageSource CompleteImageSource(string resourceName)
    {
        return ImageSource.FromResource("railwayUA.Images." + resourceName);
    }

    #endregion

#endif

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
            int baseStackIndex = OnPlatform(1, 6, 3);
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

            OnPlatform(
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

    #region Math

    public static class Math
    {
        public static double Lerp(double p1, double p2, double t01)
        {
            return p1 + (p2 - p1) * t01;
        }

        public static float Lerp(float p1, float p2, float t01)
        {
            return p1 + (p2 - p1) * t01;
        }

        public static double Lerp(double y1, double y2, double x1, double x2, double x)
        {
            return y1 + (y2 - y1) * ((x - x1) / (x2 - x1));
        }

        public static float Lerp(float y1, float y2, float x1, float x2, float x)
        {
            return y1 + (y2 - y1) * ((x - x1) / (x2 - x1));
        }

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

    public static void OnPlatform(Action ios,Action android, Action wp)
    {
        if (_Mono_Android_Android_Util_Log_Debug_string_string != null) {
            android();
        }
        else
            throw new NotImplementedException();
    }

    public static T OnPlatform<T>(T ios, T android, T wp)
    {
        T ret = default(T);
        OnPlatform(() => ret = ios, () => ret = android, () => ret = wp);
        return ret;
    }


    #endregion

    #region Resources

    public static string LoadRawString(string resourceName, string assemblyName = null)
    {
        Assembly assembly;
        if (assemblyName == null) {
            assembly = typeof(AKUtils).GetTypeInfo().Assembly;
            var names = assembly.GetManifestResourceNames();
            assemblyName = assembly.GetName().Name;
        }
        else
        {
            assembly = Assembly.Load(new AssemblyName(assemblyName));
        }

        Stream stream = assembly.GetManifestResourceStream(assemblyName + "." + resourceName);

        using (var reader = new System.IO.StreamReader(stream))
        {
            return reader.ReadToEnd();
        }
    }

    #endregion
}

